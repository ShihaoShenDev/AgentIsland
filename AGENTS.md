# AgentIsland - Development Guide

## Project Type

ClassIsland plugin (Avalonia desktop app, .NET 8, Windows-only). Exposes MCP Server for external AI agents to interact with ClassIsland's timetable system.

## Build Commands

```powershell
# Debug build + launch ClassIsland (kills existing instance first)
.\build-debug.ps1

# Release build + launch
.\build-release.ps1

# Create .cipx plugin package
.\create-cipx.ps1
```

**Prerequisites**: ClassIsland development environment must be set up first. See https://docs.classisland.tech/dev/get-started/development-plugins.html

**Important**: Requires `$env:ClassIsland_DebugBinaryDirectory` environment variable pointing to ClassIsland's debug binary location.

## Architecture

- **Entry point**: `Plugin.cs` - lifecycle management (Initialize/Start/Stop/Dispose)
- **MCP Server**: `Mcp/McpServerManager.cs` - manages server lifecycle
- **MCP Tools**: `Mcp/Tools/` - individual tool implementations (get_current_class, swap_classes, etc.)
- **Settings UI**: `Views/SettingsPages/` - Avalonia XAML pages (McpSettingsPage, AcpSettingsPage, AiTextSettingsPage)
- **Models**: `Models/AgentIslandSettings.cs` - central settings (Enabled, Port, TransportMode, etc.)
- **Services**: `Services/AcpRunnerService.cs` - ACP agent runner (work in progress)
- **Automation**: `Automation/RunAcpAction.cs` - ClassIsland automation action

## UI Conventions

- FluentAvalonia for UI controls
- `ci:` namespace: ClassIsland core controls (`ci:SettingsPageBase`, `ci:FluentIcon`, `ci:IconText`)
- `fa:` namespace: FluentAvalonia controls (`fa:SettingsExpander`, `fa:ComboBox`)
- Icons use FluentIcon Glyph hex codes (e.g., `&#xE774;` for settings)
- Settings pages inherit from `ci:SettingsPageBase`

## Key NuGet Packages

- `ClassIsland.PluginSdk` (v1.7.106.2-dev-v2) - plugin SDK, exclude runtime/native assets
- `DotNetCampus.ModelContextProtocol` (v0.1.0-alpha.40) - MCP server implementation
- `AgentClientProtocol` (v0.1.5) - ACP client

## Files to Ignore

- `reference/` - reference code, not part of build
- `cipx/` - generated plugin packages
- `.qoder/` - specs, contains sensitive data

## Settings Storage

Settings auto-persist to `{PluginConfigFolder}/Settings.json` via `ConfigureFileHelper`. Property changes trigger save automatically.

## MCP Server

Default port: `5943`. Endpoints: `/mcp` (Streamable HTTP), `/sse` (SSE, currently disabled). Server starts/stops with ClassIsland lifecycle via `AppBase.Current.AppStarted`/`AppStopping` events.
