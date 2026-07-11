using AgentIsland.Mcp.Tools;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AgentIsland.Automation;

[ActionInfo(
    "agentisland.acp.run",
    "运行 ACP",
    "\uEDE9",
    true,
    "AgentIsland")]
public class RunAcpAction : ActionBase<RunAcpActionSettings>
{
    private readonly AgentIslandSettings _pluginSettings;
    private readonly AcpRunnerService _acpRunnerService;
    private readonly ILogger<RunAcpAction>? _logger;

    public RunAcpAction(AgentIslandSettings pluginSettings, AcpRunnerService acpRunnerService, ILogger<RunAcpAction>? logger = null)
    {
        _pluginSettings = pluginSettings;
        _acpRunnerService = acpRunnerService;
        _logger = logger;
    }

    protected override async Task OnInvoke()
    {
        await base.OnInvoke();

        _logger?.LogInformation("运行 ACP 动作被触发。");

        if (!_pluginSettings.IsAcpEnabled)
        {
            _logger?.LogWarning("ACP 功能当前未启用，拒绝执行。");
            throw new InvalidOperationException("ACP 功能当前未启用。");
        }

        if (!_pluginSettings.IsAgentAutomationEnabled)
        {
            _logger?.LogWarning("基于 Agent 的自动化当前未启用，拒绝执行。");
            throw new InvalidOperationException("基于 Agent 的自动化当前未启用。");
        }

        var agent = _pluginSettings.AcpAgents.FirstOrDefault(x =>
            string.Equals(x.Name, Settings.AgentName, StringComparison.Ordinal));

        if (agent is null)
        {
            _logger?.LogWarning($"未找到名为\u201c{Settings.AgentName}\u201d的 ACP Agent。");
            throw new InvalidOperationException($"未找到名为\u201c{Settings.AgentName}\u201d的 ACP Agent。");
        }

        if (!agent.IsEnabled)
        {
            _logger?.LogWarning($"ACP Agent \u201c{agent.Name}\u201d当前已被停用，拒绝执行。");
            throw new InvalidOperationException($"ACP Agent \u201c{agent.Name}\u201d当前已被停用。");
        }

        _logger?.LogInformation($"正在启动 ACP Agent \u201c{agent.Name}\u201d（{agent.Command}）。");

        await _acpRunnerService.RunAgentAsync(
            agent,
            Settings.CustomPayload,
            ActionSet.Guid.ToString(),
            ActionItem.Id,
            InterruptCancellationToken);

        agent.Status = $"上次运行：{DateTime.Now:HH:mm:ss}";
        _logger?.LogInformation($"ACP Agent \u201c{agent.Name}\u201d 已启动。");

        if (_pluginSettings.ShowAutomationNotifications && Settings.ShowNotification)
        {
            AgentIslandNotificationProvider.Instance?.Notify(
                "ACP",
                $"已运行 ACP：{agent.Name}",
                2.0,
                3.0);
        }
    }
}
