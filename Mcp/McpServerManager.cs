using AgentIsland.Mcp.Tools;
using AgentIsland.Models;
using AgentIsland.Services;
using DotNetCampus.ModelContextProtocol.Servers;
using DotNetCampus.ModelContextProtocol.Transports.Http;
using Microsoft.Extensions.Logging;
using Sentry;

namespace AgentIsland.Mcp;

public sealed class McpServerManager : IDisposable
{
    private McpServer? _server;
    private CancellationTokenSource? _cts;
    private bool _disposed;
    private readonly ILogger<McpServerManager>? _logger;
    private readonly SentryTelemetryService? _telemetry;

    public McpServerManager(ILogger<McpServerManager>? logger = null, SentryTelemetryService? telemetry = null)
    {
        _logger = logger;
        _telemetry = telemetry;
    }

    public async Task StartAsync(int port, McpTransportMode transportMode)
    {
        if (_server is not null)
        {
            return;
        }

        _logger?.LogInformation("MCP 服务器启动中，传输模式: {TransportMode}，端口: {Port}", transportMode, port);
        var transaction = _telemetry is not null
            ? SentrySdk.StartTransaction("mcp.server.start", "server")
            : null;

        try
        {
            _cts = new CancellationTokenSource();

            var builder = new McpServerBuilder("AgentIsland", "1.0.0")
                .WithTools(tools =>
                {
                    tools.WithTool<LessonTools>();
                    tools.WithTool<ScheduleTools>();
                    tools.WithTool<SwapClassesTool>(new SwapClassesTool());
                    tools.WithTool<GetScheduleByDateTool>(new GetScheduleByDateTool());
                    tools.WithTool<SendNotificationTool>(new SendNotificationTool());
                    tools.WithTool<SetComponentTextTool>(new SetComponentTextTool());
                })
                .WithJsonSerializer(AgentIslandJsonContext.Default);

            builder = transportMode switch
            {
                McpTransportMode.Sse => builder.WithLocalHostHttp(new LocalHostHttpServerTransportOptions
                {
                    Port = port,
                    EndPoint = "sse",
                    IsCompatibleWithSse = true
                }),
                _ => builder.WithLocalHostHttp(new LocalHostHttpServerTransportOptions
                {
                    Port = port,
                    EndPoint = "mcp",
                    IsCompatibleWithSse = false
                })
            };

            _server = builder.Build();

            await _server.StartAsync(_cts.Token);

            _logger?.LogInformation("MCP 服务器已启动");
            transaction?.Finish(SpanStatus.Ok);
        }
        catch (Exception ex)
        {
            _telemetry?.CaptureException(ex, "MCP server start");
            transaction?.Finish(SpanStatus.InternalError);
            throw;
        }
    }

    public async Task StopAsync()
    {
        _logger?.LogInformation("MCP 服务器停止中");
        var transaction = _telemetry is not null
            ? SentrySdk.StartTransaction("mcp.server.stop", "server")
            : null;

        try
        {
            _cts?.Cancel();
            if (_server is not null)
            {
                await _server.StopAsync(CancellationToken.None);
                _server = null;
            }

            _cts?.Dispose();
            _cts = null;

            _logger?.LogInformation("MCP 服务器已停止");
            transaction?.Finish(SpanStatus.Ok);
        }
        catch (Exception ex)
        {
            _telemetry?.CaptureException(ex, "MCP server stop");
            transaction?.Finish(SpanStatus.InternalError);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
    }
}
