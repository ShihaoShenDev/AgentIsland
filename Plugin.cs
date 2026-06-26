using System.IO;
using AgentIsland.Automation;
using AgentIsland.Mcp;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using ClassIsland.Shared.Helpers;
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

    public static AgentIslandSettings Settings { get; private set; } = new();

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        var path = Path.Combine(PluginConfigFolder, "Settings.json");
        Settings = ConfigureFileHelper.LoadConfig<AgentIslandSettings>(path);
        Settings.PropertyChanged += (_, _) =>
            ConfigureFileHelper.SaveConfig(path, Settings);

        services.AddSingleton(Settings);
        services.AddSingleton<AcpRunnerService>();
        services.AddNotificationProvider<AgentIsland.Mcp.Tools.AgentIslandNotificationProvider>();
        services.AddSettingsPage<Views.SettingsPages.McpSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.AcpSettingsPage>();
        services.AddAction<RunAcpAction, Views.ActionSettings.RunAcpActionSettingsControl>();

        AppBase.Current.AppStarted += OnAppStarted;
        AppBase.Current.AppStopping += OnAppStopping;
    }

    private async void OnAppStarted(object? sender, EventArgs e)
    {
        if (!Settings.IsEnabled)
        {
            return;
        }

        _logger = IAppHost.GetService<ILogger<Plugin>>();
        _mcpManager = new McpServerManager();

        try
        {
            await _mcpManager.StartAsync(Settings.Port, Settings.TransportMode);
            var endPoint = Settings.TransportMode == McpTransportMode.Sse ? "sse" : "mcp";
            _logger?.LogInformation($"AgentIsland MCP server started at http://localhost:{Settings.Port}/{endPoint} (mode: {Settings.TransportMode}).");
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
