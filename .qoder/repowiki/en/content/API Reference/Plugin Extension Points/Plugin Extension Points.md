# Plugin Extension Points

<cite>
**Referenced Files in This Document**
- [Plugin.cs](file://Plugin.cs)
- [AgentIsland.csproj](file://AgentIsland.csproj)
- [manifest.yml](file://manifest.yml)
- [AiTextComponent.axaml](file://Components/AiTextComponent.axaml)
- [AiTextComponent.axaml.cs](file://Components/AiTextComponent.axaml.cs)
- [AiTextComponentSettingsControl.axaml.cs](file://Components/AiTextComponentSettingsControl.axaml.cs)
- [AiTextComponentSettings.cs](file://Models/AiTextComponentSettings.cs)
- [AiTextEntry.cs](file://Models/AiTextEntry.cs)
- [AgentIslandSettings.cs](file://Models/AgentIslandSettings.cs)
- [SentryTelemetryService.cs](file://Services/SentryTelemetryService.cs)
- [AcpRunnerService.cs](file://Services/AcpRunnerService.cs)
- [RunAcpAction.cs](file://Automation/RunAcpAction.cs)
- [McpServerManager.cs](file://Mcp/McpServerManager.cs)
- [SetComponentTextTool.cs](file://Mcp/Tools/SetComponentTextTool.cs)
- [AgentIslandNotificationProvider.cs](file://Mcp/Tools/AgentIslandNotificationProvider.cs)
- [AiTextSettingsPage.axaml.cs](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs)
</cite>

## Table of Contents
1. Introduction
2. Project Structure
3. Core Components
4. Architecture Overview
5. Detailed Component Analysis
6. Dependency Analysis
7. Performance Considerations
8. Troubleshooting Guide
9. Conclusion

## Introduction
This document explains AgentIsland’s plugin extension points for ClassIsland, focusing on:
- Plugin lifecycle and dependency injection setup
- Service registration patterns
- Custom UI components (e.g., AiTextComponent) with Avalonia XAML integration and property binding
- Action system extensions for automation workflows
- Settings pages with reactive properties
- Event handling mechanisms
- MCP tool integration and notifications

It provides code-level diagrams and references to source files so you can implement custom plugins, register services, and integrate with existing AgentIsland functionality.

## Project Structure
AgentIsland is a ClassIsland plugin that registers services, UI components, actions, settings pages, and an MCP server. The plugin entry point is marked by an attribute and implements the standard lifecycle methods.

```mermaid
graph TB
A["Plugin.cs<br/>Plugin class"] --> B["Services<br/>SentryTelemetryService.cs<br/>AcpRunnerService.cs"]
A --> C["Components<br/>AiTextComponent.axaml(.cs)<br/>AiTextComponentSettingsControl.axaml.cs"]
A --> D["Settings Pages<br/>AiTextSettingsPage.axaml.cs"]
A --> E["Actions<br/>RunAcpAction.cs"]
A --> F["MCP Server<br/>McpServerManager.cs"]
F --> G["MCP Tools<br/>SetComponentTextTool.cs"]
G --> H["Notification Provider<br/>AgentIslandNotificationProvider.cs"]
A --> I["Models<br/>AgentIslandSettings.cs<br/>AiTextComponentSettings.cs<br/>AiTextEntry.cs"]
J["manifest.yml"] -.-> A
K["AgentIsland.csproj"] -.-> A
```

**Diagram sources**
- [Plugin.cs:19-53](file://Plugin.cs#L19-L53)
- [SentryTelemetryService.cs:11-40](file://Services/SentryTelemetryService.cs#L11-L40)
- [AcpRunnerService.cs:14-30](file://Services/AcpRunnerService.cs#L14-L30)
- [AiTextComponent.axaml.cs:16-56](file://Components/AiTextComponent.axaml.cs#L16-L56)
- [AiTextComponentSettingsControl.axaml.cs:7-27](file://Components/AiTextComponentSettingsControl.axaml.cs#L7-L27)
- [AiTextSettingsPage.axaml.cs:9-20](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L9-L20)
- [RunAcpAction.cs:10-27](file://Automation/RunAcpAction.cs#L10-L27)
- [McpServerManager.cs:11-30](file://Mcp/McpServerManager.cs#L11-L30)
- [SetComponentTextTool.cs:17-40](file://Mcp/Tools/SetComponentTextTool.cs#L17-L40)
- [AgentIslandNotificationProvider.cs:10-25](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L10-L25)
- [AgentIslandSettings.cs:13-32](file://Models/AgentIslandSettings.cs#L13-L32)
- [AiTextComponentSettings.cs:5-12](file://Models/AiTextComponentSettings.cs#L5-L12)
- [AiTextEntry.cs:5-18](file://Models/AiTextEntry.cs#L5-L18)
- [manifest.yml:1-13](file://manifest.yml#L1-L13)
- [AgentIsland.csproj:22-29](file://AgentIsland.csproj#L22-L29)

**Section sources**
- [Plugin.cs:19-53](file://Plugin.cs#L19-L53)
- [manifest.yml:1-13](file://manifest.yml#L1-L13)
- [AgentIsland.csproj:22-29](file://AgentIsland.csproj#L22-L29)

## Core Components
- Plugin lifecycle and DI registration:
  - Initialize loads settings, wires telemetry, registers services, components, settings pages, and actions, and subscribes to app events.
  - Start behavior is driven by AppStarted event; Stop behavior by AppStopping event.
  - Dispose unsubscribes and disposes resources.
- Services:
  - SentryTelemetryService manages SDK initialization/shutdown based on settings and exposes instrumentation helpers.
  - AcpRunnerService orchestrates external agent processes via stdio JSON-RPC.
- UI components:
  - AiTextComponent renders text or placeholder based on selected entry and settings.
  - AiTextComponentSettingsControl binds to component settings and updates selection.
- Actions:
  - RunAcpAction invokes AcpRunnerService after validating settings and permissions.
- MCP server:
  - McpServerManager builds and starts/stops the server, registering tools.
  - SetComponentTextTool updates AI text entries from MCP calls.
  - AgentIslandNotificationProvider posts notifications through ClassIsland channels.

**Section sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [Plugin.cs:55-97](file://Plugin.cs#L55-L97)
- [Plugin.cs:99-112](file://Plugin.cs#L99-L112)
- [SentryTelemetryService.cs:21-40](file://Services/SentryTelemetryService.cs#L21-L40)
- [AcpRunnerService.cs:25-77](file://Services/AcpRunnerService.cs#L25-L77)
- [AiTextComponent.axaml.cs:36-56](file://Components/AiTextComponent.axaml.cs#L36-L56)
- [AiTextComponentSettingsControl.axaml.cs:16-27](file://Components/AiTextComponentSettingsControl.axaml.cs#L16-L27)
- [RunAcpAction.cs:29-82](file://Automation/RunAcpAction.cs#L29-L82)
- [McpServerManager.cs:25-82](file://Mcp/McpServerManager.cs#L25-L82)
- [SetComponentTextTool.cs:41-72](file://Mcp/Tools/SetComponentTextTool.cs#L41-L72)
- [AgentIslandNotificationProvider.cs:27-50](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L27-L50)

## Architecture Overview
The plugin integrates with ClassIsland via attributes and DI, exposing UI components, actions, settings pages, and an MCP server. Telemetry is optional and controlled by settings.

```mermaid
graph TB
subgraph "ClassIsland Host"
CI["ClassIsland Runtime"]
end
subgraph "AgentIsland Plugin"
P["Plugin.cs"]
S1["SentryTelemetryService.cs"]
S2["AcpRunnerService.cs"]
C1["AiTextComponent.axaml(.cs)"]
C2["AiTextComponentSettingsControl.axaml.cs"]
SP["AiTextSettingsPage.axaml.cs"]
ACT["RunAcpAction.cs"]
MCP["McpServerManager.cs"]
TOOL["SetComponentTextTool.cs"]
NOTIF["AgentIslandNotificationProvider.cs"]
M1["AgentIslandSettings.cs"]
M2["AiTextComponentSettings.cs"]
M3["AiTextEntry.cs"]
end
CI --> P
P --> S1
P --> S2
P --> C1
P --> C2
P --> SP
P --> ACT
P --> MCP
MCP --> TOOL
TOOL --> NOTIF
P --> M1
C1 --> M2
C1 --> M3
C2 --> M2
SP --> M1
```

**Diagram sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [SentryTelemetryService.cs:11-40](file://Services/SentryTelemetryService.cs#L11-L40)
- [AcpRunnerService.cs:14-30](file://Services/AcpRunnerService.cs#L14-L30)
- [AiTextComponent.axaml.cs:16-56](file://Components/AiTextComponent.axaml.cs#L16-L56)
- [AiTextComponentSettingsControl.axaml.cs:7-27](file://Components/AiTextComponentSettingsControl.axaml.cs#L7-L27)
- [AiTextSettingsPage.axaml.cs:9-20](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L9-L20)
- [RunAcpAction.cs:10-27](file://Automation/RunAcpAction.cs#L10-L27)
- [McpServerManager.cs:11-30](file://Mcp/McpServerManager.cs#L11-L30)
- [SetComponentTextTool.cs:17-40](file://Mcp/Tools/SetComponentTextTool.cs#L17-L40)
- [AgentIslandNotificationProvider.cs:10-25](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L10-L25)
- [AgentIslandSettings.cs:13-32](file://Models/AgentIslandSettings.cs#L13-L32)
- [AiTextComponentSettings.cs:5-12](file://Models/AiTextComponentSettings.cs#L5-L12)
- [AiTextEntry.cs:5-18](file://Models/AiTextEntry.cs#L5-L18)

## Detailed Component Analysis

### Plugin Lifecycle and Dependency Injection
- Initialize:
  - Loads settings from disk and persists changes automatically.
  - Creates and registers telemetry service.
  - Registers singleton services (settings, telemetry, AcpRunnerService).
  - Adds notification provider, UI component, and multiple settings pages.
  - Adds action type with its settings control.
  - Subscribes to application start/stop events.
- Start:
  - On AppStarted, if enabled, constructs logger and MCP manager, then starts the server.
- Stop:
  - On AppStopping, stops the MCP server gracefully.
- Dispose:
  - Unsubscribes from app events and disposes managed resources.

```mermaid
sequenceDiagram
participant Host as "ClassIsland Host"
participant Plugin as "Plugin.cs"
participant Telemetry as "SentryTelemetryService.cs"
participant MCPServer as "McpServerManager.cs"
Host->>Plugin : Initialize(context, services)
Plugin->>Plugin : Load settings and wire persistence
Plugin->>Telemetry : Create and EvaluateAndApply()
Plugin->>Host : Register services/pages/components/actions
Plugin->>Host : Subscribe to AppStarted/AppStopping
Host-->>Plugin : AppStarted
Plugin->>MCPServer : StartAsync(port, mode)
MCPServer-->>Plugin : Started or error
Host-->>Plugin : AppStopping
Plugin->>MCPServer : StopAsync()
MCPServer-->>Plugin : Stopped
```

**Diagram sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [Plugin.cs:55-79](file://Plugin.cs#L55-L79)
- [Plugin.cs:81-97](file://Plugin.cs#L81-L97)
- [SentryTelemetryService.cs:30-40](file://Services/SentryTelemetryService.cs#L30-L40)
- [McpServerManager.cs:25-82](file://Mcp/McpServerManager.cs#L25-L82)

**Section sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [Plugin.cs:55-97](file://Plugin.cs#L55-L97)
- [Plugin.cs:99-112](file://Plugin.cs#L99-L112)

### Service Registration Patterns and DI Setup
- Singleton registrations:
  - Settings object (global configuration).
  - Telemetry service (lifecycle-aware).
  - AcpRunnerService (process orchestration).
- Notification provider registration:
  - Adds a notification provider implementation.
- Component registration:
  - Registers a custom component and its settings control.
- Settings pages:
  - Registers multiple settings pages for different features.
- Actions:
  - Registers an action with its settings control.

```mermaid
classDiagram
class Plugin {
+Initialize(context, services)
+Dispose()
}
class AgentIslandSettings
class SentryTelemetryService
class AcpRunnerService
class NotificationProviderBase
class ComponentBase~TSettings~
class SettingsPageBase
class ActionBase~TSettings~
Plugin --> AgentIslandSettings : "registers singleton"
Plugin --> SentryTelemetryService : "creates & registers"
Plugin --> AcpRunnerService : "registers singleton"
Plugin --> NotificationProviderBase : "adds provider"
Plugin --> ComponentBase : "adds component"
Plugin --> SettingsPageBase : "adds pages"
Plugin --> ActionBase : "adds action"
```

**Diagram sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [SentryTelemetryService.cs:11-40](file://Services/SentryTelemetryService.cs#L11-L40)
- [AcpRunnerService.cs:14-30](file://Services/AcpRunnerService.cs#L14-L30)
- [AgentIslandNotificationProvider.cs:10-25](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L10-L25)
- [AiTextComponent.axaml.cs:16-20](file://Components/AiTextComponent.axaml.cs#L16-L20)
- [AiTextSettingsPage.axaml.cs:9-14](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L9-L14)
- [RunAcpAction.cs:10-16](file://Automation/RunAcpAction.cs#L10-L16)

**Section sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)

### Custom UI Component: AiTextComponent
- Purpose:
  - Displays dynamic text content bound to an entry in settings, with a placeholder when empty.
- Implementation highlights:
  - Inherits from ComponentBase<TSettings>.
  - Exposes Avalonia StyledProperties for ResolvedText and PlaceholderText.
  - Wires collection and property change events to update display.
  - Uses RelativeSource bindings in XAML to bind to component properties.
- Settings control:
  - Binds a combo box to the list of entries and updates the component’s EntryId setting.

```mermaid
classDiagram
class AiTextComponent {
+ResolvedText : string
+PlaceholderText : string
+AiTextComponent()
-UpdateText()
}
class AiTextComponentSettings {
+EntryId : string
+PlaceholderText : string
}
class AiTextEntry {
+Id : string
+Description : string
+Text : string
+DisplayName : string
}
class AiTextComponentSettingsControl {
+AiTextComponentSettingsControl()
-RefreshItems()
-SyncSelection()
}
class AgentIslandSettings {
+AiTextEntries : ObservableCollection~AiTextEntry~
}
AiTextComponent --> AiTextComponentSettings : "inherits settings"
AiTextComponent --> AiTextEntry : "reads/writes via settings"
AiTextComponentSettingsControl --> AiTextComponentSettings : "binds"
AiTextComponentSettingsControl --> AgentIslandSettings : "updates EntryId"
```

**Diagram sources**
- [AiTextComponent.axaml.cs:16-83](file://Components/AiTextComponent.axaml.cs#L16-L83)
- [AiTextComponent.axaml:1-20](file://Components/AiTextComponent.axaml#L1-L20)
- [AiTextComponentSettingsControl.axaml.cs:7-52](file://Components/AiTextComponentSettingsControl.axaml.cs#L7-L52)
- [AiTextComponentSettings.cs:5-12](file://Models/AiTextComponentSettings.cs#L5-L12)
- [AiTextEntry.cs:5-18](file://Models/AiTextEntry.cs#L5-L18)
- [AgentIslandSettings.cs:107-122](file://Models/AgentIslandSettings.cs#L107-L122)

**Section sources**
- [AiTextComponent.axaml.cs:36-83](file://Components/AiTextComponent.axaml.cs#L36-L83)
- [AiTextComponent.axaml:10-18](file://Components/AiTextComponent.axaml#L10-L18)
- [AiTextComponentSettingsControl.axaml.cs:29-51](file://Components/AiTextComponentSettingsControl.axaml.cs#L29-L51)

### Action System Extension: RunAcpAction
- Purpose:
  - Triggers execution of an ACP agent process based on configured settings.
- Behavior:
  - Validates global and feature flags.
  - Locates the named agent and checks it is enabled.
  - Invokes AcpRunnerService.RunAgentAsync with context identifiers.
  - Updates last-run status and optionally shows a notification.

```mermaid
flowchart TD
Start([Invoke Action]) --> CheckEnabled["Check IsAcpEnabled and IsAgentAutomationEnabled"]
CheckEnabled --> |Not Enabled| ThrowError["Throw InvalidOperationException"]
CheckEnabled --> FindAgent["Find agent by name"]
FindAgent --> Found{"Agent found?"}
Found --> |No| ThrowNotFound["Throw not found error"]
Found --> CheckAgentEnabled["Check agent.IsEnabled"]
CheckAgentEnabled --> |Disabled| ThrowDisabled["Throw disabled error"]
CheckAgentEnabled --> RunAgent["Call AcpRunnerService.RunAgentAsync(...)"]
RunAgent --> UpdateStatus["Update agent.Status"]
UpdateStatus --> Notify{"Show notification?"}
Notify --> |Yes| ShowNotify["Send notification via provider"]
Notify --> |No| End([Done])
ShowNotify --> End
```

**Diagram sources**
- [RunAcpAction.cs:29-82](file://Automation/RunAcpAction.cs#L29-L82)
- [AcpRunnerService.cs:25-77](file://Services/AcpRunnerService.cs#L25-L77)
- [AgentIslandNotificationProvider.cs:27-50](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L27-L50)

**Section sources**
- [RunAcpAction.cs:29-82](file://Automation/RunAcpAction.cs#L29-L82)

### Settings Page Creation with Reactive Properties
- AiTextSettingsPage:
  - Declares metadata for discovery and UI placement.
  - Binds DataContext to global settings.
  - Provides handlers to add/remove entries in the observable collection.
- Reactive model:
  - AgentIslandSettings uses ObservableObject and raises derived property changes for computed values.
  - Collections hook/unhook item events to keep derived properties updated.

```mermaid
sequenceDiagram
participant User as "User"
participant Page as "AiTextSettingsPage.axaml.cs"
participant Settings as "AgentIslandSettings.cs"
participant Entries as "ObservableCollection~AiTextEntry~"
User->>Page : Click Add Entry
Page->>Settings : Add new AiTextEntry to AiTextEntries
Settings->>Entries : CollectionChanged
Settings-->>Page : Derived properties updated
```

**Diagram sources**
- [AiTextSettingsPage.axaml.cs:22-34](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L22-L34)
- [AgentIslandSettings.cs:340-392](file://Models/AgentIslandSettings.cs#L340-L392)

**Section sources**
- [AiTextSettingsPage.axaml.cs:9-34](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L9-L34)
- [AgentIslandSettings.cs:240-273](file://Models/AgentIslandSettings.cs#L240-L273)

### MCP Tool Integration: SetComponentTextTool
- Purpose:
  - Allows agents to update AI text component content by ID.
- Behavior:
  - Validates required parameters.
  - Runs UI updates on the UI thread.
  - Adds or updates entries in the settings collection.
  - Returns structured result and captures telemetry.

```mermaid
sequenceDiagram
participant Client as "MCP Client"
participant Server as "McpServerManager.cs"
participant Tool as "SetComponentTextTool.cs"
participant UI as "UiThreadHelper"
participant Settings as "AgentIslandSettings.cs"
Client->>Server : CallTool("set_component_text", {id, text})
Server->>Tool : Invoke CallTool(context)
Tool->>UI : RunOnUi(update entry)
UI->>Settings : Add or update AiTextEntry
Tool-->>Client : CallToolResult(success, message)
```

**Diagram sources**
- [McpServerManager.cs:41-51](file://Mcp/McpServerManager.cs#L41-L51)
- [SetComponentTextTool.cs:41-72](file://Mcp/Tools/SetComponentTextTool.cs#L41-L72)
- [AgentIslandSettings.cs:107-122](file://Models/AgentIslandSettings.cs#L107-L122)

**Section sources**
- [SetComponentTextTool.cs:41-72](file://Mcp/Tools/SetComponentTextTool.cs#L41-L72)

### Notification Provider: AgentIslandNotificationProvider
- Purpose:
  - Posts mask and overlay notifications via ClassIsland channels.
- Behavior:
  - Initializes static instance.
  - Marshals UI updates onto the UI thread.
  - Builds notification content and shows via channel.

```mermaid
classDiagram
class AgentIslandNotificationProvider {
+Notify(maskText, overlayText, maskDuration, overlayDuration)
+Instance : AgentIslandNotificationProvider?
}
class NotificationProviderBase
AgentIslandNotificationProvider --|> NotificationProviderBase
```

**Diagram sources**
- [AgentIslandNotificationProvider.cs:10-25](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L10-L25)
- [AgentIslandNotificationProvider.cs:27-50](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L27-L50)

**Section sources**
- [AgentIslandNotificationProvider.cs:27-50](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L27-L50)

## Dependency Analysis
Key dependencies and relationships:
- Plugin depends on ClassIsland core abstractions and attributes for discovery.
- Services depend on logging and telemetry.
- MCP server depends on transport and tool implementations.
- UI components depend on models and settings.

```mermaid
graph LR
Plugin["Plugin.cs"] --> DI["DI Extensions (AddSingleton/AddComponent/AddSettingsPage/AddAction)"]
Plugin --> Telemetry["SentryTelemetryService.cs"]
Plugin --> Acp["AcpRunnerService.cs"]
Plugin --> Comp["AiTextComponent.axaml.cs"]
Plugin --> SettingsPage["AiTextSettingsPage.axaml.cs"]
Plugin --> Action["RunAcpAction.cs"]
Plugin --> Mcp["McpServerManager.cs"]
Mcp --> Tool["SetComponentTextTool.cs"]
Tool --> Notif["AgentIslandNotificationProvider.cs"]
Comp --> Models["AgentIslandSettings.cs / AiTextEntry.cs"]
SettingsPage --> Models
Action --> Acp
```

**Diagram sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)
- [McpServerManager.cs:41-51](file://Mcp/McpServerManager.cs#L41-L51)
- [SetComponentTextTool.cs:41-72](file://Mcp/Tools/SetComponentTextTool.cs#L41-L72)
- [AiTextComponent.axaml.cs:36-83](file://Components/AiTextComponent.axaml.cs#L36-L83)
- [AiTextSettingsPage.axaml.cs:22-34](file://Views/SettingsPages/AiTextSettingsPage.axaml.cs#L22-L34)
- [RunAcpAction.cs:29-82](file://Automation/RunAcpAction.cs#L29-L82)

**Section sources**
- [Plugin.cs:29-53](file://Plugin.cs#L29-L53)

## Performance Considerations
- Avoid heavy work on the UI thread; use background tasks and marshal UI updates where needed (as seen in MCP tool updates).
- Reuse singletons registered via DI to minimize allocations and ensure consistent state.
- Use cancellation tokens for long-running operations (e.g., MCP server lifecycle, ACP process communication).
- Keep settings updates minimal and batched; rely on observable collections and derived properties to avoid redundant UI refreshes.

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
Common issues and resolutions:
- MCP server fails to start:
  - Check port availability and transport mode configuration.
  - Review logs and telemetry breadcrumbs for errors during startup.
- ACP agent does not run:
  - Ensure IsAcpEnabled and IsAgentAutomationEnabled are true.
  - Verify agent exists and is enabled in settings.
  - Confirm command line arguments are valid.
- Notifications do not appear:
  - Verify notification provider is registered and channel IDs match.
  - Ensure UI thread marshaling is used when creating notification content.
- Telemetry not captured:
  - Confirm telemetry is active and privacy policy consent or custom DSN is set.
  - Validate DSN and network connectivity.

**Section sources**
- [Plugin.cs:67-79](file://Plugin.cs#L67-L79)
- [Plugin.cs:81-97](file://Plugin.cs#L81-L97)
- [RunAcpAction.cs:35-60](file://Automation/RunAcpAction.cs#L35-L60)
- [AgentIslandNotificationProvider.cs:27-50](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L27-L50)
- [SentryTelemetryService.cs:30-40](file://Services/SentryTelemetryService.cs#L30-L40)

## Conclusion
AgentIsland demonstrates a complete ClassIsland plugin architecture:
- Lifecycle management via Initialize/Start/Stop/Dispose
- Centralized DI registration for services, components, actions, and settings pages
- Reactive UI components with Avalonia XAML and property binding
- Automation actions with robust validation and notifications
- MCP server integration with tools that interact with UI state safely
- Optional telemetry with privacy controls

Use these patterns to extend AgentIsland or build your own ClassIsland plugins.

[No sources needed since this section summarizes without analyzing specific files]