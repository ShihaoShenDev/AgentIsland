using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;

namespace AgentIsland.Views.SettingsPages
{
    /// <summary>
    /// 概览设置页面
    /// </summary>
    [SettingsPageInfo(
        id: "agentisland.overview",
        name: "AgentIsland / 概览",
        category: SettingsPageCategory.External
    )]
    public partial class OverviewSettingsPage : SettingsPageBase
    {
        public OverviewSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            UpdateSystemStatus();
        }

        private void UpdateSystemStatus()
        {
            var settings = Plugin.Settings;

            McpStatusText.Text = settings.IsEnabled
                ? $"MCP 服务器：已启用 (端口 {settings.Port})"
                : "MCP 服务器：已禁用";

            AcpStatusText.Text = settings.IsAcpEnabled
                ? $"ACP 面板：已启用 ({settings.AcpAgentSummary})"
                : "ACP 面板：已禁用";
        }

        private void OnOpenLink(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }
    }
}
