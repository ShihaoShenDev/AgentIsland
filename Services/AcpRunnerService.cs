using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AgentIsland.Models;
using ClassIsland.Shared;
using Sentry;
using Microsoft.Extensions.Logging;

namespace AgentIsland.Services;

/// <summary>
/// 负责通过 ACP stdio 协议与 Agent 通信。
/// </summary>
public class AcpRunnerService : IDisposable
{
    private readonly List<AcpAgentSession> _sessions = [];
    private readonly ILogger<AcpRunnerService>? _logger;
    private bool _disposed;

    public AcpRunnerService(ILogger<AcpRunnerService>? logger = null)
    {
        _logger = logger;
    }

    public async Task RunAgentAsync(
        AcpAgentProfile agent,
        string customPayload,
        string workflowId,
        string actionSetId,
        CancellationToken cancellationToken = default)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb($"ACP agent run: {agent.Name}", "acp.agent", BreadcrumbLevel.Info);

        ArgumentNullException.ThrowIfNull(agent);

        if (string.IsNullOrWhiteSpace(agent.Command))
        {
            throw new InvalidOperationException($"ACP Agent \u201c{agent.Name}\u201d 未配置启动命令。");
        }

        _logger?.LogInformation("启动 ACP Agent: {AgentName}", agent.Name);

        var parts = agent.Command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            throw new InvalidOperationException($"ACP Agent \u201c{agent.Name}\u201d 的启动命令无效。");
        }

        var fileName = parts[0];
        var arguments = parts.Length > 1 ? parts[1] : "";

        _logger?.LogDebug("Agent 命令: {FileName} {Arguments}", fileName, arguments);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        var session = new AcpAgentSession(agent, process);
        _sessions.Add(session);

        await InitializeAcpSessionAsync(session, cancellationToken);
        agent.Status = $"已连接：{DateTime.Now:HH:mm:ss}";

        _logger?.LogInformation("Agent 已启动, SessionId: {SessionId}", session.SessionId);
    }

    private static async Task InitializeAcpSessionAsync(AcpAgentSession session, CancellationToken cancellationToken)
    {
        var initRequest = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                protocolVersion = 1,
                clientCapabilities = new { }
            }
        };

        await SendJsonRpcMessageAsync(session.Process.StandardInput, initRequest, cancellationToken);
        var response = await ReadJsonRpcMessageAsync(session.Process.StandardOutput, cancellationToken);

        if (response is JsonElement json && json.TryGetProperty("result", out _))
        {
            session.IsInitialized = true;
        }
    }

    public async Task SendPromptAsync(
        AcpAgentProfile agent,
        string message,
        CancellationToken cancellationToken = default)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb($"ACP prompt sent to {agent.Name}", "acp.agent", BreadcrumbLevel.Info);

        _logger?.LogDebug("向 Agent {AgentName} 发送 Prompt", agent.Name);

        var session = _sessions.FirstOrDefault(s => s.Agent == agent);
        if (session is null || !session.IsInitialized)
        {
            throw new InvalidOperationException($"ACP Agent \u201c{agent.Name}\u201d 未初始化。");
        }

        var promptRequest = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "session/prompt",
            @params = new
            {
                sessionId = session.SessionId,
                message
            }
        };

        await SendJsonRpcMessageAsync(session.Process.StandardInput, promptRequest, cancellationToken);
    }

    private static async Task SendJsonRpcMessageAsync(
        StreamWriter writer,
        object message,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message);
        await writer.WriteLineAsync(json.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
    }

    private static async Task<JsonElement?> ReadJsonRpcMessageAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        var line = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrEmpty(line))
        {
            return null;
        }

        return JsonSerializer.Deserialize<JsonElement>(line);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger?.LogInformation("关闭所有 ACP 会话");

        foreach (var session in _sessions)
        {
            try
            {
                if (!session.Process.HasExited)
                {
                    session.Process.StandardInput.Close();
                    session.Process.WaitForExit(5000);
                    if (!session.Process.HasExited)
                    {
                        session.Process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "停止 ACP 会话时出错");
            }
            finally
            {
                session.Process.Dispose();
            }
        }

        _sessions.Clear();
        _disposed = true;
    }

    private sealed class AcpAgentSession
    {
        public AcpAgentProfile Agent { get; }
        public Process Process { get; }
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public bool IsInitialized { get; set; }

        public AcpAgentSession(AcpAgentProfile agent, Process process)
        {
            Agent = agent;
            Process = process;
        }
    }
}
