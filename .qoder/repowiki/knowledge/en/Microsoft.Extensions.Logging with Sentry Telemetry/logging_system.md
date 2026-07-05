The AgentIsland plugin uses a two-layer observability approach built on top of the ClassIsland host's dependency injection container:

**Primary logging: Microsoft.Extensions.Logging (`ILogger<T>`)**
- Every service and tool class injects `ILogger<T>` via constructor (e.g. `Plugin`, `McpServerManager`, `RunAcpAction`, all MCP tools under `Mcp/Tools/`).
- Log levels used consistently across the codebase:
  - `LogInformation` for lifecycle events (server start/stop, agent launch, action triggers).
  - `LogWarning` for rejected operations (features disabled, agents not found or stopped).
  - `LogDebug` for per-call tracing inside MCP tools (tool entry points, parameters).
  - `LogError(ex, ...)` in catch blocks to capture exceptions with stack traces.
- Messages use structured parameter interpolation (e.g. `{TransportMode}`, `{Port}`, `{Date}`) rather than string concatenation, enabling downstream structured log sinks.
- No custom logger factory is configured in this project; the plugin relies on the ClassIsland host to provide an `ILoggerFactory` implementation through its DI container (`IAppHost.GetService<ILogger<T>>()`, `services.AddSingleton(...)`). There is no local file sink, console sink, or log-level filter defined here — output routing is delegated entirely to the host.

**Secondary telemetry: Sentry SDK (`SentryTelemetryService`)**
- A dedicated `Services/SentryTelemetryService` wraps `SentrySdk` initialization, shutdown, breadcrumb emission, exception capture, and transaction/span instrumentation.
- It is gated by user consent and settings (`IsTelemetryActive`, `HasAgreedToPrivacyPolicy`, `CustomSentryDsn`) and re-initializes dynamically when those properties change.
- The default DSN is embedded as a constant but can be overridden via settings; `SendDefaultPii` is explicitly disabled.
- Structured context is attached via tags (`plugin=AgentIsland`, `classisland.plugin=AgentIsland`) and extras (`context`, `tool`).
- `WithInstrumentation` / `WithInstrumentationAsync` helpers wrap synchronous and asynchronous operations, automatically emitting breadcrumbs, transactions, and capturing exceptions.

**Conventions observed**
- All public services are registered as singletons in `Plugin.Initialize` via `services.AddSingleton`.
- Logging calls are guarded with null-conditional operators (`_logger?.Log...`) because the injected logger may be absent during early initialization phases.
- Exceptions are always logged at `Error` level with the exception object passed as the first argument so that the underlying provider can render stack traces.
- No `LogTrace` usage was found; the lowest level used is `Debug`.