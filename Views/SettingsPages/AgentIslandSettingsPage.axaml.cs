using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Shared;
using AgentIsland.Models;

namespace AgentIsland.Views.SettingsPages
{
    /// <summary>
    /// AgentIsland 插件设置页面
    /// </summary>
    [SettingsPageInfo(
        id: "agentisland.settings",
        name: "AgentIsland 设置",
        category: SettingsPageCategory.External
    )]
    public partial class AgentIslandSettingsPage : SettingsPageBase
    {
        public AgentIslandSettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            DataContext = Plugin.Settings;
        }
    }
}
