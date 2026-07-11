# MCP Tools API

<cite>
**Referenced Files in This Document**
- [McpServerManager.cs](file://Mcp/McpServerManager.cs)
- [LessonTools.cs](file://Mcp/Tools/LessonTools.cs)
- [ScheduleTools.cs](file://Mcp/Tools/ScheduleTools.cs)
- [GetScheduleByDateTool.cs](file://Mcp/Tools/GetScheduleByDateTool.cs)
- [SwapClassesTool.cs](file://Mcp/Tools/SwapClassesTool.cs)
- [SendNotificationTool.cs](file://Mcp/Tools/SendNotificationTool.cs)
- [SetComponentTextTool.cs](file://Mcp/Tools/SetComponentTextTool.cs)
- [ListComponentTextTool.cs](file://Mcp/Tools/ListComponentTextTool.cs)
- [AddComponentTextTool.cs](file://Mcp/Tools/AddComponentTextTool.cs)
- [DeleteComponentTextTool.cs](file://Mcp/Tools/DeleteComponentTextTool.cs)
- [ClearComponentTextTool.cs](file://Mcp/Tools/ClearComponentTextTool.cs)
- [EchoCaveTool.cs](file://Mcp/Tools/EchoCaveTool.cs)
- [AgentIslandNotificationProvider.cs](file://Mcp/Tools/AgentIslandNotificationProvider.cs)
- [ToolResults.cs](file://Models/ToolResults.cs)
- [AiTextEntry.cs](file://Models/AiTextEntry.cs)
- [AgentIslandSettings.cs](file://Models/AgentIslandSettings.cs)
- [McpTransportMode.cs](file://Models/McpTransportMode.cs)
</cite>

## Update Summary
**Changes Made**
- Added comprehensive documentation for Echo Cave feature with `get_echo_cave` tool
- Expanded AI text management section with four new tools: `add_component_text`, `delete_component_text`, `clear_component_text`, and `list_component_text`
- Updated ToolResults models section to include new result types: `EchoCaveResult`, `ComponentTextListResult`, and `ComponentTextEntry`
- Enhanced architecture diagrams to reflect new tool registrations
- Updated endpoint summary with all new tool names

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)
10. [Appendices](#appendices)

## Introduction
This document provides comprehensive API documentation for AgentIsland's Model Context Protocol (MCP) tools. It covers lesson management, schedule management, notifications, component control, AI text management, and Echo Cave endpoints exposed by the local MCP server. For each tool, it specifies:
- Tool name and purpose
- Transport mode and URL patterns
- Request parameters with types and validation rules
- Response schemas with field descriptions
- Error handling behavior
- JSON request/response examples
- Authentication requirements
- Rate limiting information
- Common usage patterns
- ToolResults models and their relationships to responses

The MCP server runs locally on a configurable port and supports two transport modes: Streamable HTTP and Server-Sent Events (SSE).

## Project Structure
AgentIsland exposes MCP tools via a local HTTP server. The server is configured through settings and registers multiple tool classes that implement or decorate MCP tool functionality.

```mermaid
graph TB
subgraph "Local MCP Server"
Manager["McpServerManager<br/>StartAsync(port, transportMode)"]
Builder["McpServerBuilder<br/>WithTools(...)"]
Transport["LocalHostHttpServerTransportOptions<br/>Port + EndPoint"]
end
subgraph "Tools"
LT["LessonTools<br/>get_current_class, get_next_class, get_time_status"]
ST["ScheduleTools<br/>get_today_schedule, list_subjects"]
GSDT["GetScheduleByDateTool<br/>get_schedule_by_date"]
SCT["SwapClassesTool<br/>swap_classes"]
SNT["SendNotificationTool<br/>send_notification"]
SCText["SetComponentTextTool<br/>set_component_text"]
LCT["ListComponentTextTool<br/>list_component_text"]
ACT["AddComponentTextTool<br/>add_component_text"]
DCT["DeleteComponentTextTool<br/>delete_component_text"]
CCT["ClearComponentTextTool<br/>clear_component_text"]
ECT["EchoCaveTool<br/>get_echo_cave"]
end
subgraph "Models"
TR["ToolResults<br/>CurrentClassResult, NextClassResult, TimeStatusResult,<br/>ScheduleResult, ScheduleClassEntry, SwapResult,<br/>SubjectListResult, SubjectEntry, NotificationResult, SetTextResult,<br/>EchoCaveResult, ComponentTextListResult, ComponentTextEntry"]
AIE["AiTextEntry"]
AIS["AgentIslandSettings<br/>ConnectionAddress, Port, TransportMode"]
MTM["McpTransportMode<br/>StreamableHttp, Sse"]
ECF["echo-cave.txt<br/>Embedded resource"]
end
Manager --> Builder
Builder --> LT
Builder --> ST
Builder --> GSDT
Builder --> SCT
Builder --> SNT
Builder --> SCText
Builder --> LCT
Builder --> ACT
Builder --> DCT
Builder --> CCT
Builder --> ECT
Builder --> Transport
LT --> TR
ST --> TR
GSDT --> TR
SCT --> TR
SNT --> TR
SCText --> TR
LCT --> TR
ACT --> TR
DCT --> TR
CCT --> TR
ECT --> TR
ECT --> ECF
SCText --> AIE
Manager --> AIS
AIS --> MTM
```

**Diagram sources**
- [McpServerManager.cs:41-55](file://Mcp/McpServerManager.cs#L41-L55)
- [LessonTools.cs:14-146](file://Mcp/Tools/LessonTools.cs#L14-L146)
- [ScheduleTools.cs:15-204](file://Mcp/Tools/ScheduleTools.cs#L15-L204)
- [GetScheduleByDateTool.cs:16-92](file://Mcp/Tools/GetScheduleByDateTool.cs#L16-L92)
- [SwapClassesTool.cs:16-103](file://Mcp/Tools/SwapClassesTool.cs#L16-L103)
- [SendNotificationTool.cs:16-137](file://Mcp/Tools/SendNotificationTool.cs#L16-L137)
- [SetComponentTextTool.cs:17-92](file://Mcp/Tools/SetComponentTextTool.cs#L17-L92)
- [ListComponentTextTool.cs:18-66](file://Mcp/Tools/ListComponentTextTool.cs#L18-L66)
- [AddComponentTextTool.cs:18-110](file://Mcp/Tools/AddComponentTextTool.cs#L18-L110)
- [DeleteComponentTextTool.cs:18-101](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L101)
- [ClearComponentTextTool.cs:18-121](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L121)
- [EchoCaveTool.cs:17-93](file://Mcp/Tools/EchoCaveTool.cs#L17-L93)
- [ToolResults.cs:1-71](file://Models/ToolResults.cs#L1-L71)
- [AiTextEntry.cs:1-31](file://Models/AiTextEntry.cs#L1-L31)
- [AgentIslandSettings.cs:34-211](file://Models/AgentIslandSettings.cs#L34-L211)
- [McpTransportMode.cs:1-18](file://Models/McpTransportMode.cs#L1-L18)

**Section sources**
- [McpServerManager.cs:25-82](file://Mcp/McpServerManager.cs#L25-L82)
- [AgentIslandSettings.cs:34-211](file://Models/AgentIslandSettings.cs#L34-L211)
- [McpTransportMode.cs:1-18](file://Models/McpTransportMode.cs#L1-L18)

## Core Components
- LessonTools: Provides read-only tools for current class, next class, and time status.
- ScheduleTools: Provides read-only tools for today's schedule and subject listing; also contains logic used by swap and date-based schedule retrieval.
- GetScheduleByDateTool: Exposes a tool to retrieve a schedule for a specific date.
- SwapClassesTool: Exposes a tool to swap two classes on a given date.
- SendNotificationTool: Exposes a tool to display a notification overlay in the UI.
- SetComponentTextTool: Exposes a tool to update text displayed by an AI text component identified by ID.
- ListComponentTextTool: Lists all AI text entries with their IDs, descriptions, text content, and display names.
- AddComponentTextTool: Adds new AI text entries with unique IDs and optional content.
- DeleteComponentTextTool: Deletes AI text entries by ID.
- ClearComponentTextTool: Clears text content of entries while preserving metadata (supports clearing all entries).
- EchoCaveTool: Returns random inspirational or informational content from an embedded text file.
- ToolResults: Defines structured response models for all tools.

Key behaviors:
- All tools are registered with the MCP server builder and serialized using a shared JSON context.
- Read-only tools are annotated as such; write operations are marked non-idempotent where applicable.
- Some tools execute UI-bound operations on the UI thread.
- New AI text management tools provide comprehensive CRUD operations for managing dynamic text components.

**Section sources**
- [LessonTools.cs:14-146](file://Mcp/Tools/LessonTools.cs#L14-L146)
- [ScheduleTools.cs:15-204](file://Mcp/Tools/ScheduleTools.cs#L15-L204)
- [GetScheduleByDateTool.cs:16-92](file://Mcp/Tools/GetScheduleByDateTool.cs#L16-L92)
- [SwapClassesTool.cs:16-103](file://Mcp/Tools/SwapClassesTool.cs#L16-L103)
- [SendNotificationTool.cs:16-137](file://Mcp/Tools/SendNotificationTool.cs#L16-L137)
- [SetComponentTextTool.cs:17-92](file://Mcp/Tools/SetComponentTextTool.cs#L17-L92)
- [ListComponentTextTool.cs:18-66](file://Mcp/Tools/ListComponentTextTool.cs#L18-L66)
- [AddComponentTextTool.cs:18-110](file://Mcp/Tools/AddComponentTextTool.cs#L18-L110)
- [DeleteComponentTextTool.cs:18-101](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L101)
- [ClearComponentTextTool.cs:18-121](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L121)
- [EchoCaveTool.cs:17-93](file://Mcp/Tools/EchoCaveTool.cs#L17-L93)
- [ToolResults.cs:1-71](file://Models/ToolResults.cs#L1-L71)

## Architecture Overview
The MCP server listens locally and routes tool calls to the appropriate handler. Depending on the configured transport mode, the endpoint path differs.

```mermaid
sequenceDiagram
participant Client as "Client"
participant Server as "McpServerManager"
participant Builder as "McpServerBuilder"
participant Tool as "IMcpServerTool / McpServerTool"
participant Service as "ClassIsland Services"
participant UI as "UI Thread"
participant FileSys as "File System"
Client->>Server : "HTTP call to /mcp or /sse"
Server->>Builder : "Build server with tools"
Builder-->>Server : "McpServer instance"
Server->>Tool : "CallTool(arguments)"
alt Read-only tool
Tool->>Service : "Read data (e.g., ILessonsService)"
Service-->>Tool : "Data"
Tool-->>Client : "Structured result"
else Write tool
Tool->>UI : "RunOnUi(...)"
UI->>Service : "Update profile/state"
Service-->>UI : "Success"
UI-->>Tool : "Done"
Tool-->>Client : "Structured result"
else Echo Cave tool
Tool->>FileSys : "Load embedded resource"
FileSys-->>Tool : "Random line content"
Tool-->>Client : "EchoCaveResult"
end
```

**Diagram sources**
- [McpServerManager.cs:25-82](file://Mcp/McpServerManager.cs#L25-L82)
- [LessonTools.cs:14-146](file://Mcp/Tools/LessonTools.cs#L14-L146)
- [ScheduleTools.cs:15-204](file://Mcp/Tools/ScheduleTools.cs#L15-L204)
- [SendNotificationTool.cs:16-137](file://Mcp/Tools/SendNotificationTool.cs#L16-L137)
- [SetComponentTextTool.cs:17-92](file://Mcp/Tools/SetComponentTextTool.cs#L17-L92)
- [EchoCaveTool.cs:58-83](file://Mcp/Tools/EchoCaveTool.cs#L58-L83)

## Detailed Component Analysis

### General Connection Information
- Localhost only.
- Default port: 5943.
- Transport modes:
  - Streamable HTTP: endpoint path "mcp"
  - SSE: endpoint path "sse"
- Base URLs:
  - Streamable HTTP: http://localhost:{port}/mcp
  - SSE: http://localhost:{port}/sse

These are derived from configuration properties and transport selection.

**Section sources**
- [AgentIslandSettings.cs:34-211](file://Models/AgentIslandSettings.cs#L34-L211)
- [McpTransportMode.cs:1-18](file://Models/McpTransportMode.cs#L1-L18)
- [McpServerManager.cs:53-67](file://Mcp/McpServerManager.cs#L53-L67)

### Authentication
- No authentication is implemented for the local MCP server. Calls originate from localhost.

### Rate Limiting
- No rate limiting is implemented in the server or tools.

---

### Lesson Management Tools

#### get_current_class
- Purpose: Returns information about the currently active class, if any.
- Transport: Streamable HTTP or SSE (same tool name across transports).
- Method: As defined by MCP protocol over HTTP (tool invocation).
- URL pattern: http://localhost:{port}/mcp or http://localhost:{port}/sse
- Parameters: None.
- Response schema: CurrentClassResult
  - SubjectName: string
  - TeacherName: string
  - StartTime: string? formatted as hh:mm:ss
  - EndTime: string? formatted as hh:mm:ss
  - RemainingSeconds: int seconds remaining in class (non-negative)
  - IsInClass: bool true when a class is active
- Example response:
  {
    "SubjectName": "Mathematics",
    "TeacherName": "Alice",
    "StartTime": "09:00:00",
    "EndTime": "09:45:00",
    "RemainingSeconds": 1200,
    "IsInClass": true
  }
- Errors: None expected; returns empty fields when no class is active.

**Section sources**
- [LessonTools.cs:14-45](file://Mcp/Tools/LessonTools.cs#L14-L45)
- [ToolResults.cs:3-9](file://Models/ToolResults.cs#L3-L9)

#### get_next_class
- Purpose: Returns information about the next scheduled class.
- Parameters: None.
- Response schema: NextClassResult
  - SubjectName: string
  - TeacherName: string
  - StartTime: string? hh:mm:ss
  - EndTime: string? hh:mm:ss
  - SecondsUntilStart: int seconds until start (non-negative)
  - HasNextClass: bool true if a next class exists
- Example response:
  {
    "SubjectName": "Physics",
    "TeacherName": "Bob",
    "StartTime": "10:00:00",
    "EndTime": "10:45:00",
    "SecondsUntilStart": 3600,
    "HasNextClass": true
  }

**Section sources**
- [LessonTools.cs:47-83](file://Mcp/Tools/LessonTools.cs#L47-L83)
- [ToolResults.cs:11-17](file://Models/ToolResults.cs#L11-L17)

#### get_time_status
- Purpose: Returns the current state and remaining time.
- Parameters: None.
- Response schema: TimeStatusResult
  - CurrentState: string normalized state (e.g., InClass, Breaking, AfterSchool)
  - RemainingSeconds: int seconds remaining in current period (non-negative)
  - CurrentTime: string ISO 8601 local time
- Example response:
  {
    "CurrentState": "InClass",
    "RemainingSeconds": 1200,
    "CurrentTime": "2026-06-19T09:20:00.0000000+08:00"
  }

**Section sources**
- [LessonTools.cs:85-113](file://Mcp/Tools/LessonTools.cs#L85-L113)
- [ToolResults.cs:19-22](file://Models/ToolResults.cs#L19-L22)

---

### Schedule Management Tools

#### get_today_schedule
- Purpose: Returns today's schedule based on the current class plan.
- Parameters: None.
- Response schema: ScheduleResult
  - ClassPlanName: string
  - Date: string yyyy-MM-dd
  - Classes: list of ScheduleClassEntry
- ScheduleClassEntry fields:
  - Index: int zero-based index
  - SubjectName: string
  - TeacherName: string
  - StartTime: string? hh:mm:ss
  - EndTime: string? hh:mm:ss
  - IsChangedClass: bool indicates overridden class
  - IsEnabled: bool indicates whether the class is enabled
- Example response:
  {
    "ClassPlanName": "Monday Plan",
    "Date": "2026-06-19",
    "Classes": [
      {
        "Index": 0,
        "SubjectName": "Mathematics",
        "TeacherName": "Alice",
        "StartTime": "09:00:00",
        "EndTime": "09:45:00",
        "IsChangedClass": false,
        "IsEnabled": true
      }
    ]
  }

**Section sources**
- [ScheduleTools.cs:15-39](file://Mcp/Tools/ScheduleTools.cs#L15-L39)
- [ToolResults.cs:24-36](file://Models/ToolResults.cs#L24-L36)

#### get_schedule_by_date
- Purpose: Returns the schedule for a specified date.
- Parameters:
  - date: string required, format yyyy-MM-dd (e.g., 2026-06-19)
- Validation:
  - If missing or invalid, throws argument error which is caught and returned as an error payload.
- Response schema: ScheduleResult (same as above)
- Example request:
  {
    "date": "2026-06-19"
  }
- Example success response:
  {
    "ClassPlanName": "Friday Plan",
    "Date": "2026-06-19",
    "Classes": []
  }
- Example error response (invalid date):
  {
    "ClassPlanName": "Error: Invalid date format. Use yyyy-MM-dd.",
    "Date": "",
    "Classes": []
  }

**Section sources**
- [GetScheduleByDateTool.cs:16-92](file://Mcp/Tools/GetScheduleByDateTool.cs#L16-L92)
- [ScheduleTools.cs:41-56](file://Mcp/Tools/ScheduleTools.cs#L41-L56)
- [ToolResults.cs:24-36](file://Models/ToolResults.cs#L24-L36)

#### list_subjects
- Purpose: Lists all subjects available in the profile.
- Parameters: None.
- Response schema: SubjectListResult
  - Subjects: list of SubjectEntry
- SubjectEntry fields:
  - Id: string GUID
  - Name: string
  - TeacherName: string
  - Initial: string
- Example response:
  {
    "Subjects": [
      {
        "Id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "Name": "Mathematics",
        "TeacherName": "Alice",
        "Initial": "M"
      }
    ]
  }

**Section sources**
- [ScheduleTools.cs:105-131](file://Mcp/Tools/ScheduleTools.cs#L105-L131)
- [ToolResults.cs:42-49](file://Models/ToolResults.cs#L42-L49)

#### swap_classes
- Purpose: Swaps two classes on a given date by creating or reusing a temporary overlay class plan.
- Parameters:
  - classIndex1: integer required, zero-based index
  - classIndex2: integer required, zero-based index
  - date: string optional, yyyy-MM-dd; empty string means today
- Validation:
  - Indices must be within range of the day's classes.
  - If no class plan exists for the date, returns failure message.
- Response schema: SwapResult
  - Success: boolean
  - Message: string human-readable status
- Example request:
  {
    "classIndex1": 0,
    "classIndex2": 1,
    "date": "2026-06-19"
  }
- Example success response:
  {
    "Success": true,
    "Message": "Classes swapped successfully."
  }
- Example error response (out-of-range indices):
  {
    "Success": false,
    "Message": "Class index is out of range."
  }

Notes:
- The operation modifies the profile by creating or updating a temporary overlay class plan and persists changes.

**Section sources**
- [SwapClassesTool.cs:16-103](file://Mcp/Tools/SwapClassesTool.cs#L16-L103)
- [ScheduleTools.cs:58-103](file://Mcp/Tools/ScheduleTools.cs#L58-L103)
- [ToolResults.cs:38-40](file://Models/ToolResults.cs#L38-L40)

---

### Notification Tools

#### send_notification
- Purpose: Displays a notification overlay in the UI with optional body text and durations.
- Parameters:
  - message: string required; main title/mask text
  - body: string optional; detailed content
  - maskDuration: number optional; seconds for mask display (default 3.0)
  - overlayDuration: number optional; seconds for overlay display (default 5.0)
- Validation:
  - message is required and must be a string.
  - Optional parameters default if omitted.
- Response schema: NotificationResult
  - Success: boolean
  - Message: string human-readable status
- Example request:
  {
    "message": "Upcoming class reminder",
    "body": "Mathematics starts in 5 minutes",
    "maskDuration": 4.0,
    "overlayDuration": 6.0
  }
- Example success response:
  {
    "Success": true,
    "Message": "Notification sent successfully."
  }
- Example error response (provider not initialized):
  {
    "Success": false,
    "Message": "Notification provider is not initialized yet."
  }

Behavior:
- Uses the notification provider to show a mask and optional overlay content on the UI thread.

**Section sources**
- [SendNotificationTool.cs:16-137](file://Mcp/Tools/SendNotificationTool.cs#L16-L137)
- [AgentIslandNotificationProvider.cs:12-52](file://Mcp/Tools/AgentIslandNotificationProvider.cs#L12-L52)
- [ToolResults.cs:51-53](file://Models/ToolResults.cs#L51-L53)

---

### Component Control Tools

#### set_component_text
- Purpose: Updates the text shown by an AI text component identified by its ID.
- Parameters:
  - id: string required; component identifier
  - text: string required; new text content
- Behavior:
  - If the entry exists, updates its text.
  - If not, creates a new entry with the provided id and text.
- Response schema: SetTextResult
  - Success: boolean
  - Message: string human-readable status
- Example request:
  {
    "id": "status-banner",
    "text": "Exam week schedule active"
  }
- Example success response:
  {
    "Success": true,
    "Message": "Text updated."
  }

Persistence:
- Entries are stored in plugin settings and can be managed via the UI settings page.

**Section sources**
- [SetComponentTextTool.cs:17-92](file://Mcp/Tools/SetComponentTextTool.cs#L17-L92)
- [AiTextEntry.cs:1-31](file://Models/AiTextEntry.cs#L1-L31)
- [ToolResults.cs:55-57](file://Models/ToolResults.cs#L55-L57)

---

### AI Text Management Tools

#### list_component_text
- Purpose: Lists all AI text entries with their complete metadata including IDs, descriptions, text content, and display names.
- Parameters: None.
- Response schema: ComponentTextListResult
  - Entries: list of ComponentTextEntry
- ComponentTextEntry fields:
  - Id: string unique identifier
  - Description: string description for UI settings page
  - Text: string current text content
  - DisplayName: string display name for UI presentation
- Example response:
  {
    "Entries": [
      {
        "Id": "status-banner",
        "Description": "Main status banner",
        "Text": "Exam week schedule active",
        "DisplayName": "Status Banner"
      },
      {
        "Id": "announcement",
        "Description": "System announcements",
        "Text": "Maintenance scheduled for tonight",
        "DisplayName": "Announcements"
      }
    ]
  }
- Errors: Returns empty entries list on exceptions.

**Section sources**
- [ListComponentTextTool.cs:18-66](file://Mcp/Tools/ListComponentTextTool.cs#L18-L66)
- [ToolResults.cs:62-69](file://Models/ToolResults.cs#L62-L69)

#### add_component_text
- Purpose: Adds a new AI text entry with a unique ID and optional content.
- Parameters:
  - id: string required; unique component identifier
  - text: string optional; initial text content (defaults to empty)
  - description: string optional; description for UI settings page (defaults to empty)
- Validation:
  - id is required and must be unique (no duplicate IDs allowed).
  - text and description are optional with empty string defaults.
- Response schema: SetTextResult
  - Success: boolean
  - Message: string human-readable status
- Example request:
  {
    "id": "weather-widget",
    "text": "Sunny, 72°F",
    "description": "Weather information display"
  }
- Example success response:
  {
    "Success": true,
    "Message": "Entry 'weather-widget' added."
  }
- Example error response (duplicate ID):
  {
    "Success": false,
    "Message": "Entry with id 'weather-widget' already exists."
  }

**Section sources**
- [AddComponentTextTool.cs:18-110](file://Mcp/Tools/AddComponentTextTool.cs#L18-L110)
- [ToolResults.cs:55-57](file://Models/ToolResults.cs#L55-L57)

#### delete_component_text
- Purpose: Deletes an AI text entry by its ID.
- Parameters:
  - id: string required; component identifier to delete
- Validation:
  - id is required and must exist in the system.
- Response schema: SetTextResult
  - Success: boolean
  - Message: string human-readable status
- Example request:
  {
    "id": "old-notification"
  }
- Example success response:
  {
    "Success": true,
    "Message": "Entry 'old-notification' deleted."
  }
- Example error response (not found):
  {
    "Success": false,
    "Message": "Entry with id 'old-notification' not found."
  }

**Section sources**
- [DeleteComponentTextTool.cs:18-101](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-L101)
- [ToolResults.cs:55-57](file://Models/ToolResults.cs#L55-L57)

#### clear_component_text
- Purpose: Clears the text content of AI text entries while preserving their metadata (ID, description, display name).
- Parameters:
  - id: string required; component identifier or "all" (case-insensitive) to clear all entries
- Validation:
  - id is required and must be either a valid entry ID or "all".
- Response schema: SetTextResult
  - Success: boolean
  - Message: string human-readable status
- Example requests:
  {
    "id": "status-banner"
  }
  {
    "id": "all"
  }
- Example success response (single entry):
  {
    "Success": true,
    "Message": "Text of entry 'status-banner' cleared."
  }
- Example success response (all entries):
  {
    "Success": true,
    "Message": "Cleared text of 5 entries."
  }
- Example error response (not found):
  {
    "Success": false,
    "Message": "Entry with id 'nonexistent' not found."
  }

Notes:
- Unlike delete_component_text, this tool preserves the entry structure but clears only the text content.
- Supports bulk clearing with "all" parameter for efficient cleanup operations.

**Section sources**
- [ClearComponentTextTool.cs:18-121](file://Mcp/Tools/ClearComponentTextTool.cs#L18-L121)
- [ToolResults.cs:55-57](file://Models/ToolResults.cs#L55-L57)

---

### Echo Cave Feature

#### get_echo_cave
- Purpose: Returns random inspirational or informational content from an embedded text file called "Echo Cave".
- Parameters: None.
- Response schema: EchoCaveResult
  - Content: string random line from the echo cave resource
- Example response:
  {
    "Content": "热知识：回声洞的内容是可以改的。——开发者"
  }
- Example response:
  {
    "Content": "Hello, world!——匿名"
  }
- Errors:
  - Returns error message if embedded resource is not found.
  - Returns empty content message if resource file is empty.
  - Returns exception message if any error occurs during processing.

Behavior:
- Reads from embedded resource "AgentIsland.echo-cave.txt"
- Randomly selects one line from the resource file
- Provides inspirational quotes, facts, or messages
- Useful for generating dynamic content or motivational messages

**Section sources**
- [EchoCaveTool.cs:17-93](file://Mcp/Tools/EchoCaveTool.cs#L17-L93)
- [ToolResults.cs:59-60](file://Models/ToolResults.cs#L59-L60)

---

### ToolResults Models and Relationships
All tools return structured results defined in ToolResults. These records map directly to JSON payloads returned by the MCP server.

```mermaid
classDiagram
class CurrentClassResult {
+string SubjectName
+string TeacherName
+string? StartTime
+string? EndTime
+int RemainingSeconds
+bool IsInClass
}
class NextClassResult {
+string SubjectName
+string TeacherName
+string? StartTime
+string? EndTime
+int SecondsUntilStart
+bool HasNextClass
}
class TimeStatusResult {
+string CurrentState
+int RemainingSeconds
+string CurrentTime
}
class ScheduleClassEntry {
+int Index
+string SubjectName
+string TeacherName
+string? StartTime
+string? EndTime
+bool IsChangedClass
+bool IsEnabled
}
class ScheduleResult {
+string ClassPlanName
+string Date
+ScheduleClassEntry[] Classes
}
class SwapResult {
+bool Success
+string Message
}
class SubjectEntry {
+string Id
+string Name
+string TeacherName
+string Initial
}
class SubjectListResult {
+SubjectEntry[] Subjects
}
class NotificationResult {
+bool Success
+string Message
}
class SetTextResult {
+bool Success
+string Message
}
class EchoCaveResult {
+string Content
}
class ComponentTextListResult {
+ComponentTextEntry[] Entries
}
class ComponentTextEntry {
+string Id
+string Description
+string Text
+string DisplayName
}
ScheduleResult --> ScheduleClassEntry : "contains"
SubjectListResult --> SubjectEntry : "contains"
ComponentTextListResult --> ComponentTextEntry : "contains"
```

**Diagram sources**
- [ToolResults.cs:1-71](file://Models/ToolResults.cs#L1-L71)

**Section sources**
- [ToolResults.cs:1-71](file://Models/ToolResults.cs#L1-L71)

## Dependency Analysis
The following diagram shows how tools depend on services and models during execution.

```mermaid
graph LR
LT["LessonTools"] --> LS["ILessonsService"]
LT --> ET["IExactTimeService"]
ST["ScheduleTools"] --> LS
ST --> PS["IProfileService"]
GSDT["GetScheduleByDateTool"] --> ST
SCT["SwapClassesTool"] --> ST
SNT["SendNotificationTool"] --> NIP["AgentIslandNotificationProvider"]
SCText["SetComponentTextTool"] --> SET["Plugin.Settings.AiTextEntries"]
SCText --> AIE["AiTextEntry"]
LCT["ListComponentTextTool"] --> SET
LCT --> CTE["ComponentTextEntry"]
ACT["AddComponentTextTool"] --> SET
ACT --> AIE
DCT["DeleteComponentTextTool"] --> SET
CCT["ClearComponentTextTool"] --> SET
ECT["EchoCaveTool"] --> ECR["echo-cave.txt Resource"]
ECT --> ECR["EchoCaveResult"]
```

**Diagram sources**
- [LessonTools.cs:22-45](file://Mcp/Tools/LessonTools.cs#L22-L45)
- [LessonTools.cs:55-83](file://Mcp/Tools/LessonTools.cs#L55-L83)
- [LessonTools.cs:93-113](file://Mcp/Tools/LessonTools.cs#L93-L113)
- [ScheduleTools.cs:23-39](file://Mcp/Tools/ScheduleTools.cs#L23-L39)
- [ScheduleTools.cs:41-56](file://Mcp/Tools/ScheduleTools.cs#L41-L56)
- [ScheduleTools.cs:58-103](file://Mcp/Tools/ScheduleTools.cs#L58-L103)
- [GetScheduleByDateTool.cs:53-78](file://Mcp/Tools/GetScheduleByDateTool.cs#L53-L78)
- [SwapClassesTool.cs:63-80](file://Mcp/Tools/SwapClassesTool.cs#L63-80)
- [SendNotificationTool.cs:85-96](file://Mcp/Tools/SendNotificationTool.cs#L85-96)
- [SetComponentTextTool.cs:56-65](file://Mcp/Tools/SetComponentTextTool.cs#L56-65)
- [ListComponentTextTool.cs:47-50](file://Mcp/Tools/ListComponentTextTool.cs#L47-50)
- [AddComponentTextTool.cs:62-78](file://Mcp/Tools/AddComponentTextTool.cs#L62-78)
- [DeleteComponentTextTool.cs:57-69](file://Mcp/Tools/DeleteComponentTextTool.cs#L57-69)
- [ClearComponentTextTool.cs:59-89](file://Mcp/Tools/ClearComponentTextTool.cs#L59-89)
- [EchoCaveTool.cs:58-83](file://Mcp/Tools/EchoCaveTool.cs#L58-83)
- [AiTextEntry.cs:1-31](file://Models/AiTextEntry.cs#L1-31)

**Section sources**
- [LessonTools.cs:14-146](file://Mcp/Tools/LessonTools.cs#L14-146)
- [ScheduleTools.cs:15-204](file://Mcp/Tools/ScheduleTools.cs#L15-204)
- [GetScheduleByDateTool.cs:16-92](file://Mcp/Tools/GetScheduleByDateTool.cs#L16-92)
- [SwapClassesTool.cs:16-103](file://Mcp/Tools/SwapClassesTool.cs#L16-103)
- [SendNotificationTool.cs:16-137](file://Mcp/Tools/SendNotificationTool.cs#L16-137)
- [SetComponentTextTool.cs:17-92](file://Mcp/Tools/SetComponentTextTool.cs#L17-92)
- [ListComponentTextTool.cs:18-66](file://Mcp/Tools/ListComponentTextTool.cs#L18-66)
- [AddComponentTextTool.cs:18-110](file://Mcp/Tools/AddComponentTextTool.cs#L18-110)
- [DeleteComponentTextTool.cs:18-101](file://Mcp/Tools/DeleteComponentTextTool.cs#L18-101)
- [ClearComponentTextTool.cs:18-121](file://Mcp/Tools/ClearComponentTextTool.cs#L18-121)
- [EchoCaveTool.cs:17-93](file://Mcp/Tools/EchoCaveTool.cs#L17-93)
- [AiTextEntry.cs:1-31](file://Models/AiTextEntry.cs#L1-31)

## Performance Considerations
- UI-bound operations: Several tools run on the UI thread to access UI-related services safely. Avoid excessive concurrent calls to prevent UI thread contention.
- Profile persistence: Swap operations save the profile; batch operations should be minimized to reduce I/O overhead.
- Time calculations: Time computations are lightweight but rely on exact time services; ensure system clock accuracy for correct countdowns.
- Echo Cave performance: File reading operations are lightweight but occur on every call; consider caching if high-frequency access is needed.
- AI text management: Bulk operations like clearing all entries may involve multiple UI thread operations; use judiciously.

## Troubleshooting Guide
Common issues and resolutions:
- Invalid date format for get_schedule_by_date: Ensure the date parameter uses yyyy-MM-dd. Errors are returned as a ScheduleResult with an error message in the ClassPlanName field.
- Missing required parameters:
  - send_notification requires message.
  - set_component_text requires both id and text.
  - swap_classes requires classIndex1 and classIndex2.
  - add_component_text requires id.
  - delete_component_text requires id.
  - clear_component_text requires id.
- Out-of-range indices for swap_classes: Verify indices against the length of the day's classes.
- Notification provider not initialized: send_notification may fail if the provider is not ready; retry after initialization.
- No class plan found: swap_classes returns a failure message if there is no class plan for the specified date.
- Duplicate IDs for add_component_text: Ensure unique IDs when adding new entries.
- Non-existent entries: delete_component_text and clear_component_text will fail if the specified ID doesn't exist.
- Echo Cave resource issues: Check if the embedded resource file exists and contains content.

**Section sources**
- [GetScheduleByDateTool.cs:71-78](file://Mcp/Tools/GetScheduleByDateTool.cs#L71-78)
- [SwapClassesTool.cs:71-80](file://Mcp/Tools/SwapClassesTool.cs#L71-80)
- [ScheduleTools.cs:70-103](file://Mcp/Tools/ScheduleTools.cs#L70-103)
- [SendNotificationTool.cs:85-96](file://Mcp/Tools/SendNotificationTool.cs#L85-96)
- [AddComponentTextTool.cs:64-68](file://Mcp/Tools/AddComponentTextTool.cs#L64-68)
- [DeleteComponentTextTool.cs:59-64](file://Mcp/Tools/DeleteComponentTextTool.cs#L59-64)
- [ClearComponentTextTool.cs:79-84](file://Mcp/Tools/ClearComponentTextTool.cs#L79-84)
- [EchoCaveTool.cs:61-66](file://Mcp/Tools/EchoCaveTool.cs#L61-66)

## Conclusion
AgentIsland's MCP tools provide a comprehensive interface for querying and modifying schedule data, retrieving real-time class status, sending notifications, controlling UI components, managing AI text entries, and accessing inspirational content through the Echo Cave feature. The server operates locally with no authentication and no rate limiting. Clients should respect parameter validation rules and handle structured error responses appropriately. The expanded AI text management capabilities provide full CRUD operations for dynamic content management, while the Echo Cave adds a fun element of randomness to applications.

## Appendices

### Endpoint Summary
- Transport modes:
  - Streamable HTTP: http://localhost:{port}/mcp
  - SSE: http://localhost:{port}/sse
- Tool names (transport-agnostic):
  - get_current_class
  - get_next_class
  - get_time_status
  - get_today_schedule
  - get_schedule_by_date
  - list_subjects
  - swap_classes
  - send_notification
  - set_component_text
  - list_component_text
  - add_component_text
  - delete_component_text
  - clear_component_text
  - get_echo_cave

**Section sources**
- [McpServerManager.cs:41-55](file://Mcp/McpServerManager.cs#L41-55)
- [AgentIslandSettings.cs:204-211](file://Models/AgentIslandSettings.cs#L204-211)
- [McpTransportMode.cs:1-18](file://Models/McpTransportMode.cs#L1-18)