using AgentIsland.Mcp.Tools;
using AgentIsland.Models;
using DotNetCampus.ModelContextProtocol.Servers;
using DotNetCampus.ModelContextProtocol.Transports.Http;

namespace AgentIsland.Mcp;

public sealed class McpServerManager : IDisposable
{
    private McpServer? _server;
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public async Task StartAsync(int port, McpTransportMode transportMode)
    {
        if (_server is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();

        var builder = new McpServerBuilder("AgentIsland", "1.0.0")
            .WithTools(tools =>
            {
                tools.WithTool<LessonTools>();
                tools.WithTool<ScheduleTools>();
                tools.WithTool<SwapClassesTool>(new SwapClassesTool());
                tools.WithTool<GetScheduleByDateTool>(new GetScheduleByDateTool());
                tools.WithTool<SendNotificationTool>(new SendNotificationTool());
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
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        if (_server is not null)
        {
            await _server.StopAsync(CancellationToken.None);
            _server = null;
        }

        _cts?.Dispose();
        _cts = null;
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
