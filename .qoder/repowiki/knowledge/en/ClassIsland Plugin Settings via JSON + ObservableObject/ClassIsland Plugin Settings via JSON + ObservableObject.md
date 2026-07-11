---
kind: configuration_system
name: ClassIsland Plugin Settings via JSON + ObservableObject
category: configuration_system
scope:
    - '**'
source_files:
    - Plugin.cs
    - Models/AgentIslandSettings.cs
    - Models/AiTextEntry.cs
    - Models/AcpAgentProfile.cs
    - Models/RunAcpActionSettings.cs
    - Models/AiTextComponentSettings.cs
    - Models/McpTransportMode.cs
---

AgentIsland uses a single-file, in-process configuration system provided by the ClassIsland host. There is no custom config loader — all persistence and change propagation are delegated to the host's `ConfigureFileHelper` and the plugin's own MVVM model layer.

**Storage format and location**
- A single `Settings.json` file lives under the ClassIsland plugin config directory (`PluginConfigFolder/Settings.json`).
- The file is loaded once during `Plugin.Initialize` and saved back on every property change of the root settings object.
- No environment variables, feature flags, or multi-source merging are used; the JSON file is the sole source of truth.

**Root settings object**
- `Models/AgentIslandSettings.cs` is the top-level settings class. It derives from `CommunityToolkit.Mvvm.ObservableObject` and exposes properties such as `Port`, `TransportMode`, `IsEnabled`, `IsAcpEnabled`, `IsAgentAutomationEnabled`, `AutoStartAgentsWithClassIsland`, `ShowAutomationNotifications`, `AiTextEntries`, `AcpAgents`, `IsTelemetryEnabled`, `HasAgreedToPrivacyPolicy`, and `CustomSentryDsn`.
- Each property is annotated with `[JsonPropertyName(...)]` so serialization keys match the persisted JSON shape.
- Derived/read-only computed properties (`ConnectionAddress`, `EffectiveSentryDsn`, `IsTelemetryActive`, `CanToggleTelemetry`, agent counts) are kept in sync via `OnPropertyChanged` overrides.
- Two collections (`AiTextEntries`, `AcpAgents`) are `ObservableCollection<T>` whose items also derive from `ObservableObject`; the parent hooks/unhooks their `PropertyChanged` events so any nested change triggers a save.

**Child setting models**
- `Models/AiTextEntry.cs` — per-entry text configuration (id, description, text).
- `Models/AcpAgentProfile.cs` — per-agent profile (name, command, isEnabled, status).
- `Models/RunAcpActionSettings.cs` — settings for the "Run ACP" automation action (agent name, notification toggle, custom payload).
- `Models/AiTextComponentSettings.cs` — runtime component-scoped settings (entry id, placeholder text), derived from `ObservableRecipient` rather than persisted directly.
- `Models/McpTransportMode.cs` — enum backing the transport-mode setting.

**Persistence mechanism**
- Load: `Plugin.Initialize` calls `ConfigureFileHelper.LoadConfig<AgentIslandSettings>(Path.Combine(PluginConfigFolder, "Settings.json"))`.
- Save: a `PropertyChanged` handler on the root `Settings` instance invokes `ConfigureFileHelper.SaveConfig(path, Settings)` after every mutation.
- This means there is no explicit "save" button — changes are written immediately to disk.

**Dependency injection integration**
- The fully-loaded `AgentIslandSettings` singleton is registered into the ClassIsland DI container via `services.AddSingleton(Settings)`, making it available to actions, services, and components without further lookup.

**UI binding**
- Settings pages (`Views/SettingsPages/*.axaml`) bind directly to the same `AgentIslandSettings` instance, so UI edits flow straight through the observable properties into the persistent JSON file.

**Rules developers should follow**
1. Persisted settings must be defined as `[JsonPropertyName]`-annotated properties on classes that derive from `ObservableObject` (or `ObservableRecipient` for view-scoped settings).
2. New collection-based settings must be `ObservableCollection<T>` where T is an `ObservableObject`, and the parent must hook/unhook child `PropertyChanged` events so nested mutations trigger saves.
3. Do not add separate config files — everything belongs in `AgentIslandSettings` (and its referenced child types) serialized into the single `Settings.json`.
4. Avoid reading/writing `Settings.json` directly outside `Plugin.Initialize`; use the DI-resolved `AgentIslandSettings` singleton instead.