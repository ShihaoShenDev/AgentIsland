using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;

namespace AgentIsland.Views.SettingsPages
{
    /// <summary>
    /// MCP 设置页面
    /// </summary>
    [SettingsPageInfo(
        id: "agentisland.mcp",
        name: "AgentIsland / MCP 设置",
        category: SettingsPageCategory.External
    )]
    public partial class McpSettingsPage : SettingsPageBase
    {
        public McpSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DataContext = Plugin.Settings;
        }

        private async void OnCopyConnectionAddress(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string address)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Clipboard is not null)
                {
                    await topLevel.Clipboard.SetTextAsync(address);
                    FlyoutBase.ShowAttachedFlyout(button);
                }
            }
        }

        private void OnOpenHowToConnect(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/ShihaoShenDev/AgentIsland",
                UseShellExecute = true
            });
        }
    }
}
