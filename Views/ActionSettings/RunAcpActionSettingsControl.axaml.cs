using System.Collections.Generic;
using System.Linq;
using AgentIsland.Models;
using ClassIsland.Core.Abstractions.Controls;

namespace AgentIsland.Views.ActionSettings;

public partial class RunAcpActionSettingsControl : ActionSettingsControlBase<RunAcpActionSettings>
{
    public RunAcpActionSettingsControl()
    {
        InitializeComponent();
    }

    public IReadOnlyList<string> AgentNames =>
        Plugin.Settings.AcpAgents.Select(x => x.Name).ToList();

    public string EmptyStateText => AgentNames.Count > 0
        ? "将启动所选 Agent 进程。"
        : "当前还没有可用的 ACP Agent，请先到 AgentIsland / ACP 设置中添加。";

    protected override void OnAdded()
    {
        base.OnAdded();

        if (string.IsNullOrWhiteSpace(Settings.AgentName) && AgentNames.Count > 0)
        {
            Settings.AgentName = AgentNames[0];
        }

        ChangeActionName(string.IsNullOrWhiteSpace(Settings.AgentName)
            ? "运行 ACP"
            : $"运行 ACP：{Settings.AgentName}");
        ChangeActionIcon("\uEDE9");
    }
}
