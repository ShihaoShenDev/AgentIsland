using AgentIsland.Mcp;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentIsland;

[PluginEntrance]
public class Plugin : PluginBase, IDisposable
{
    private McpServerManager? _mcpManager;
    private ILogger<Plugin>? _logger;
    private bool _disposed;

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddNotificationProvider<AgentIsland.Mcp.Tools.AgentIslandNotificationProvider>();
        AppBase.Current.AppStarted += OnAppStarted;
        AppBase.Current.AppStopping += OnAppStopping;
    }

    private async void OnAppStarted(object? sender, EventArgs e)
    {
        _logger = IAppHost.GetService<ILogger<Plugin>>();
        _mcpManager = new McpServerManager();

        try
        {
            await _mcpManager.StartAsync();
            _logger?.LogInformation("AgentIsland MCP server started at http://localhost:5943/mcp and http://localhost:5943/sse.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start AgentIsland MCP server.");
        }
    }

    private async void OnAppStopping(object? sender, EventArgs e)
    {
        try
        {
            if (_mcpManager is not null)
            {
                await _mcpManager.StopAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to stop AgentIsland MCP server.");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        AppBase.Current.AppStarted -= OnAppStarted;
        AppBase.Current.AppStopping -= OnAppStopping;
        _mcpManager?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
