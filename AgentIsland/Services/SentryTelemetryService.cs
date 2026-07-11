using System.ComponentModel;
using AgentIsland.Models;
using Sentry;

namespace AgentIsland.Services;

/// <summary>
/// 管理 Sentry SDK 的生命周期并提供遥测 API。
/// 根据用户隐私同意状态和开关实时初始化或关闭 SDK。
/// </summary>
public sealed class SentryTelemetryService : IDisposable
{
    /// <summary>
    /// 默认 Sentry DSN（请替换为实际的 DSN）
    /// </summary>
    public const string DefaultDsn = "https://56ca4edaa5857ec14e6987a98a2d5045@o4510368355909632.ingest.us.sentry.io/4511635030343680";

    private IDisposable? _sentryHandle;
    private readonly AgentIslandSettings _settings;

    public SentryTelemetryService(AgentIslandSettings settings)
    {
        _settings = settings;
        _settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    /// <summary>
    /// 根据当前设置状态决定是否初始化或关闭 Sentry SDK。
    /// </summary>
    public void EvaluateAndApply()
    {
        if (_settings.IsTelemetryActive && _sentryHandle is null)
        {
            Initialize();
        }
        else if (!_settings.IsTelemetryActive && _sentryHandle is not null)
        {
            Shutdown();
        }
    }

    private void Initialize()
    {
        if (string.IsNullOrWhiteSpace(_settings.EffectiveSentryDsn))
        {
            return;
        }

        _sentryHandle = SentrySdk.Init(options =>
        {
            options.Dsn = _settings.EffectiveSentryDsn;
            options.Debug = false;
            options.TracesSampleRate = 1.0;
            options.SendDefaultPii = false;
            options.AutoSessionTracking = false;
            options.SetBeforeSend(sentryEvent =>
            {
                sentryEvent.SetTag("plugin", "AgentIsland");
                return sentryEvent;
            });
        });

        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("classisland.plugin", "AgentIsland");
        });

        AddBreadcrumb("Sentry telemetry initialized", "plugin.lifecycle", BreadcrumbLevel.Info);
    }

    private void Shutdown()
    {
        _sentryHandle?.Dispose();
        _sentryHandle = null;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AgentIslandSettings.IsTelemetryEnabled)
            or nameof(AgentIslandSettings.HasAgreedToPrivacyPolicy)
            or nameof(AgentIslandSettings.CustomSentryDsn))
        {
            // DSN 变化时需要重新初始化（先关再开）
            if (e.PropertyName == nameof(AgentIslandSettings.CustomSentryDsn) && _sentryHandle is not null)
            {
                Shutdown();
            }
            EvaluateAndApply();
        }
    }

    /// <summary>
    /// 捕获异常并上报到 Sentry。
    /// </summary>
    public void CaptureException(Exception exception, string? context = null)
    {
        if (_sentryHandle is null)
        {
            return;
        }

        SentrySdk.CaptureException(exception, scope =>
        {
            if (context is not null)
            {
                scope.SetExtra("context", context);
            }
        });
    }

    /// <summary>
    /// 添加面包屑事件。
    /// </summary>
    public void AddBreadcrumb(string message, string category, BreadcrumbLevel level = BreadcrumbLevel.Info)
    {
        if (_sentryHandle is null)
        {
            return;
        }

        SentrySdk.AddBreadcrumb(message, category, level: level);
    }

    /// <summary>
    /// 包裹同步操作，自动添加 Transaction、面包屑和异常捕获。
    /// </summary>
    public T? WithInstrumentation<T>(string name, Func<T> action)
    {
        if (_sentryHandle is null)
        {
            return action();
        }

        SentrySdk.AddBreadcrumb($"Tool call: {name}", "mcp.tool", level: BreadcrumbLevel.Info);
        var transaction = SentrySdk.StartTransaction(name, "mcp.tool.call");
        try
        {
            var result = action();
            transaction.Finish(SpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex, scope => scope.SetExtra("tool", name));
            transaction.Finish(SpanStatus.InternalError);
            throw;
        }
    }

    /// <summary>
    /// 包裹异步操作，自动添加 Transaction、面包屑和异常捕获。
    /// </summary>
    public async Task<T?> WithInstrumentationAsync<T>(string name, Func<Task<T>> action)
    {
        if (_sentryHandle is null)
        {
            return await action();
        }

        SentrySdk.AddBreadcrumb($"Tool call: {name}", "mcp.tool", level: BreadcrumbLevel.Info);
        var transaction = SentrySdk.StartTransaction(name, "mcp.tool.call");
        try
        {
            var result = await action();
            transaction.Finish(SpanStatus.Ok);
            return result;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex, scope => scope.SetExtra("tool", name));
            transaction.Finish(SpanStatus.InternalError);
            throw;
        }
    }

    public void Dispose()
    {
        _settings.PropertyChanged -= OnSettingsPropertyChanged;
        Shutdown();
    }
}
