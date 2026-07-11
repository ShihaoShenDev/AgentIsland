using System;
using System.Text.Json;
using System.Threading.Tasks;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions;
using ClassIsland.Shared;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using DotNetCampus.ModelContextProtocol.Protocol.Messages;
using DotNetCampus.ModelContextProtocol.Servers;
using Microsoft.Extensions.Logging;
using Sentry;

namespace AgentIsland.Mcp.Tools;

public sealed class GetScheduleByDateTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            date = new
            {
                type = "string",
                description = "要查询的日期，格式 yyyy-MM-dd；例如 2026-06-19。"
            }
        },
        required = new[] { "date" }
    });

    public string ToolName => "get_schedule_by_date";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext)
    {
        return new Tool
        {
            Name = ToolName,
            Title = "Get Schedule By Date",
            Description = "查询指定日期的课程表信息。",
            InputSchema = InputSchema,
            OutputSchema = null,
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = true,
                DestructiveHint = false,
                IdempotentHint = true,
                OpenWorldHint = false
            }
        };
    }

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: get_schedule_by_date", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            JsonElement arguments = context.InputJsonArguments;
            string date = ReadRequiredString(arguments, "date");

            var _logger = IAppHost.GetService<ILogger<GetScheduleByDateTool>>();
            _logger?.LogDebug("调用 get_schedule_by_date, 日期: {Date}", date);

            ScheduleResult result = new ScheduleTools().GetScheduleByDate(date);
            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                result,
                AgentIslandJsonContext.Default.ScheduleResult));
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "get_schedule_by_date");
            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new ScheduleResult($"Error: {ex.Message}", "", new System.Collections.Generic.List<ScheduleClassEntry>()),
                AgentIslandJsonContext.Default.ScheduleResult));
        }
    }

    private static string ReadRequiredString(JsonElement arguments, string name)
    {
        if (arguments.ValueKind != JsonValueKind.Object ||
            !arguments.TryGetProperty(name, out JsonElement value) ||
            value.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException($"Missing or invalid required string parameter '{name}'.");
        }

        return value.GetString()!;
    }
}
