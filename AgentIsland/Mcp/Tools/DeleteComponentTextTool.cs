using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgentIsland.Helpers;
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

public sealed class DeleteComponentTextTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            id = new { type = "string", description = "要删除的条目 ID。" }
        },
        required = new[] { "id" }
    });

    public string ToolName => "delete_component_text";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext) => new()
    {
        Name = ToolName,
        Title = "Delete AI Text Entry",
        Description = "按 ID 删除单个 AI 文字条目。若该 ID 不存在则失败。如需清空全部条目请使用 clear_component_text。",
        InputSchema = InputSchema,
        Annotations = new ToolAnnotations { ReadOnlyHint = false, IdempotentHint = true }
    };

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: delete_component_text", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            JsonElement args = context.InputJsonArguments;
            if (!TryGetString(args, "id", out string id))
                return Result(false, "Missing required parameter 'id'.");

            var _logger = IAppHost.GetService<ILogger<DeleteComponentTextTool>>();
            _logger?.LogDebug("调用 delete_component_text, 组件ID: {Id}", id);

            bool success = false;
            string message = "";
            UiThreadHelper.RunOnUi(() =>
            {
                var entry = Plugin.Settings.AiTextEntries.FirstOrDefault(e => e.Id == id);
                if (entry is null)
                {
                    message = $"Entry with id '{id}' not found.";
                    return;
                }

                Plugin.Settings.AiTextEntries.Remove(entry);
                success = true;
                message = $"Entry '{id}' deleted.";
            });

            if (success)
                _logger?.LogInformation("删除组件文字条目: {Id}", id);

            return Result(success, message);
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "delete_component_text");
            return Result(false, ex.Message);
        }
    }

    private static bool TryGetString(JsonElement args, string name, out string value)
    {
        if (args.ValueKind == JsonValueKind.Object &&
            args.TryGetProperty(name, out JsonElement el) &&
            el.ValueKind == JsonValueKind.String)
        {
            value = el.GetString()!;
            return true;
        }
        value = "";
        return false;
    }

    private static ValueTask<CallToolResult> Result(bool success, string message) =>
        ValueTask.FromResult(CallToolResult.FromResultStructured(
            new SetTextResult(success, message),
            AgentIslandJsonContext.Default.SetTextResult));
}
