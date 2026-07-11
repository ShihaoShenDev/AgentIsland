---
kind: logging_system
name: 基于 Microsoft.Extensions.Logging 的结构化日志与 Sentry 遥测体系
category: logging_system
scope:
    - '**'
source_files:
    - Plugin.cs
    - Services/SentryTelemetryService.cs
    - Automation/RunAcpAction.cs
    - Mcp/McpServerManager.cs
    - Mcp/Tools/LessonTools.cs
    - Mcp/Tools/ScheduleTools.cs
    - Mcp/Tools/SwapClassesTool.cs
    - Mcp/Tools/GetScheduleByDateTool.cs
    - Mcp/Tools/SendNotificationTool.cs
    - Mcp/Tools/SetComponentTextTool.cs
    - Mcp/Tools/AgentIslandNotificationProvider.cs
    - docs/compose/plans/2026-06-27-add-logging-to-core-modules.md
---

## 系统概述

AgentIsland 采用 **Microsoft.Extensions.Logging**（通过 ClassIsland.PluginSdk 传递依赖）作为统一日志框架，结合 **Sentry** 实现异常上报与分布式追踪。日志输出遵循结构化字段 + 中文消息的约定，所有 ILogger<T> 实例均通过构造函数注入且允许为 null，调用处使用 `?.` 空条件操作符保证健壮性。

## 核心架构

### 1. 日志框架层
- **框架**: `Microsoft.Extensions.Logging.ILogger<T>`
- **来源**: 由宿主 ClassIsland 通过 DI 容器提供，项目本身不直接引用该包（仅通过 ClassIsland.PluginSdk 传递依赖）
- **注入模式**: 构造函数参数 `ILogger<T>? logger = null`，字段声明为可空引用类型
- **调用方式**: `_logger?.LogInformation(...)` / `_logger?.LogDebug(...)` / `_logger?.LogWarning(...)` / `_logger?.LogError(ex, ...)`

### 2. 遥测集成层
`Services/SentryTelemetryService.cs` 封装 Sentry SDK 生命周期：
- 根据用户隐私同意状态和设置动态初始化/关闭 SDK
- 提供 `WithInstrumentation` / `WithInstrumentationAsync` 包装器自动添加 Transaction、Breadcrumb 和异常捕获
- 全局 Scope 设置 tag `classisland.plugin=AgentIsland`
- BeforeSend 钩子统一标记 `plugin=AgentIsland` 标签

### 3. 插件入口初始化
`Plugin.cs` 中完成关键初始化：
- 加载 Settings.json 并注册 PropertyChanged 事件实现配置热更新
- 创建 SentryTelemetryService 并调用 EvaluateAndApply()
- 通过 IAppHost.GetService<ILogger<T>>() 获取各组件的 ILogger 实例
- 在 AppStarted/AppStopping 生命周期中记录 Breadcrumb 和错误日志

## 日志级别约定

| 级别 | 用途 | 示例场景 |
|------|------|----------|
| Debug | 详细执行流程、参数、状态转换 | 工具方法调用入口、内部状态检查 |
| Information | 重要业务操作（启动/停止/连接/完成） | MCP 服务器启停、ACP Agent 运行、通知发送 |
| Warning | 可恢复的异常情况 | 功能未启用、配置缺失、找不到资源 |
| Error | 不可恢复的错误，附带 Exception | 网络请求失败、进程启动异常、MCP 服务器启动失败 |

## 结构化字段规范

所有日志消息使用占位符 `{Name}` 而非字符串拼接，确保结构化输出：
```csharp
_logger?.LogInformation("MCP 服务器启动中，传输模式: {TransportMode}，端口: {Port}", transportMode, port);
_logger?.LogInformation("交换课程: {Index1} <-> {Index2}, 日期: {Date}", classIndex1, classIndex2, date);
```

## 覆盖范围

已实现日志的核心模块：
- **自动化动作**: `Automation/RunAcpAction.cs` — ACP 运行触发、参数校验、Agent 启动
- **MCP 服务管理**: `Mcp/McpServerManager.cs` — 服务器生命周期、传输模式切换
- **MCP 工具集**: `Mcp/Tools/*` — LessonTools、ScheduleTools、SwapClassesTool、GetScheduleByDateTool、SendNotificationTool、SetComponentTextTool、AgentIslandNotificationProvider
- **插件入口**: `Plugin.cs` — 插件生命周期、MCP 服务器启动/停止

尚未完全覆盖（参考计划文档）：
- `Services/AcpRunnerService.cs` — 计划中添加 Agent 生命周期日志并修复被吞掉的异常

## 开发者规则

1. **必须使用构造函数注入** `ILogger<T>? logger = null`，禁止静态 Logger 或全局单例
2. **始终使用 `?.` 调用**，因为 DI 可能未提供 ILogger 实例
3. **使用占位符格式化**，禁止字符串插值 `$"...{var}"` 用于日志消息模板
4. **中文日志消息**，保持与现有代码风格一致
5. **避免在高频路径**（如循环内）调用日志
6. **异常日志必须包含 Exception 对象**：`_logger?.LogError(ex, "错误信息")`
7. **敏感信息脱敏**，不在日志中输出密码、令牌等敏感数据