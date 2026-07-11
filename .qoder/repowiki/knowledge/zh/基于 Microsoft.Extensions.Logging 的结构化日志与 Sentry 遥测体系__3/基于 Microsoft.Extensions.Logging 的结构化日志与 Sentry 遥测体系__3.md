---
kind: logging_system
name: 基于 Microsoft.Extensions.Logging 的结构化日志与 Sentry 遥测体系
category: logging_system
scope:
    - '**'
source_files:
    - Plugin.cs
    - Services/SentryTelemetryService.cs
    - Mcp/McpServerManager.cs
    - Automation/RunAcpAction.cs
    - Mcp/Tools/AgentIslandNotificationProvider.cs
    - Mcp/Tools/LessonTools.cs
    - Mcp/Tools/ScheduleTools.cs
    - Mcp/Tools/SendNotificationTool.cs
    - Mcp/Tools/SetComponentTextTool.cs
    - AgentIsland.csproj
---

## 系统概述
AgentIsland 插件采用 **Microsoft.Extensions.Logging**（ILogger）作为统一日志抽象，配合 **Sentry SDK** 实现结构化异常上报与性能追踪。日志由宿主 ClassIsland 注入 ILogger 实例，本插件不自行配置日志输出目标（sink），仅消费 DI 提供的 logger。

## 关键文件与包
- `Plugin.cs`：插件入口，通过 `IAppHost.GetService<ILogger<T>>()` 获取 logger，并在生命周期事件中添加 Sentry breadcrumb。
- `Services/SentryTelemetryService.cs`：封装 Sentry SDK 初始化/关闭、Breadcrumb、Transaction/Span、异常捕获；根据设置动态启停。
- `Mcp/McpServerManager.cs`：MCP 服务器启动/停止流程中记录信息日志并包裹 Transaction。
- `Automation/RunAcpAction.cs`：自动化动作触发时记录信息/警告日志。
- `Mcp/Tools/*`：各 MCP 工具方法内部使用 `LogDebug`/`LogInformation` 记录调用参数与结果。
- `AgentIsland.csproj`：引入 `Sentry` 5.14.1 包，未引入 Serilog/NLog 等第三方日志框架。

## 架构与约定
- **日志级别使用规范**
  - `LogInformation`：记录业务状态变更（如“MCP 服务器已启动”“ACP Agent 已启动”“发送通知”）。
  - `LogWarning`：记录可恢复的异常条件（如功能未启用、找不到 Agent、Agent 被停用）。
  - `LogError(ex, ...)`：仅在捕获到异常时记录，同时附带异常对象。
  - `LogDebug(...)`：在 MCP 工具层记录入参、调试路径等高频低价值信息。
- **结构化字段**：全部使用模板占位符 `{Name}` 而非字符串拼接，便于下游结构化解析。
- **Sentry 集成**：通过 `SentryTelemetryService` 提供 `AddBreadcrumb`、`CaptureException`、`WithInstrumentation`/`WithInstrumentationAsync` 包装器，将关键操作自动纳入 Transaction 并附加 context/tag。
- **DI 注入模式**：所有需要日志的类以可选 `ILogger<T>` 构造器参数接收，避免强依赖；当宿主未提供 logger 时调用点使用 `?.` 安全调用。
- **无自定义 sink**：插件不注册任何 Console/File/DiagnosticSource 等输出目标，日志路由完全交由宿主 ClassIsland 决定。

## 开发者应遵循的规则
1. 通过构造函数注入 `ILogger<T>`，不要直接 new 或静态访问。
2. 优先使用 `LogInformation` 描述业务里程碑，`LogWarning` 描述可恢复问题，`LogError(ex, msg)` 仅用于异常路径。
3. 消息一律使用模板占位符 `{Field}`，禁止 `$"...{x}..."` 拼接。
4. 对耗时/外部调用使用 `SentryTelemetryService.WithInstrumentation*` 包裹，以便自动产出 Transaction 和错误上报。
5. 不在 UI 线程外直接写控制台或文件；如需持久化日志，应通过宿主扩展点或 Sentry 上报。