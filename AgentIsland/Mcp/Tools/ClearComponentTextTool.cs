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

public sealed class ClearComponentTextTool : IMcpServerTool
{
    private const string All = "all";

    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            id = new { type = "string", description = "要置空文字内容的条目 ID；传入 \"all\"（不区分大小写）则置空所有条目的文字。条目本身（ID、描述）保留。" }
        },
        required = new[] { "id" }
    });

    public string ToolName => "clear_component_text";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext) => new()
    {
        Name = ToolName,
        Title = "Clear AI Text Entry Content",
        Description = "按 ID 把 AI 文字条目的文字内容置空（条目本身保留）。id=\"all\" 时置空全部条目的文字。如需删除条目本身请使用 delete_component_text。",
        InputSchema = InputSchema,
        Annotations = new ToolAnnotations { ReadOnlyHint = false, IdempotentHint = true }
    };

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: clear_component_text", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            JsonElement args = context.InputJsonArguments;
            if (!TryGetString(args, "id", out string id))
                return Result(false, "Missing required parameter 'id'.");

            var _logger = IAppHost.GetService<ILogger<ClearComponentTextTool>>();
            _logger?.LogDebug("调用 clear_component_text, 组件ID: {Id}", id);

            bool success = false;
            string message = "";
            UiThreadHelper.RunOnUi(() =>
            {
                if (string.Equals(id, All, StringComparison.OrdinalIgnoreCase))
                {
                    int count = Plugin.Settings.AiTextEntries.Count;
                    if (count == 0)
                    {
                        success = true;
                        message = "No text entries to clear.";
                        return;
                    }

                    foreach (var entry in Plugin.Settings.AiTextEntries)
                        entry.Text = "";

                    success = true;
                    message = $"Cleared text of {count} entr{(count == 1 ? "y" : "ies")}.";
                    return;
                }

                var target = Plugin.Settings.AiTextEntries.FirstOrDefault(e => e.Id == id);
                if (target is null)
                {
                    message = $"Entry with id '{id}' not found.";
                    return;
                }

                target.Text = "";
                success = true;
                message = $"Text of entry '{id}' cleared.";
            });

            if (success)
                _logger?.LogInformation("置空组件文字内容: {Id}", id);

            return Result(success, message);
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "clear_component_text");
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
