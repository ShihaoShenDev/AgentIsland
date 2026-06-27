# Add Logging to Core Modules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `ILogger<T>` logging (Debug + Info mix) to all core modules that currently lack it.

**Architecture:** Follow existing pattern from `Plugin.cs` and `RunAcpAction.cs` — `Microsoft.Extensions.Logging.ILogger<T>`, constructor-injected with `= null` default, null-conditional `?.` calls.

**Tech Stack:** `Microsoft.Extensions.Logging` (transitive via ClassIsland.PluginSdk)

## Global Constraints

- Use `Microsoft.Extensions.Logging` (`ILogger<T>`) — no new NuGet packages
- Follow existing pattern: nullable logger field, `?.` null-conditional calls
- Debug level: detailed execution flow, parameters, state transitions
- Info level: significant operations (start, stop, connect, complete)
- No log calls inside tight loops or high-frequency paths
- Chinese log messages (consistent with existing `RunAcpAction.cs` style)

## File Structure

| File | Action |
|---|---|
| `Mcp/McpServerManager.cs` | Add ILogger, log server lifecycle |
| `Mcp/Tools/LessonTools.cs` | Add ILogger, log tool invocations |
| `Mcp/Tools/ScheduleTools.cs` | Add ILogger, log schedule operations |
| `Mcp/Tools/SwapClassesTool.cs` | Add ILogger, log swap operations |
| `Mcp/Tools/GetScheduleByDateTool.cs` | Add ILogger, log query operations |
| `Mcp/Tools/SendNotificationTool.cs` | Add ILogger, log notification sends |
| `Mcp/Tools/SetComponentTextTool.cs` | Add ILogger, log text updates |
| `Mcp/Tools/AgentIslandNotificationProvider.cs` | Add ILogger, log notification provider |
| `Services/AcpRunnerService.cs` | Add ILogger, log agent lifecycle + fix swallowed exceptions |

---

### Task 1: McpServerManager — Server lifecycle logging

**Files:**
- Modify: `Mcp/McpServerManager.cs`

**Interfaces:**
- Consumes: nothing new
- Produces: ILogger<McpServerManager> for MCP server lifecycle

- [ ] **Step 1: Add logging to McpServerManager**

Add `using Microsoft.Extensions.Logging;` and `_logger` field. Log:
- Info: server starting with transport/port
- Info: server stopped
- Debug: tool registration

```csharp
using Microsoft.Extensions.Logging;

// Add field
private readonly ILogger<McpServerManager>? _logger;

// Add constructor parameter
public McpServerManager(AgentIslandSettings settings, ILogger<McpServerManager>? logger = null)
{
    _logger = logger;
    // ... existing code
}

// In StartAsync:
_logger?.LogInformation("MCP 服务器启动中，传输模式: {TransportMode}，端口: {Port}", _settings.TransportMode, _settings.Port);
// ... after server.StartAsync():
_logger?.LogInformation("MCP 服务器已启动");

// In StopAsync:
_logger?.LogInformation("MCP 服务器停止中");
// ... after:
_logger?.LogInformation("MCP 服务器已停止");
```

- [ ] **Step 2: Build and verify**

Run: `.\build-debug.ps1`
Expected: Build succeeds, no warnings from McpServerManager.cs

---

### Task 2: MCP Tools — LessonTools, ScheduleTools, SwapClassesTool

**Files:**
- Modify: `Mcp/Tools/LessonTools.cs`
- Modify: `Mcp/Tools/ScheduleTools.cs`
- Modify: `Mcp/Tools/SwapClassesTool.cs`

**Interfaces:**
- Consumes: ILogger<T> via constructor
- Produces: debug-level logs for each tool invocation

- [ ] **Step 1: Add logging to LessonTools.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<LessonTools>? _logger;

public LessonTools(ILogger<LessonTools>? logger = null)
{
    _logger = logger;
}

// In each method:
_logger?.LogDebug("调用 get_current_class");
_logger?.LogDebug("调用 get_next_class");
_logger?.LogDebug("调用 get_time_status");
```

- [ ] **Step 2: Add logging to ScheduleTools.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<ScheduleTools>? _logger;

public ScheduleTools(ILogger<ScheduleTools>? logger = null)
{
    _logger = logger;
}

// In GetTodaySchedule:
_logger?.LogDebug("获取今日课程表");
// In GetScheduleByDate:
_logger?.LogDebug("获取 {Date} 的课程表", date);
// In SwapClasses:
_logger?.LogInformation("交换课程: {Class1} <-> {Class2}", class1Name, class2Name);
// In ListSubjects:
_logger?.LogDebug("列出所有科目");
```

- [ ] **Step 3: Add logging to SwapClassesTool.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<SwapClassesTool>? _logger;

public SwapClassesTool(ILogger<SwapClassesTool>? logger = null)
{
    _logger = logger;
}

// In SwapClasses method:
_logger?.LogDebug("调用 swap_classes, 参数: {Args}", args);
```

- [ ] **Step 4: Build and verify**

Run: `.\build-debug.ps1`
Expected: Build succeeds

---

### Task 3: MCP Tools — GetScheduleByDateTool, SendNotificationTool, SetComponentTextTool, NotificationProvider

**Files:**
- Modify: `Mcp/Tools/GetScheduleByDateTool.cs`
- Modify: `Mcp/Tools/SendNotificationTool.cs`
- Modify: `Mcp/Tools/SetComponentTextTool.cs`
- Modify: `Mcp/Tools/AgentIslandNotificationProvider.cs`

**Interfaces:**
- Consumes: ILogger<T> via constructor
- Produces: debug-level logs for each operation

- [ ] **Step 1: Add logging to GetScheduleByDateTool.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<GetScheduleByDateTool>? _logger;

public GetScheduleByDateTool(ILogger<GetScheduleByDateTool>? logger = null)
{
    _logger = logger;
}

// In GetScheduleByDate method:
_logger?.LogDebug("调用 get_schedule_by_date, 日期: {Date}", date);
```

- [ ] **Step 2: Add logging to SendNotificationTool.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<SendNotificationTool>? _logger;

public SendNotificationTool(ILogger<SendNotificationTool>? logger = null)
{
    _logger = logger;
}

// In SendNotification method:
_logger?.LogDebug("调用 send_notification");
_logger?.LogInformation("发送通知: {Message}", message);
```

- [ ] **Step 3: Add logging to SetComponentTextTool.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<SetComponentTextTool>? _logger;

public SetComponentTextTool(ILogger<SetComponentTextTool>? logger = null)
{
    _logger = logger;
}

// In SetComponentText method:
_logger?.LogDebug("调用 set_component_text, 组件ID: {ComponentId}", componentId);
_logger?.LogInformation("设置组件文本: {ComponentId} -> {Text}", componentId, text);
```

- [ ] **Step 4: Add logging to AgentIslandNotificationProvider.cs**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<AgentIslandNotificationProvider>? _logger;

// Add logger parameter to constructor or Initialize method
_logger?.LogDebug("通知提供方已初始化");
```

- [ ] **Step 5: Build and verify**

Run: `.\build-debug.ps1`
Expected: Build succeeds

---

### Task 4: AcpRunnerService — Agent lifecycle logging + fix swallowed exceptions

**Files:**
- Modify: `Services/AcpRunnerService.cs`

**Interfaces:**
- Consumes: ILogger<AcpRunnerService> via constructor
- Produces: info/debug logs for agent lifecycle, proper exception logging

- [ ] **Step 1: Add logging to AcpRunnerService**

```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<AcpRunnerService>? _logger;

// Add to constructor or field initialization
public AcpRunnerService(ILogger<AcpRunnerService>? logger = null)
{
    _logger = logger;
}

// In RunAgentAsync:
_logger?.LogInformation("启动 ACP Agent: {AgentName}", agent.Name);
_logger?.LogDebug("Agent 命令: {Command} {Arguments}", agent.Command, agent.Arguments);
_logger?.LogInformation("Agent 已启动, SessionId: {SessionId}", sessionId);
_logger?.LogError(ex, "启动 Agent 失败: {AgentName}", agent.Name);

// In SendPromptAsync:
_logger?.LogDebug("向 Session {SessionId} 发送 Prompt", sessionId);
_logger?.LogError(ex, "发送 Prompt 失败: {SessionId}", sessionId);

// In Dispose:
_logger?.LogInformation("关闭所有 ACP 会话");
// Replace empty catch with:
catch (Exception ex)
{
    _logger?.LogError(ex, "停止 ACP 会话时出错");
}
```

- [ ] **Step 2: Build and verify**

Run: `.\build-debug.ps1`
Expected: Build succeeds, no warnings

---

### Task 5: Final verification

- [ ] **Step 1: Full build**

Run: `.\build-debug.ps1`
Expected: Build succeeds with zero errors

- [ ] **Step 2: Verify no missed files**

Grep for `IMcpServerTool` and `ILogger` across all .cs files to confirm all tool classes have logging.
