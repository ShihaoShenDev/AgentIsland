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
using Sentry;

namespace AgentIsland;

[PluginEntrance]
public class Plugin : PluginBase, IDisposable
{
    private McpServerManager? _mcpManager;
    private ILogger<Plugin>? _logger;
    private SentryTelemetryService? _telemetry;
    private bool _disposed;

    public static AgentIslandSettings Settings { get; private set; } = new();

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        var path = Path.Combine(PluginConfigFolder, "Settings.json");
        Settings = ConfigureFileHelper.LoadConfig<AgentIslandSettings>(path);
        Settings.PropertyChanged += (_, _) =>
            ConfigureFileHelper.SaveConfig(path, Settings);

        _telemetry = new SentryTelemetryService(Settings);
        _telemetry.EvaluateAndApply();
        _telemetry.AddBreadcrumb("Plugin initialized", "plugin.lifecycle", BreadcrumbLevel.Info);

        services.AddSingleton(Settings);
        services.AddSingleton(_telemetry);
        services.AddSingleton<AcpRunnerService>();
        services.AddNotificationProvider<AgentIsland.Mcp.Tools.AgentIslandNotificationProvider>();
        services.AddComponent<Components.AiTextComponent, Components.AiTextComponentSettingsControl>();
        services.AddSettingsPage<Views.SettingsPages.OverviewSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.McpSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.AcpSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.AiTextSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.TelemetrySettingsPage>();
        services.AddAction<RunAcpAction, Views.ActionSettings.RunAcpActionSettingsControl>();

        AppBase.Current.AppStarted += OnAppStarted;
        AppBase.Current.AppStopping += OnAppStopping;
    }

    private async void OnAppStarted(object? sender, EventArgs e)
    {
        _telemetry?.AddBreadcrumb("App started", "plugin.lifecycle", BreadcrumbLevel.Info);

        if (!Settings.IsEnabled)
        {
            return;
        }

        _logger = IAppHost.GetService<ILogger<Plugin>>();
        _mcpManager = new McpServerManager(IAppHost.GetService<ILogger<McpServerManager>>(), _telemetry);

        try
        {
            await _mcpManager.StartAsync(Settings.Port, Settings.TransportMode);
            var endPoint = Settings.TransportMode == McpTransportMode.Sse ? "sse" : "mcp";
            _logger?.LogInformation($"AgentIsland MCP server started at http://localhost:{Settings.Port}/{endPoint} (mode: {Settings.TransportMode}).");
            _telemetry?.AddBreadcrumb($"MCP server started on port {Settings.Port}", "mcp.lifecycle", BreadcrumbLevel.Info);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start AgentIsland MCP server.");
            _telemetry?.CaptureException(ex, "MCP server start");
        }
    }

    private async void OnAppStopping(object? sender, EventArgs e)
    {
        _telemetry?.AddBreadcrumb("App stopping", "plugin.lifecycle", BreadcrumbLevel.Info);

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
            _telemetry?.CaptureException(ex, "MCP server stop");
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
        _telemetry?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
