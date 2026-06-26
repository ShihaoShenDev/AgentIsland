using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentIsland.Models;

/// <summary>
/// "运行 ACP" 自动化动作的设置。
/// </summary>
public class RunAcpActionSettings : ObservableObject
{
    private string _agentName = string.Empty;
    private bool _showNotification = true;
    private string _customPayload = string.Empty;

    [JsonPropertyName("agentName")]
    public string AgentName
    {
        get => _agentName;
        set => SetProperty(ref _agentName, value);
    }

    [JsonPropertyName("showNotification")]
    public bool ShowNotification
    {
        get => _showNotification;
        set => SetProperty(ref _showNotification, value);
    }

    [JsonPropertyName("customPayload")]
    public string CustomPayload
    {
        get => _customPayload;
        set => SetProperty(ref _customPayload, value);
    }
}
