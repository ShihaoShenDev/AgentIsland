---
kind: logging_system
name: 基于 Microsoft.Extensions.Logging 的结构化日志与 Sentry 遥测体系
category: logging_system
scope:
    - '**'
source_files:
    - AgentIsland/Plugin.cs
    - AgentIsland/Services/SentryTelemetryService.cs
    - AgentIsland/Mcp/McpServerManager.cs
    - AgentIsland/Automation/RunAcpAction.cs
    - AgentIsland/Mcp/Tools/AddComponentTextTool.cs
    - AgentIsland/AgentIsland.csproj
---

## 1. 使用的系统与框架
- **结构化日志**：使用 `Microsoft.Extensions.Logging`（通过 ClassIsland 宿主注入），以泛型 `ILogger<T>` 形式在业务类中消费，采用 `{Name}` 占位符进行结构化字段输出。
- **异常与性能遥测**：使用 `Sentry` SDK（版本 5.14.1）作为错误上报、事务/跨度追踪和面包屑记录的后端。
- **无第三方日志库**：项目中未引入 Serilog、NLog 等独立日志框架，所有应用层日志均走 DI 提供的 `ILogger<T>`。

## 2. 关键文件与位置
- `AgentIsland/Plugin.cs`：插件入口，负责生命周期钩子中的日志调用（启动/停止 MCP 服务器）、从 IAppHost 解析 `ILogger<T>` 实例。
- `AgentIsland/Services/SentryTelemetryService.cs`：封装 Sentry SDK 的初始化、动态启停、异常捕获、面包屑与 Transaction 包装 API。
- `AgentIsland/Mcp/McpServerManager.cs`：MCP 服务器生命周期日志（启动/停止、传输模式、端口）。
- `AgentIsland/Automation/RunAcpAction.cs`：自动化动作执行过程中的信息/警告日志。
- `AgentIsland/Mcp/Tools/*.cs`：各工具方法内部通过 `IAppHost.GetService<ILogger<T>>()` 获取 logger，记录调试/信息级结构化日志。
- `AgentIsland/AgentIsland.csproj`：声明对 `ClassIsland.PluginSdk`、`DotNetCampus.ModelContextProtocol`、`AgentClientProtocol`、`Sentry` 的依赖。

## 3. 架构与约定
- **日志来源分层**
  - 应用层：通过 DI 注入的 `ILogger<T>` 输出结构化日志，统一由宿主（ClassIsland）配置 sink。
  - 遥测层：通过 `SentryTelemetryService` 主动添加 Breadcrumb、CaptureException、StartTransaction/Finish，形成可关联的错误上下文。
- **结构化字段约定**
  - 使用 `{Id}`、`{TransportMode}`、`{Port}`、`{agent.Name}` 等命名占位符，便于下游检索与聚合。
  - 关键操作（如 MCP 启动、工具调用）同时写入日志与 Sentry Breadcrumb，保证本地诊断与云端遥测一致。
- **Sentry 遥测策略**
  - 默认 DSN 硬编码于代码中，可通过设置项 `CustomSentryDsn` 覆盖；仅在用户同意隐私政策且开启遥测时初始化。
  - 通过 `EvaluateAndApply()` 监听设置变更，支持运行时动态启用/关闭 Sentry。
  - 为所有事件打上 `plugin=AgentIsland`、`classisland.plugin=AgentIsland` 标签，便于多插件区分。
  - 提供 `WithInstrumentation` / `WithInstrumentationAsync` 包装器，自动为同步/异步操作创建 Transaction、记录 Breadcrumb 并在异常时上报。
- **日志级别使用规范**
  - `LogInformation`：正常业务流程（MCP 启动/停止、新增条目、触发动作）。
  - `LogWarning`：条件不满足时的拒绝路径（功能未启用、Agent 停用、找不到 Agent）。
  - `LogError(ex, ...)`：异常场景，附带 Exception 对象以便堆栈采集。
  - `LogDebug`：仅用于开发期调试（如工具参数打印）。

## 4. 开发者应遵循的规则
- **不要直接使用 Console/Trace**：所有应用日志必须通过 DI 注入的 `ILogger<T>` 或 `IAppHost.GetService<ILogger<T>>()` 获取。
- **使用结构化占位符**：避免字符串拼接，统一用 `{Name}` 占位符输出字段，便于查询与过滤。
- **合理选择日志级别**：流程性信息用 Information，可恢复问题用 Warning，不可恢复错误用 Error 并附带异常对象。
- **遥测与日志联动**：关键路径（尤其是 MCP 工具调用）应同时记录日志与 Breadcrumb/Transaction，优先使用 `SentryTelemetryService.WithInstrumentation*` 包装。
- **尊重隐私开关**：不要绕过 `SentryTelemetryService` 直接调用 `SentrySdk`；遥测行为应由设置项控制，保持可动态启停。
- **敏感数据脱敏**：Sentry 已禁用 PII 发送（`SendDefaultPII = false`），自定义 Extra/Tag 时需自行确保不包含个人信息。