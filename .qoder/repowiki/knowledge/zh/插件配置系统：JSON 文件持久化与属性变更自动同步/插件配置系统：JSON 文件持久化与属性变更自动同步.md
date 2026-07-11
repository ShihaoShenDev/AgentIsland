---
kind: configuration_system
name: 插件配置系统：JSON 文件持久化与属性变更自动同步
category: configuration_system
scope:
    - '**'
source_files:
    - Plugin.cs
    - Models/AgentIslandSettings.cs
    - Models/AcpAgentProfile.cs
    - Models/McpTransportMode.cs
    - Models/AiTextEntry.cs
    - Models/RunAcpActionSettings.cs
    - Services/AcpRunnerService.cs
---

## 配置系统与加载机制

AgentIsland 作为 ClassIsland 桌面插件，采用**单一 JSON 配置文件 + 属性变更自动持久化**的配置方案。所有运行时设置集中在 `Settings.json` 文件中，位于宿主提供的 `PluginConfigFolder` 目录下。

### 核心流程

1. **启动时加载**：`Plugin.Initialize()` 中通过 `ConfigureFileHelper.LoadConfig<AgentIslandSettings>(Path.Combine(PluginConfigFolder, "Settings.json"))` 读取配置
2. **变更监听**：订阅 `Settings.PropertyChanged` 事件，每次属性修改后调用 `ConfigureFileHelper.SaveConfig(path, Settings)` 立即写回磁盘
3. **依赖注入**：将 `AgentIslandSettings` 单例注册到 DI 容器，供各服务直接消费
4. **运行时生效**：遥测、MCP 服务器等组件在应用启动阶段从 Settings 读取并应用配置

### 配置模型设计

- 使用 `CommunityToolkit.Mvvm.ObservableObject` 实现 INotifyPropertyChanged，确保 UI 双向绑定与自动保存联动
- 通过 `[JsonPropertyName]` 显式声明 JSON 字段名（如 `isEnabled`、`transportMode`），保持序列化契约稳定
- 集合类型使用 `ObservableCollection<T>`，并在 setter 中钩子新旧集合的 `CollectionChanged` 与项级 `PropertyChanged` 事件，保证嵌套对象变更也能触发保存
- 派生属性（如 `ConnectionAddress`、`IsTelemetryActive`、`EffectiveSentryDsn`）在 `OnPropertyChanged` 中集中计算并通知，避免冗余逻辑

### 配置项分类

| 类别 | 关键属性 | 说明 |
|------|----------|------|
| MCP 服务器 | `Port`、`TransportMode`、`IsEnabled` | 监听端口、StreamableHttp/SSE 传输模式、开关 |
| ACP Agent | `AcpAgents`（`AcpAgentProfile[]`）、`AutoStartAgentsWithClassIsland` | Agent 命令、启用状态、是否随宿主启动 |
| AI 文字 | `AiTextEntries`（`AiTextEntry[]`） | 动态文本条目列表 |
| 遥测 | `IsTelemetryEnabled`、`HasAgreedToPrivacyPolicy`、`CustomSentryDsn` | Sentry DSN 与隐私协议同意标志 |
| 自动化 | `IsAgentAutomationEnabled`、`ShowAutomationNotifications` | 动作执行与提示控制 |

### 约定与约束

- 新增配置属性必须标注 `[JsonPropertyName]`，默认值在字段初始化处给出
- 对 `ObservableCollection` 属性的替换需遵循 Hook/Unhook 模式，防止内存泄漏
- 派生属性变更需在 `OnPropertyChanged` 中显式 `RaisePropertyChanged`，UI 才能响应
- 敏感信息（如 DSN）通过配置项暴露，不硬编码；遥测启用受隐私协议与自定义 DSN 双重条件控制
- 配置结构由 `AgentIslandJsonContext`（System.Text.Json Source Generator）参与编译期优化，新增类型需同步更新上下文