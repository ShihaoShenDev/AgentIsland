using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
    private McpTransportMode _transportMode = McpTransportMode.StreamableHttp;
    private bool _isAcpEnabled = true;
    private bool _isAgentAutomationEnabled = true;
    private bool _autoStartAgentsWithClassIsland;
    private bool _showAutomationNotifications = true;
    private ObservableCollection<AiTextEntry> _aiTextEntries = [];
    private ObservableCollection<AcpAgentProfile> _acpAgents = [];
    private bool _isTelemetryEnabled = true;
    private bool _hasAgreedToPrivacyPolicy;
    private string _customSentryDsn = "";

    public AgentIslandSettings()
    {
        HookAgentCollection(_acpAgents);
        HookAiTextCollection(_aiTextEntries);
    }

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

    /// <summary>
    /// MCP 服务器传输模式
    /// </summary>
    [JsonPropertyName("transportMode")]
    public McpTransportMode TransportMode
    {
        get => _transportMode;
        set => SetProperty(ref _transportMode, value);
    }

    /// <summary>
    /// 是否启用 ACP 面板能力
    /// </summary>
    [JsonPropertyName("isAcpEnabled")]
    public bool IsAcpEnabled
    {
        get => _isAcpEnabled;
        set => SetProperty(ref _isAcpEnabled, value);
    }

    /// <summary>
    /// 是否启用基于 Agent 的自动化
    /// </summary>
    [JsonPropertyName("isAgentAutomationEnabled")]
    public bool IsAgentAutomationEnabled
    {
        get => _isAgentAutomationEnabled;
        set => SetProperty(ref _isAgentAutomationEnabled, value);
    }

    /// <summary>
    /// 是否在 ClassIsland 启动时自动启动 Agent
    /// </summary>
    [JsonPropertyName("autoStartAgentsWithClassIsland")]
    public bool AutoStartAgentsWithClassIsland
    {
        get => _autoStartAgentsWithClassIsland;
        set => SetProperty(ref _autoStartAgentsWithClassIsland, value);
    }

    /// <summary>
    /// 是否显示自动化提示
    /// </summary>
    [JsonPropertyName("showAutomationNotifications")]
    public bool ShowAutomationNotifications
    {
        get => _showAutomationNotifications;
        set => SetProperty(ref _showAutomationNotifications, value);
    }

    /// <summary>
    /// AI 文字条目列表
    /// </summary>
    [JsonPropertyName("aiTextEntries")]
    public ObservableCollection<AiTextEntry> AiTextEntries
    {
        get => _aiTextEntries;
        set
        {
            if (_aiTextEntries == value)
            {
                return;
            }

            UnhookAiTextCollection(_aiTextEntries);
            SetProperty(ref _aiTextEntries, value);
            HookAiTextCollection(_aiTextEntries);
        }
    }

    /// <summary>
    /// ACP Agent 列表
    /// </summary>
    [JsonPropertyName("acpAgents")]
    public ObservableCollection<AcpAgentProfile> AcpAgents
    {
        get => _acpAgents;
        set
        {
            if (_acpAgents == value)
            {
                return;
            }

            UnhookAgentCollection(_acpAgents);
            SetProperty(ref _acpAgents, value);
            HookAgentCollection(_acpAgents);
            RaiseAcpDerivedPropertiesChanged();
        }
    }

    /// <summary>
    /// 是否启用遥测数据收集
    /// </summary>
    [JsonPropertyName("isTelemetryEnabled")]
    public bool IsTelemetryEnabled
    {
        get => _isTelemetryEnabled;
        set => SetProperty(ref _isTelemetryEnabled, value);
    }

    /// <summary>
    /// 是否已同意隐私政策和跨境数据传输协议
    /// </summary>
    [JsonPropertyName("hasAgreedToPrivacyPolicy")]
    public bool HasAgreedToPrivacyPolicy
    {
        get => _hasAgreedToPrivacyPolicy;
        set => SetProperty(ref _hasAgreedToPrivacyPolicy, value);
    }

    /// <summary>
    /// 用户自定义 Sentry DSN（留空使用默认 DSN）
    /// </summary>
    [JsonPropertyName("customSentryDsn")]
    public string CustomSentryDsn
    {
        get => _customSentryDsn;
        set => SetProperty(ref _customSentryDsn, value);
    }

    /// <summary>
    /// 遥测是否处于活动状态（已启用且已同意隐私政策，或使用了自定义 DSN）
    /// </summary>
    public bool IsTelemetryActive =>
        IsTelemetryEnabled &&
        (HasAgreedToPrivacyPolicy || !string.IsNullOrWhiteSpace(CustomSentryDsn));

    /// <summary>
    /// 遥测开关是否可用（同意协议或使用自定义 DSN 时才可开启）
    /// </summary>
    public bool CanToggleTelemetry =>
        HasAgreedToPrivacyPolicy || !string.IsNullOrWhiteSpace(CustomSentryDsn);

    /// <summary>
    /// 是否正在使用自定义 DSN（此时跳过隐私协议同意检查）
    /// </summary>
    public bool IsUsingCustomDsn => !string.IsNullOrWhiteSpace(CustomSentryDsn);

    /// <summary>
    /// 实际使用的 Sentry DSN（自定义优先，否则用默认）
    /// </summary>
    public string EffectiveSentryDsn =>
        string.IsNullOrWhiteSpace(CustomSentryDsn)
            ? Services.SentryTelemetryService.DefaultDsn
            : CustomSentryDsn;

    /// <summary>
    /// MCP 服务器连接地址（只读）
    /// </summary>
    public string ConnectionAddress
    {
        get
        {
            var endPoint = TransportMode == McpTransportMode.Sse ? "sse" : "mcp";
            return $"http://localhost:{Port}/{endPoint}";
        }
    }

    /// <summary>
    /// ACP Agent 总数
    /// </summary>
    public int TotalAgentCount => AcpAgents.Count;

    /// <summary>
    /// 已启用的 ACP Agent 数量
    /// </summary>
    public int EnabledAgentCount => AcpAgents.Count(x => x.IsEnabled);

    /// <summary>
    /// 是否已配置 ACP Agent
    /// </summary>
    public bool HasAcpAgents => AcpAgents.Count > 0;

    /// <summary>
    /// ACP Agent 摘要文本
    /// </summary>
    public string AcpAgentSummary => $"已配置 {TotalAgentCount} 个 Agent，启用 {EnabledAgentCount} 个。";

    /// <summary>
    /// ACP Agent 空状态文本
    /// </summary>
    public string AcpAgentEmptyStateText => HasAcpAgents
        ? string.Empty
        : "尚未添加 ACP Agent。可先创建占位项，后续再补真实连接信息。";

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Port) or nameof(TransportMode))
        {
            OnPropertyChanged(nameof(ConnectionAddress));
        }

        if (e.PropertyName is nameof(IsTelemetryEnabled)
            or nameof(HasAgreedToPrivacyPolicy)
            or nameof(CustomSentryDsn))
        {
            OnPropertyChanged(nameof(IsTelemetryActive));
        }

        if (e.PropertyName is nameof(HasAgreedToPrivacyPolicy)
            or nameof(CustomSentryDsn))
        {
            OnPropertyChanged(nameof(CanToggleTelemetry));

            // 同意协议或使用自定义 DSN 后自动开启遥测
            if (CanToggleTelemetry && !IsTelemetryEnabled)
            {
                IsTelemetryEnabled = true;
            }
        }

        if (e.PropertyName is nameof(CustomSentryDsn))
        {
            OnPropertyChanged(nameof(EffectiveSentryDsn));
            OnPropertyChanged(nameof(IsUsingCustomDsn));
        }
    }

    private void HookAgentCollection(ObservableCollection<AcpAgentProfile>? agents)
    {
        if (agents is null)
        {
            return;
        }

        agents.CollectionChanged += OnAcpAgentsCollectionChanged;
        foreach (var agent in agents)
        {
            agent.PropertyChanged += OnAcpAgentPropertyChanged;
        }
    }

    private void UnhookAgentCollection(ObservableCollection<AcpAgentProfile>? agents)
    {
        if (agents is null)
        {
            return;
        }

        agents.CollectionChanged -= OnAcpAgentsCollectionChanged;
        foreach (var agent in agents)
        {
            agent.PropertyChanged -= OnAcpAgentPropertyChanged;
        }
    }

    private void OnAcpAgentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (AcpAgentProfile agent in e.OldItems)
            {
                agent.PropertyChanged -= OnAcpAgentPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (AcpAgentProfile agent in e.NewItems)
            {
                agent.PropertyChanged += OnAcpAgentPropertyChanged;
            }
        }

        OnPropertyChanged(nameof(AcpAgents));
        RaiseAcpDerivedPropertiesChanged();
    }

    private void OnAcpAgentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AcpAgents));
        RaiseAcpDerivedPropertiesChanged();
    }

    private void RaiseAcpDerivedPropertiesChanged()
    {
        OnPropertyChanged(nameof(TotalAgentCount));
        OnPropertyChanged(nameof(EnabledAgentCount));
        OnPropertyChanged(nameof(HasAcpAgents));
        OnPropertyChanged(nameof(AcpAgentSummary));
        OnPropertyChanged(nameof(AcpAgentEmptyStateText));
    }

    private void HookAiTextCollection(ObservableCollection<AiTextEntry>? entries)
    {
        if (entries is null)
        {
            return;
        }

        entries.CollectionChanged += OnAiTextCollectionChanged;
        foreach (var entry in entries)
        {
            entry.PropertyChanged += OnAiTextEntryPropertyChanged;
        }
    }

    private void UnhookAiTextCollection(ObservableCollection<AiTextEntry>? entries)
    {
        if (entries is null)
        {
            return;
        }

        entries.CollectionChanged -= OnAiTextCollectionChanged;
        foreach (var entry in entries)
        {
            entry.PropertyChanged -= OnAiTextEntryPropertyChanged;
        }
    }

    private void OnAiTextCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (AiTextEntry entry in e.OldItems)
            {
                entry.PropertyChanged -= OnAiTextEntryPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (AiTextEntry entry in e.NewItems)
            {
                entry.PropertyChanged += OnAiTextEntryPropertyChanged;
            }
        }

        OnPropertyChanged(nameof(AiTextEntries));
    }

    private void OnAiTextEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AiTextEntries));
    }
}
