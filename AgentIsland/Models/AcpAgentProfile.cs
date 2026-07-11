using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentIsland.Models;

/// <summary>
/// ACP Agent 的基础配置。
/// </summary>
public class AcpAgentProfile : ObservableObject
{
    private string _name = "新 ACP Agent";
    private string _command = "";
    private bool _isEnabled = true;
    private string _status = "未连接";

    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [JsonPropertyName("command")]
    public string Command
    {
        get => _command;
        set => SetProperty(ref _command, value);
    }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    [JsonPropertyName("status")]
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
