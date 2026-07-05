This repository uses a straightforward exception-driven error handling strategy rather than a dedicated error type system. Errors are represented by standard .NET exceptions, propagated up the call stack, and captured via logging and optional Sentry telemetry. There is no custom Error/Result monad or sentinel-error pattern; instead, callers rely on try/catch blocks around specific operations and structured logging for diagnostics.

### What system/approach is used
- Standard .NET exceptions (InvalidOperationException, ArgumentException, Exception) are thrown to signal invalid state, bad input, or unexpected failures.
- Microsoft.Extensions.Logging (ILogger<T>) is used throughout for structured log output at Information, Warning, Debug, and Error levels.
- Sentry SDK is integrated through a thin SentryTelemetryService wrapper that conditionally captures exceptions and breadcrumbs based on user privacy settings. It provides CaptureException, AddBreadcrumb, and instrumentation helpers WithInstrumentation / WithInstrumentationAsync.
- MCP tool layer wraps most tool calls in try/catch blocks and returns structured result DTOs (e.g., NotificationResult, SwapResult, ScheduleResult) carrying a success flag plus an error message string, so MCP clients receive errors as data rather than HTTP-level failures.

### Key files and packages
- Services/SentryTelemetryService.cs — centralized Sentry initialization, lifecycle management, exception capture, breadcrumb recording, and async/sync instrumentation wrappers.
- Plugin.cs — top-level plugin entry point; catches startup/shutdown exceptions from the MCP server manager, logs them, and forwards to Sentry.
- Mcp/McpServerManager.cs — wraps MCP server start/stop in try/catch, finishes Sentry transactions with SpanStatus.Ok / SpanStatus.InternalError, rethrows after reporting.
- Automation/RunAcpAction.cs — validates preconditions and throws InvalidOperationException with descriptive messages when ACP features are disabled or agents are missing/disabled.
- Mcp/Tools/ScheduleTools.cs — uses try/catch inside UI-thread-bound operations, returning SwapResult(false, ex.Message) on failure; throws ArgumentException for malformed date strings.
- Mcp/Tools/SendNotificationTool.cs — parses JSON arguments with helper methods that throw ArgumentException on missing/invalid fields; outer try/catch captures any runtime exception, reports it to Sentry, and returns a structured failure result.

### Architecture and conventions
1. Throw early on invalid input/state: Parameter validation helpers raise ArgumentException; feature-gate checks raise InvalidOperationException. This keeps error paths explicit and testable.
2. Wrap long-running or external operations: McpServerManager.StartAsync / StopAsync and Plugin.OnAppStarted / OnAppStopping surround critical sections with try/catch, log the exception, forward to Sentry, then rethrow so the host can observe the failure.
3. MCP tools return structured results: Instead of throwing out of CallTool, tools catch exceptions, record them via telemetry?.CaptureException(ex, "<tool>"), and return a result object with success: false and message: ex.Message. This ensures the MCP transport layer stays stable even when underlying logic fails.
4. Optional telemetry: All Sentry calls are guarded by _sentryHandle is null checks or optional chaining (_telemetry?.CaptureException(...)), so the plugin remains fully functional when telemetry is disabled.
5. No global middleware or panic/recover: There is no application-wide exception filter or try { ... } finally { ... } top-level handler beyond the plugin lifecycle hooks. Each component owns its own error boundary.

### Rules developers should follow
- Prefer throwing typed exceptions (ArgumentException, InvalidOperationException) for parameter/state violations; do not swallow them silently.
- For operations that cross process/UI boundaries (MCP tool calls, file I/O, network calls), wrap in try/catch, log with ILogger, optionally report via SentryTelemetryService.CaptureException, and either rethrow (for lifecycle methods) or return a structured failure result (for MCP tools).
- Use SentryTelemetryService.WithInstrumentation / WithInstrumentationAsync to automatically create transactions, breadcrumbs, and exception capture around tool implementations.
- Always check _sentryHandle is null before calling into Sentry; never assume telemetry is active.
- Do not introduce custom error types or Result-returning patterns — keep the codebase consistent with the existing exception-first approach.