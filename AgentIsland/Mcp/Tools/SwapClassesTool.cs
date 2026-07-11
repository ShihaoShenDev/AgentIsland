using System.Text.Json;
using AgentIsland.Helpers;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using DotNetCampus.ModelContextProtocol.Protocol.Messages;
using DotNetCampus.ModelContextProtocol.Servers;
using Microsoft.Extensions.Logging;
using Sentry;

namespace AgentIsland.Mcp.Tools;

public sealed class SwapClassesTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            classIndex1 = new
            {
                type = "integer",
                description = "第一节课的索引，从 0 开始。"
            },
            classIndex2 = new
            {
                type = "integer",
                description = "第二节课的索引，从 0 开始。"
            },
            date = new
            {
                type = "string",
                description = "换课日期，格式 yyyy-MM-dd；为空字符串表示今天。"
            }
        },
        required = new[] { "classIndex1", "classIndex2" }
    });

    public string ToolName => "swap_classes";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext)
    {
        return new Tool
        {
            Name = ToolName,
            Title = "Swap Classes",
            Description = "交换指定日期课表中的两节课，并创建或复用 ClassIsland 临时换课层。",
            InputSchema = InputSchema,
            OutputSchema = null,
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = false,
                DestructiveHint = false,
                IdempotentHint = false,
                OpenWorldHint = false
            }
        };
    }

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: swap_classes", "mcp.tool", BreadcrumbLevel.Info);

        JsonElement arguments = context.InputJsonArguments;
        int classIndex1 = ReadRequiredInt(arguments, "classIndex1");
        int classIndex2 = ReadRequiredInt(arguments, "classIndex2");
        string date = ReadOptionalString(arguments, "date") ?? "";

        var _logger = IAppHost.GetService<ILogger<SwapClassesTool>>();
        _logger?.LogDebug("调用 swap_classes, classIndex1: {Index1}, classIndex2: {Index2}, date: {Date}", classIndex1, classIndex2, date);

        SwapResult result = new ScheduleTools().SwapClasses(classIndex1, classIndex2, date);
        return ValueTask.FromResult(CallToolResult.FromResultStructured(
            result,
            AgentIslandJsonContext.Default.SwapResult));
    }

    private static int ReadRequiredInt(JsonElement arguments, string name)
    {
        if (arguments.ValueKind != JsonValueKind.Object ||
            !arguments.TryGetProperty(name, out JsonElement value) ||
            !value.TryGetInt32(out int result))
        {
            throw new ArgumentException($"Missing or invalid required integer parameter '{name}'.");
        }

        return result;
    }

    private static string? ReadOptionalString(JsonElement arguments, string name)
    {
        return arguments.ValueKind == JsonValueKind.Object &&
               arguments.TryGetProperty(name, out JsonElement value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
