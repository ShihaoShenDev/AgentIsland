# MCP 服务器管理

<cite>
**本文引用的文件**
- [McpServerManager.cs](file://Mcp/McpServerManager.cs)
- [McpTransportMode.cs](file://Models/McpTransportMode.cs)
- [SentryTelemetryService.cs](file://Services/SentryTelemetryService.cs)
- [Plugin.cs](file://Plugin.cs)
- [AgentIslandSettings.cs](file://Models/AgentIslandSettings.cs)
- [McpSettingsPage.axaml](file://Views/SettingsPages/McpSettingsPage.axaml)
- [EchoCaveTool.cs](file://Mcp/Tools/EchoCaveTool.cs)
- [AddComponentTextTool.cs](file://Mcp/Tools/AddComponentTextTool.cs)
- [SetComponentTextTool.cs](file://Mcp/Tools/SetComponentTextTool.cs)
- [ListComponentTextTool.cs](file://Mcp/Tools/ListComponentTextTool.cs)
- [DeleteComponentTextTool.cs](file://Mcp/Tools/DeleteComponentTextTool.cs)
- [ClearComponentTextTool.cs](file://Mcp/Tools/ClearComponentTextTool.cs)
- [ToolResults.cs](file://Models/ToolResults.cs)
- [AgentIslandJsonContext.cs](file://Models/AgentIslandJsonContext.cs)
</cite>

## 更新摘要
**所做更改**
- 更新了工具注册机制部分，新增 AI 文字管理工具和回声洞穴功能的详细说明
- 扩展了工具分类和描述，涵盖完整的工具集
- 新增了 AI 文字管理工具的详细分析章节
- 更新了工具架构图表以反映新增的工具类型
- 增强了工具注册示例代码

## 目录
1. [简介](#简介)
2. [项目结构](#项目结构)
3. [核心组件](#核心组件)
4. [架构总览](#架构总览)
5. [详细组件分析](#详细组件分析)
6. [依赖关系分析](#依赖关系分析)
7. [性能与并发考虑](#性能与并发考虑)
8. [故障排查指南](#故障排查指南)
9. [结论](#结论)
10. [附录：配置与使用示例](#附录配置与使用示例)

## 简介
本文件围绕 McpServerManager 类，系统化阐述 MCP 服务器的生命周期管理（启动、停止、资源清理）、传输模式配置（SSE/HTTP）以及工具注册机制。文档同时覆盖 StartAsync 和 StopAsync 的参数、异常处理与性能考量，并说明与 SentryTelemetryService 的集成用于监控与错误追踪。文末提供不同传输模式的配置与使用示例，以及常见问题（端口冲突、状态管理、资源泄漏）的处理建议，兼顾初学者友好与资深开发者的技术深度。

**更新** 新增了 AI 文字管理工具和回声洞穴功能的详细文档，包括完整的工作流程和使用示例。

## 项目结构
与 MCP 服务器管理直接相关的代码分布在以下位置：
- 服务器管理器：Mcp/McpServerManager.cs
- 工具实现：Mcp/Tools/ 目录下包含所有工具类
- 传输模式枚举：Models/McpTransportMode.cs
- 遥测服务：Services/SentryTelemetryService.cs
- 插件入口与生命周期绑定：Plugin.cs
- 设置模型与连接地址计算：Models/AgentIslandSettings.cs
- 设置界面（端口、传输模式等）：Views/SettingsPages/McpSettingsPage.axaml
- 工具结果模型：Models/ToolResults.cs

```mermaid
graph TB
subgraph "插件层"
Plugin["Plugin.cs"]
end
subgraph "MCP 服务器"
Manager["McpServerManager.cs"]
Transport["McpTransportMode.cs"]
Tools["工具集合"]
end
subgraph "AI 文字管理工具"
AddTool["AddComponentTextTool.cs"]
SetTextTool["SetComponentTextTool.cs"]
ListTool["ListComponentTextTool.cs"]
DeleteTool["DeleteComponentTextTool.cs"]
ClearTool["ClearComponentTextTool.cs"]
end
subgraph "其他工具"
EchoTool["EchoCaveTool.cs"]
LessonTool["LessonTools.cs"]
ScheduleTool["ScheduleTools.cs"]
NotificationTool["SendNotificationTool.cs"]
SwapTool["SwapClassesTool.cs"]
GetScheduleTool["GetScheduleByDateTool.cs"]
end
subgraph "遥测"
Telemetry["SentryTelemetryService.cs"]
end
subgraph "设置"
Settings["AgentIslandSettings.cs"]
UI["McpSettingsPage.axaml"]
end
subgraph "结果模型"
ToolResults["ToolResults.cs"]
JsonContext["AgentIslandJsonContext.cs"]
end
Plugin --> Manager
Plugin --> Telemetry
Manager --> Transport
Manager --> Telemetry
Manager --> Tools
Tools --> AddTool
Tools --> SetTextTool
Tools --> ListTool
Tools --> DeleteTool
Tools --> ClearTool
Tools --> EchoTool
Tools --> LessonTool
Tools --> ScheduleTool
Tools --> NotificationTool
Tools --> SwapTool
Tools --> GetScheduleTool
UI --> Settings
Plugin --> Settings
Tools --> ToolResults
Tools --> JsonContext
```

**图表来源**
- [McpServerManager.cs:41-55](file://Mcp/McpServerManager.cs#L41-L55)
- [EchoCaveTool.cs:17-46](file://Mcp/Tools/EchoCaveTool.cs#L17-L46)
- [AddComponentTextTool.cs:18-41](file://Mcp/Tools/AddComponentTextTool.cs#L18-L41)
- [SetComponentTextTool.cs:17-39](file://Mcp/Tools/SetComponentTextTool.cs#L17-L39)
- [ListComponentTextTool.cs:18-35](file://Mcp/Tools/ListComponentTextTool.cs#L18-L35)
- [DeleteComponentTextTool.cs:18-39](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L39)
- [ClearComponentTextTool.cs:18-41](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L41)
- [ToolResults.cs:55-69](file://Models/ToolResults.cs#L55-L69)
- [AgentIslandJsonContext.cs:14-21](file://Models/AgentIslandJsonContext.cs#L14-L21)

章节来源
- [McpServerManager.cs:41-55](file://Mcp/McpServerManager.cs#L41-L55)
- [EchoCaveTool.cs:17-46](file://Mcp/Tools/EchoCaveTool.cs#L17-L46)
- [AddComponentTextTool.cs:18-41](file://Mcp/Tools/AddComponentTextTool.cs#L18-L41)
- [SetComponentTextTool.cs:17-39](file://Mcp/Tools/SetComponentTextTool.cs#L17-L39)
- [ListComponentTextTool.cs:18-35](file://Mcp/Tools/ListComponentTextTool.cs#L18-L35)
- [DeleteComponentTextTool.cs:18-39](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L39)
- [ClearComponentTextTool.cs:18-41](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L41)
- [ToolResults.cs:55-69](file://Models/ToolResults.cs#L55-L69)
- [AgentIslandJsonContext.cs:14-21](file://Models/AgentIslandJsonContext.cs#L14-L21)

## 核心组件
- McpServerManager：封装 MCP 服务器的构建、启动、停止与资源释放；负责按传输模式选择端点；在启动/停止路径中记录日志并通过 Sentry 上报遥测。
- SentryTelemetryService：根据用户隐私同意与开关动态初始化/关闭 Sentry SDK，并提供异常捕获、面包屑、事务包装等能力。
- McpTransportMode：定义 StreamableHttp 与 SSE 两种传输模式。
- Plugin：应用生命周期事件驱动，创建并管理 McpServerManager 实例，并在应用启动/停止时调用其 StartAsync/StopAsync。
- AgentIslandSettings：提供端口、传输模式、连接地址等信息，UI 通过数据绑定修改这些值。

**更新** 新增了完整的工具生态系统，包括 AI 文字管理工具和回声洞穴功能，每个工具都实现了 IMcpServerTool 接口并提供完整的输入输出模式定义。

章节来源
- [McpServerManager.cs:11-23](file://Mcp/McpServerManager.cs#L11-L23)
- [SentryTelemetryService.cs:11-25](file://Services/SentryTelemetryService.cs#L11-L25)
- [McpTransportMode.cs:6-17](file://Models/McpTransportMode.cs#L6-L17)
- [Plugin.cs:55-97](file://Plugin.cs#L55-L97)
- [AgentIslandSettings.cs:204-211](file://Models/AgentIslandSettings.cs#L204-L211)

## 架构总览
MCP 服务器由 McpServerManager 统一管理，基于 McpServerBuilder 构建并注入工具集；传输层通过 LocalHostHttpServerTransportOptions 配置端口与端点；SentryTelemetryService 贯穿启动/停止流程进行遥测采集。

```mermaid
sequenceDiagram
participant App as "应用(Plugin)"
participant Mgr as "McpServerManager"
participant Builder as "McpServerBuilder"
participant Server as "McpServer"
participant Telemetry as "SentryTelemetryService"
App->>Mgr : "StartAsync(port, transportMode)"
Mgr->>Telemetry : "StartTransaction(\"mcp.server.start\")"
Mgr->>Builder : "WithTools(...).WithJsonSerializer(...)"
Note over Builder : 注册所有工具<br/>- AI 文字管理工具<br/>- 回声洞穴工具<br/>- 课程相关工具<br/>- 通知工具
alt "SSE 模式"
Mgr->>Builder : "WithLocalHostHttp({Port, EndPoint=\"sse\", IsCompatibleWithSse=true})"
else "Streamable HTTP 模式"
Mgr->>Builder : "WithLocalHostHttp({Port, EndPoint=\"mcp\", IsCompatibleWithSse=false})"
end
Mgr->>Server : "Build()"
Mgr->>Server : "StartAsync(token)"
Server-->>Mgr : "已监听"
Mgr->>Telemetry : "Finish(SpanStatus.Ok)"
Mgr-->>App : "完成"
App->>Mgr : "StopAsync()"
Mgr->>Telemetry : "StartTransaction(\"mcp.server.stop\")"
Mgr->>Mgr : "Cancel token"
Mgr->>Server : "StopAsync(CancellationToken.None)"
Mgr->>Mgr : "Dispose CTS"
Mgr->>Telemetry : "Finish(SpanStatus.Ok)"
Mgr-->>App : "完成"
```

**图表来源**
- [McpServerManager.cs:25-112](file://Mcp/McpServerManager.cs#L25-L112)
- [Plugin.cs:65-97](file://Plugin.cs#L65-L97)
- [SentryTelemetryService.cs:30-75](file://Services/SentryTelemetryService.cs#L30-L75)

## 详细组件分析

### McpServerManager 类分析
职责与关键点：
- 生命周期管理
  - StartAsync：幂等保护（若已有运行中的服务器则直接返回），创建取消令牌源，构建服务器并启动，记录日志，上报成功事务。
  - StopAsync：先取消令牌，再调用底层停止，置空引用并释放取消令牌源，记录日志，上报成功事务。
  - Dispose：同步阻塞调用 StopAsync 确保资源释放。
- 传输模式配置
  - 依据 McpTransportMode 选择端点：SSE 使用 “sse”，默认使用 “mcp”；是否兼容 SSE 由 IsCompatibleWithSse 控制。
- 工具注册机制
  - 通过 WithTools 委托批量注册内置工具（课程、课表、通知、组件文本、回声洞穴等）。
- 异常处理与遥测
  - 启动/停止失败时捕获异常，上报到 Sentry，并以 InternalError 结束事务。
- 性能与并发
  - 幂等检查避免重复启动；取消令牌用于优雅退出；StopAsync 顺序执行，避免竞态。

```mermaid
classDiagram
class McpServerManager {
- _server : McpServer?
- _cts : CancellationTokenSource?
- _disposed : bool
- _logger : ILogger<McpServerManager>?
- _telemetry : SentryTelemetryService?
+ StartAsync(port : int, transportMode : McpTransportMode) Task
+ StopAsync() Task
+ Dispose() void
}
class EchoCaveTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class AddComponentTextTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class SetComponentTextTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class ListComponentTextTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class DeleteComponentTextTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class ClearComponentTextTool {
+ ToolName : string
+ GetToolDefinition(jsonContext) Tool
+ CallTool(context) ValueTask~CallToolResult~
}
class SentryTelemetryService {
+ EvaluateAndApply() void
+ CaptureException(ex, context) void
+ AddBreadcrumb(message, category, level) void
+ WithInstrumentation(name, action) T?
+ WithInstrumentationAsync(name, action) Task<T?>
}
class McpTransportMode {
<<enum>>
StreamableHttp
Sse
}
McpServerManager --> SentryTelemetryService : "遥测上报"
McpServerManager --> McpTransportMode : "选择传输模式"
McpServerManager --> EchoCaveTool : "注册工具"
McpServerManager --> AddComponentTextTool : "注册工具"
McpServerManager --> SetComponentTextTool : "注册工具"
McpServerManager --> ListComponentTextTool : "注册工具"
McpServerManager --> DeleteComponentTextTool : "注册工具"
McpServerManager --> ClearComponentTextTool : "注册工具"
```

**图表来源**
- [McpServerManager.cs:11-23](file://Mcp/McpServerManager.cs#L11-L23)
- [McpServerManager.cs:25-112](file://Mcp/McpServerManager.cs#L25-L112)
- [EchoCaveTool.cs:17-46](file://Mcp/Tools/EchoCaveTool.cs#L17-L46)
- [AddComponentTextTool.cs:18-41](file://Mcp/Tools/AddComponentTextTool.cs#L18-L41)
- [SetComponentTextTool.cs:17-39](file://Mcp/Tools/SetComponentTextTool.cs#L17-L39)
- [ListComponentTextTool.cs:18-35](file://Mcp/Tools/ListComponentTextTool.cs#L18-L35)
- [DeleteComponentTextTool.cs:18-39](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L39)
- [ClearComponentTextTool.cs:18-41](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L41)
- [SentryTelemetryService.cs:30-174](file://Services/SentryTelemetryService.cs#L30-L174)
- [McpTransportMode.cs:6-17](file://Models/McpTransportMode.cs#L6-L17)

章节来源
- [McpServerManager.cs:25-112](file://Mcp/McpServerManager.cs#L25-L112)

#### StartAsync 方法详解
- 参数
  - port：本地监听端口
  - transportMode：传输模式（SSE 或 StreamableHttp）
- 行为
  - 幂等保护：若已有运行中的服务器，直接返回
  - 创建取消令牌源
  - 构建服务器：注册工具集、设置 JSON 序列化上下文
  - 根据传输模式选择端点与 SSE 兼容性
  - 启动服务器并记录日志
  - 成功时以 Ok 结束事务；失败时捕获异常并以内错结束事务
- 异常处理
  - 捕获所有异常，上报 Sentry，并重新抛出
- 性能考虑
  - 幂等避免重复绑定端口
  - 取消令牌支持优雅退出
  - 构造阶段尽量轻量，避免阻塞

章节来源
- [McpServerManager.cs:25-82](file://Mcp/McpServerManager.cs#L25-L82)

#### StopAsync 方法详解
- 行为
  - 取消令牌，触发底层停止
  - 等待停止完成后置空引用并释放取消令牌源
  - 记录日志并上报事务结果
- 异常处理
  - 捕获异常，上报 Sentry，并重新抛出
- 性能考虑
  - 顺序执行，避免并发停止导致的状态不一致
  - 释放资源及时，防止句柄泄漏

章节来源
- [McpServerManager.cs:84-112](file://Mcp/McpServerManager.cs#L84-L112)

#### 工具注册机制
**更新** 工具注册机制现已支持多种类型的工具，包括 AI 文字管理工具和回声洞穴功能：

- **AI 文字管理工具**：
  - `add_component_text`：新增 AI 文字条目，支持 ID、文本内容和描述
  - `set_component_text`：更新指定 ID 的 AI 文字内容
  - `list_component_text`：列出所有 AI 文字条目及其详细信息
  - `delete_component_text`：删除指定的 AI 文字条目
  - `clear_component_text`：清空指定或全部条目的文字内容

- **回声洞穴工具**：
  - `get_echo_cave`：从嵌入的资源文件中随机抽取一条内容返回

- **其他工具**：
  - 课程查询工具（LessonTools）
  - 课表操作工具（ScheduleTools）
  - 通知发送工具（SendNotificationTool）
  - 课程交换工具（SwapClassesTool）
  - 按日期获取课表工具（GetScheduleByDateTool）

所有工具都实现了统一的 IMcpServerTool 接口，提供标准化的工具定义和调用接口。

章节来源
- [McpServerManager.cs:41-55](file://Mcp/McpServerManager.cs#L41-L55)
- [EchoCaveTool.cs:27-46](file://Mcp/Tools/EchoCaveTool.cs#L27-L46)
- [AddComponentTextTool.cs:32-41](file://Mcp/Tools/AddComponentTextTool.cs#L32-L41)
- [SetComponentTextTool.cs:30-39](file://Mcp/Tools/SetComponentTextTool.cs#L30-L39)
- [ListComponentTextTool.cs:26-35](file://Mcp/Tools/ListComponentTextTool.cs#L26-L35)
- [DeleteComponentTextTool.cs:30-39](file://Mcp/Tools/DeleteComponentTextTool.cs#L30-L39)
- [ClearComponentTextTool.cs:32-41](file://Mcp/Tools/ClearComponentTextTool.cs#L32-L41)

### AI 文字管理工具详解
**新增** AI 文字管理工具提供了完整的 CRUD 操作，用于管理 ClassIsland 主界面上的 AI 文字组件内容。

#### 工具特性
- **线程安全**：所有 UI 操作都通过 UiThreadHelper 在 UI 线程上执行
- **错误处理**：完善的异常捕获和错误消息返回
- **遥测集成**：每个工具调用都记录面包屑信息到 Sentry
- **JSON 序列化**：使用强类型结果模型确保数据传输的一致性

#### 工具列表
| 工具名称 | 功能描述 | 必需参数 | 可选参数 |
|---------|---------|----------|----------|
| add_component_text | 新增 AI 文字条目 | id | text, description |
| set_component_text | 更新 AI 文字内容 | id, text | - |
| list_component_text | 列出所有条目 | - | - |
| delete_component_text | 删除指定条目 | id | - |
| clear_component_text | 清空文字内容 | id (支持 "all") | - |

章节来源
- [AddComponentTextTool.cs:43-90](file://Mcp/Tools/AddComponentTextTool.cs#L43-L90)
- [SetComponentTextTool.cs:41-72](file://Mcp/Tools/SetComponentTextTool.cs#L41-L72)
- [ListComponentTextTool.cs:37-65](file://Mcp/Tools/ListComponentTextTool.cs#L37-L65)
- [DeleteComponentTextTool.cs:41-81](file://Mcp/Tools/DeleteComponentTextTool.cs#L41-L81)
- [ClearComponentTextTool.cs:43-101](file://Mcp/Tools/ClearComponentTextTool.cs#L43-L101)

### 回声洞穴工具详解
**新增** 回声洞穴工具提供了一个有趣的功能，从嵌入的资源文件中随机抽取一条内容返回。

#### 功能特点
- **随机内容**：从 echo-cave.txt 资源文件中随机选择一行
- **资源管理**：正确处理资源文件的读取和流的生命周期
- **错误处理**：当资源不存在或为空时返回友好的错误消息
- **只读操作**：标记为只读工具，不会修改任何状态

#### 工作流程
1. 获取嵌入的资源流
2. 读取全部内容并按行分割
3. 验证内容不为空
4. 随机选择一行内容
5. 返回结构化结果

章节来源
- [EchoCaveTool.cs:48-92](file://Mcp/Tools/EchoCaveTool.cs#L48-L92)

### SentryTelemetryService 集成
- 作用
  - 根据设置动态初始化/关闭 Sentry SDK
  - 提供异常捕获、面包屑、事务包装等 API
- 与 McpServerManager 的协作
  - 在 StartAsync/StopAsync 中创建事务，成功/失败分别标记状态
  - 异常发生时统一上报，附带上下文信息
- 关键特性
  - 支持异步/同步包装器，自动添加面包屑与事务
  - 可配置采样率、PII 策略、标签等

**更新** 所有新增工具都集成了 Sentry 遥测，每个工具调用都会记录详细的执行信息和异常数据。

章节来源
- [SentryTelemetryService.cs:30-75](file://Services/SentryTelemetryService.cs#L30-L75)
- [SentryTelemetryService.cs:95-174](file://Services/SentryTelemetryService.cs#L95-L174)
- [McpServerManager.cs:32-81](file://Mcp/McpServerManager.cs#L32-L81)
- [McpServerManager.cs:86-111](file://Mcp/McpServerManager.cs#L86-L111)

### 插件生命周期与服务器管理
- 应用启动时：
  - 读取设置，初始化遥测服务
  - 若启用 MCP，则创建 McpServerManager 并调用 StartAsync
  - 记录启动日志与面包屑
- 应用停止时：
  - 调用 StopAsync 停止服务器
  - 记录停止日志与面包屑
- 资源释放：
  - 插件 Dispose 中释放 McpServerManager 与遥测服务

章节来源
- [Plugin.cs:55-97](file://Plugin.cs#L55-L97)

## 依赖关系分析
- 组件耦合
  - McpServerManager 依赖 McpServerBuilder 与传输选项，低耦合于具体工具实现
  - 遥测服务为可选依赖，未启用时不影响核心功能
- 外部依赖
  - DotNetCampus.ModelContextProtocol.Servers 与 Transports.Http
  - Microsoft.Extensions.Logging
  - Sentry SDK
- 可能的循环依赖
  - 当前无循环依赖迹象

**更新** 新增了工具与结果模型之间的依赖关系，所有工具都依赖于统一的 JSON 序列化上下文。

```mermaid
graph LR
Manager["McpServerManager.cs"] --> Builder["McpServerBuilder (外部库)"]
Manager --> Transport["LocalHostHttpServerTransportOptions (外部库)"]
Manager --> Logger["ILogger (Microsoft.Extensions.Logging)"]
Manager --> Telemetry["SentryTelemetryService.cs"]
Manager --> Tools["工具集合"]
Tools --> EchoTool["EchoCaveTool.cs"]
Tools --> AddTool["AddComponentTextTool.cs"]
Tools --> SetTextTool["SetComponentTextTool.cs"]
Tools --> ListTool["ListComponentTextTool.cs"]
Tools --> DeleteTool["DeleteComponentTextTool.cs"]
Tools --> ClearTool["ClearComponentTextTool.cs"]
Tools --> ToolResults["ToolResults.cs"]
Tools --> JsonContext["AgentIslandJsonContext.cs"]
Plugin["Plugin.cs"] --> Manager
Plugin --> Telemetry
UI["McpSettingsPage.axaml"] --> Settings["AgentIslandSettings.cs"]
Plugin --> Settings
```

**图表来源**
- [McpServerManager.cs:1-8](file://Mcp/McpServerManager.cs#L1-L8)
- [Plugin.cs:1-16](file://Plugin.cs#L1-L16)
- [McpSettingsPage.axaml:25-49](file://Views/SettingsPages/McpSettingsPage.axaml#L25-L49)
- [AgentIslandSettings.cs:204-211](file://Models/AgentIslandSettings.cs#L204-L211)
- [ToolResults.cs:55-69](file://Models/ToolResults.cs#L55-L69)
- [AgentIslandJsonContext.cs:14-21](file://Models/AgentIslandJsonContext.cs#L14-L21)

章节来源
- [McpServerManager.cs:1-8](file://Mcp/McpServerManager.cs#L1-L8)
- [Plugin.cs:1-16](file://Plugin.cs#L1-L16)
- [McpSettingsPage.axaml:25-49](file://Views/SettingsPages/McpSettingsPage.axaml#L25-L49)
- [AgentIslandSettings.cs:204-211](file://Models/AgentIslandSettings.cs#L204-L211)

## 性能与并发考虑
- 幂等启动：避免重复绑定端口导致的异常与资源浪费
- 取消令牌：支持优雅退出，减少强制终止带来的资源残留
- 事务粒度：将启动/停止作为独立事务，便于定位慢路径与异常
- 工具注册：在构建期一次性注册，避免运行时开销
- 线程安全：所有 UI 操作都通过 UiThreadHelper 在 UI 线程上执行
- 建议
  - 在高并发场景下，确保工具实现内部线程安全
  - 对耗时工具调用使用 Sentry 的 WithInstrumentationAsync 包裹，获取延迟指标
  - 合理使用缓存机制，避免频繁的文件 I/O 操作

**更新** 新增了关于线程安全和 UI 操作的性能考虑，确保所有涉及 UI 的操作都在正确的线程上执行。

## 故障排查指南
- 端口冲突
  - 现象：启动时报端口占用异常
  - 排查：确认端口未被其他进程占用；调整端口号后重启
  - 参考：StartAsync 在异常分支会捕获并上报异常
- 服务器状态管理
  - 现象：多次启动无效或状态不一致
  - 排查：检查幂等逻辑；确保 StopAsync 被正确调用
- 资源泄漏预防
  - 现象：进程退出后仍持有句柄
  - 排查：确认 Dispose 调用链；StopAsync 中释放取消令牌源
- 遥测未上报
  - 现象：Sentry 无数据
  - 排查：检查遥测开关与隐私协议；确认 DSN 有效；查看 EvaluateAndApply 的执行路径
- **新增** 工具执行问题
  - 现象：工具调用失败或返回错误
  - 排查：检查工具参数格式；查看 Sentry 面包屑信息；确认 UI 线程访问正常
- **新增** AI 文字管理问题
  - 现象：无法添加或删除条目
  - 排查：检查 ID 唯一性；确认 UI 线程访问；查看日志中的错误消息

**更新** 新增了针对新工具的故障排查指南，特别是 AI 文字管理和回声洞穴工具的相关问题。

章节来源
- [McpServerManager.cs:76-81](file://Mcp/McpServerManager.cs#L76-L81)
- [McpServerManager.cs:106-111](file://Mcp/McpServerManager.cs#L106-L111)
- [SentryTelemetryService.cs:30-75](file://Services/SentryTelemetryService.cs#L30-L75)
- [AddComponentTextTool.cs:85-89](file://Mcp/Tools/AddComponentTextTool.cs#L85-L89)
- [EchoCaveTool.cs:85-91](file://Mcp/Tools/EchoCaveTool.cs#L85-L91)

## 结论
McpServerManager 提供了简洁可靠的 MCP 服务器管理能力，涵盖生命周期、传输模式与工具注册，并与 SentryTelemetryService 无缝集成以实现监控与错误追踪。通过幂等设计、取消令牌与有序资源释放，系统在稳定性与可维护性方面表现良好。结合设置界面与插件生命周期，用户可以灵活配置传输模式与端口，快速启用 MCP 服务。

**更新** 新增的 AI 文字管理工具和回声洞穴功能进一步丰富了 MCP 服务器的能力，提供了完整的文本管理和娱乐功能。所有工具都遵循统一的设计模式，具备良好的可扩展性和维护性。

## 附录：配置与使用示例

### 传输模式配置与连接地址
- 传输模式
  - SSE：端点为 “sse”，兼容 SSE
  - Streamable HTTP：端点为 “mcp”，不兼容 SSE
- 连接地址计算
  - 根据端口与传输模式生成 http://localhost:{Port}/{EndPoint}

章节来源
- [McpTransportMode.cs:6-17](file://Models/McpTransportMode.cs#L6-L17)
- [AgentIslandSettings.cs:204-211](file://Models/AgentIslandSettings.cs#L204-L211)

### 在插件中启动与停止服务器
- 启动
  - 读取设置中的端口与传输模式
  - 创建 McpServerManager 并调用 StartAsync
  - 记录日志与面包屑
- 停止
  - 在应用停止事件中调用 StopAsync
  - 记录日志与面包屑

章节来源
- [Plugin.cs:65-97](file://Plugin.cs#L65-L97)

### 设置界面要点
- 端口范围：1-65535
- 传输模式：当前默认启用 Streamable HTTP，SSE 在界面中被禁用（仅展示）

章节来源
- [McpSettingsPage.axaml:25-49](file://Views/SettingsPages/McpSettingsPage.axaml#L25-L49)

### 工具使用示例
**新增** 以下是新增工具的使用示例：

#### AI 文字管理工具示例
```json
// 添加新的 AI 文字条目
{
  "name": "add_component_text",
  "arguments": {
    "id": "greeting",
    "text": "你好，欢迎使用 AI 文字组件！",
    "description": "问候语示例"
  }
}

// 更新现有条目的内容
{
  "name": "set_component_text", 
  "arguments": {
    "id": "greeting",
    "text": "早上好！今天也是美好的一天。"
  }
}

// 列出所有条目
{
  "name": "list_component_text",
  "arguments": {}
}

// 清空所有条目的内容
{
  "name": "clear_component_text",
  "arguments": {
    "id": "all"
  }
}
```

#### 回声洞穴工具示例
```json
// 从回声洞获取随机内容
{
  "name": "get_echo_cave",
  "arguments": {}
}
```

**更新** 新增了完整的新增工具使用示例，展示了如何调用 AI 文字管理工具和回声洞穴功能。