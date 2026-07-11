using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentIsland.Models;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;

namespace AgentIsland.Views.SettingsPages
{
    /// <summary>
    /// ACP 设置页面
    /// </summary>
    [SettingsPageInfo(
        id: "agentisland.acp",
        name: "ACP 设置",
        category: SettingsPageCategory.External
    )]
    public partial class AcpSettingsPage : SettingsPageBase
    {
        public AcpSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DataContext = Plugin.Settings;
        }

        private void OnAddAcpAgent(object? sender, RoutedEventArgs e)
        {
            var index = Plugin.Settings.TotalAgentCount + 1;
            Plugin.Settings.AcpAgents.Add(new AcpAgentProfile
            {
                Name = $"ACP Agent {index}",
                Command = "",
                Status = "未连接"
            });
        }

        private void OnRemoveAcpAgent(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AcpAgentProfile agent)
            {
                Plugin.Settings.AcpAgents.Remove(agent);
            }
        }

        private void OnEnableAllAcpAgents(object? sender, RoutedEventArgs e)
        {
            foreach (var agent in Plugin.Settings.AcpAgents)
            {
                agent.IsEnabled = true;
            }
        }

        private void OnDisableAllAcpAgents(object? sender, RoutedEventArgs e)
        {
            foreach (var agent in Plugin.Settings.AcpAgents)
            {
                agent.IsEnabled = false;
            }
        }
    }
}
