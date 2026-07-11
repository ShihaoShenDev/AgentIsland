using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using FluentAvalonia.UI.Controls;
using Sentry;

namespace AgentIsland.Views.SettingsPages;

/// <summary>
/// 遥测与隐私设置页面
/// </summary>
[SettingsPageInfo(
    id: "agentisland.telemetry",
    name: "遥测与隐私",
    category: SettingsPageCategory.External
)]
public partial class TelemetrySettingsPage : SettingsPageBase
{
    private static bool _restartRequested;

    public TelemetrySettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        DataContext = Plugin.Settings;
        Plugin.Settings.PropertyChanged += OnSettingsPropertyChanged;
        UpdatePrivacyUI();

        if (_restartRequested)
        {
            RestartBanner.IsOpen = true;
        }
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Models.AgentIslandSettings.HasAgreedToPrivacyPolicy)
            or nameof(Models.AgentIslandSettings.CustomSentryDsn))
        {
            UpdatePrivacyUI();
        }

        if (e.PropertyName is nameof(Models.AgentIslandSettings.IsTelemetryEnabled)
            or nameof(Models.AgentIslandSettings.CustomSentryDsn)
            or nameof(Models.AgentIslandSettings.HasAgreedToPrivacyPolicy))
        {
            RestartBanner.IsOpen = true;
            if (!_restartRequested)
            {
                _restartRequested = true;
                RequestRestart();
            }
        }
    }

    private void UpdatePrivacyUI()
    {
        var usingCustom = Plugin.Settings.IsUsingCustomDsn;
        var agreed = Plugin.Settings.HasAgreedToPrivacyPolicy;

        // 横幅切换
        DefaultDsnBanner.IsVisible = !usingCustom;
        CustomDsnBanner.IsVisible = usingCustom;

        // 测试 Sentry 项：Debug 模式或自定义 DSN 时显示
#if DEBUG
        TestSentryExpander.IsVisible = true;
#else
        TestSentryExpander.IsVisible = usingCustom;
#endif

        // 隐私协议状态切换
        if (usingCustom)
        {
            PrivacyStatusText.Text = "已忽略";
            PrivacyActionButton.Content = "同意";
            PrivacyActionButton.IsEnabled = false;
        }
        else
        {
            PrivacyStatusText.Text = agreed ? "已同意" : "未同意";
            PrivacyActionButton.Content = agreed ? "撤回同意" : "同意";
            PrivacyActionButton.IsEnabled = true;
        }
    }

    private async void OnPrivacyActionClick(object? sender, RoutedEventArgs e)
    {
        var agreed = Plugin.Settings.HasAgreedToPrivacyPolicy;

        if (!agreed)
        {
            // 显示同意确认对话框
            var dialog = new ContentDialog
            {
                Title = "同意隐私政策与数据协议",
                Content = "AgentIsland 使用 Sentry 收集匿名遥测数据以改进插件质量。\n\n" +
                          "遥测数据包含：\n" +
                          "• 未处理的异常和崩溃信息\n" +
                          "• MCP 工具调用的性能指标（延迟、成功率）\n" +
                          "• 插件生命周期事件（启动、停止等）\n\n" +
                          "遥测数据不包含：\n" +
                          "• 个人课程信息或课表内容\n" +
                          "• 个人身份信息（IP、主机名等）\n\n" +
                          "数据将由 Sentry 处理，涉及跨境数据传输。" +
                          "您可以随时在设置中撤回同意，撤回后遥测将立即停止。",
                PrimaryButtonText = "同意",
                SecondaryButtonText = "取消"
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Plugin.Settings.HasAgreedToPrivacyPolicy = true;
                Plugin.Settings.IsTelemetryEnabled = true;
                RestartBanner.IsOpen = true;
                if (!_restartRequested)
                {
                    _restartRequested = true;
                    RequestRestart();
                }
            }
        }
        else
        {
            // 显示撤回确认对话框
            var dialog = new ContentDialog
            {
                Title = "撤回隐私协议同意",
                Content = "撤回同意后，遥测数据收集将立即停止。\n\n" +
                          "已上报到 Sentry 的历史数据无法撤回，但后续不再收集新数据。\n\n" +
                          "确定要撤回同意吗？",
                PrimaryButtonText = "撤回同意",
                SecondaryButtonText = "取消"
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Plugin.Settings.HasAgreedToPrivacyPolicy = false;
                Plugin.Settings.IsTelemetryEnabled = false;
                RestartBanner.IsOpen = true;
                if (!_restartRequested)
                {
                    _restartRequested = true;
                    RequestRestart();
                }
            }
        }
    }

    private void OnTestSentryClick(object? sender, RoutedEventArgs e)
    {
        SentrySdk.CaptureMessage("Something went wrong");
    }

    private void OnViewPrivacyPolicy(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://my.feishu.cn/wiki/K4RowGQs8iZ8e4kATH0cP7mynAd",
            UseShellExecute = true
        });
    }

    public void Dispose()
    {
        Plugin.Settings.PropertyChanged -= OnSettingsPropertyChanged;
    }
}
