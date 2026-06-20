using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgentIsland.Models;

/// <summary>
/// AgentIsland 插件的设置类
/// </summary>
public class AgentIslandSettings : ObservableObject
{
    private int _port = 5943;
    private bool _isEnabled = true;

    /// <summary>
    /// MCP 服务器监听端口
    /// </summary>
    [JsonPropertyName("port")]
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    /// <summary>
    /// 是否启用 MCP 服务器
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }
}
