本仓库采用 .NET 标准异常体系，未定义自定义错误类型或统一 Result 模式。错误处理呈现以下特点：

1. **参数校验**：在 MCP Tools 层使用 `ArgumentException` 抛出参数缺失/格式错误（如日期、整数参数），由调用方捕获并包装为工具结果。
2. **业务状态异常**：在自动化动作中使用 `InvalidOperationException` 表达“功能未启用”“Agent 不存在/已停用”等可恢复的业务错误。
3. **外围边界 try-catch**：MCP 服务器启动/停止、各 Tool 执行入口均包裹 try-catch(Exception)，将异常通过 `SentryTelemetryService.CaptureException` 上报 Sentry，并返回友好的错误字符串给上层。
4. **致命异常识别**：参考子模块 IslandMQ 的 `ExceptionHelper.IsFatal` 提供 OOM/AccessViolation 判断，用于区分应崩溃还是可恢复的错误。
5. **无全局中间件**：未实现统一的异常中间件或 panic/recover 机制；每个调用点自行决定捕获范围。
6. **日志记录**：插件生命周期中的关键失败路径使用 `_logger.LogError` 输出结构化错误信息。

开发者约定：对外暴露的 API 应在入口处捕获所有异常并通过遥测服务上报，同时向调用者返回人类可读的错误描述；内部参数校验优先使用 `ArgumentNullException.ThrowIfNull` 和显式 `ArgumentException`。