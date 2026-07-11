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
        name: "MCP 设置",
        category: SettingsPageCategory.External
    )]
    public partial class McpSettingsPage : SettingsPageBase
    {
        private static bool _restartRequested;

        public McpSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DataContext = Plugin.Settings;
            Plugin.Settings.PropertyChanged += OnSettingsPropertyChanged;

            if (_restartRequested)
            {
                RestartBanner.IsOpen = true;
            }
        }

        private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Models.AgentIslandSettings.IsEnabled)
                or nameof(Models.AgentIslandSettings.Port)
                or nameof(Models.AgentIslandSettings.TransportMode))
            {
                RestartBanner.IsOpen = true;
                if (!_restartRequested)
                {
                    _restartRequested = true;
                    RequestRestart();
                }
            }
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
                FileName = "https://my.feishu.cn/wiki/MXrjwyuWRiphTMk81TOc1YeEnUe#share-Hqy8dLYaDo2unlxqfTFcke16nRb",
                UseShellExecute = true
            });
        }
    }
}
