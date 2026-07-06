using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions;
using ClassIsland.Shared;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using DotNetCampus.ModelContextProtocol.Protocol.Messages;
using DotNetCampus.ModelContextProtocol.Servers;
using Microsoft.Extensions.Logging;
using Sentry;

namespace AgentIsland.Mcp.Tools;

public sealed class EchoCaveTool : IMcpServerTool
{
    private static readonly Random Rng = new();
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new { },
        required = Array.Empty<string>()
    });

    public string ToolName => "get_echo_cave";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext)
    {
        return new Tool
        {
            Name = ToolName,
            Title = "Echo Cave",
            Description = "从回声洞中随机抽取一条内容返回。",
            InputSchema = InputSchema,
            OutputSchema = null,
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = true,
                DestructiveHint = false,
                IdempotentHint = false,
                OpenWorldHint = false
            }
        };
    }

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: get_echo_cave", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            var _logger = IAppHost.GetService<ILogger<EchoCaveTool>>();
            _logger?.LogDebug("调用 get_echo_cave");

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "echo-cave.txt");
            if (!File.Exists(filePath))
            {
                return ValueTask.FromResult(CallToolResult.FromResultStructured(
                    new EchoCaveResult("回声洞文件不存在。"),
                    AgentIslandJsonContext.Default.EchoCaveResult));
            }

            string[] lines = File.ReadAllLines(filePath)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (lines.Length == 0)
            {
                return ValueTask.FromResult(CallToolResult.FromResultStructured(
                    new EchoCaveResult("回声洞是空的。"),
                    AgentIslandJsonContext.Default.EchoCaveResult));
            }

            string line = lines[Rng.Next(lines.Length)];

            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new EchoCaveResult(line),
                AgentIslandJsonContext.Default.EchoCaveResult));
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "get_echo_cave");
            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new EchoCaveResult($"回声洞出错了: {ex.Message}"),
                AgentIslandJsonContext.Default.EchoCaveResult));
        }
    }
}
