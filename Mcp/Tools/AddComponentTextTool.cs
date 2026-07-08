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

public sealed class AddComponentTextTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            id = new { type = "string", description = "条目 ID，需唯一；ClassIsland 主界面上的 AI 文字组件通过此 ID 绑定到对应条目。" },
            text = new { type = "string", description = "（可选）要显示的文字内容，默认为空。" },
            description = new { type = "string", description = "（可选）条目描述，用于在设置页中区分不同条目，默认为空。" }
        },
        required = new[] { "id" }
    });

    public string ToolName => "add_component_text";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext) => new()
    {
        Name = ToolName,
        Title = "Add AI Text Entry",
        Description = "新增一个 AI 文字条目（对应 ClassIsland 主界面 AI 文字组件可显示的内容）。若指定 ID 已存在则失败。",
        InputSchema = InputSchema,
        Annotations = new ToolAnnotations { ReadOnlyHint = false, IdempotentHint = false }
    };

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: add_component_text", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            JsonElement args = context.InputJsonArguments;
            if (!TryGetString(args, "id", out string id))
                return Result(false, "Missing required parameter 'id'.");

            string text = TryGetString(args, "text", out string t) ? t : "";
            string description = TryGetString(args, "description", out string d) ? d : "";

            var _logger = IAppHost.GetService<ILogger<AddComponentTextTool>>();
            _logger?.LogDebug("调用 add_component_text, 组件ID: {Id}", id);

            bool added = false;
            string message = "";
            UiThreadHelper.RunOnUi(() =>
            {
                if (Plugin.Settings.AiTextEntries.Any(e => e.Id == id))
                {
                    message = $"Entry with id '{id}' already exists.";
                    return;
                }

                Plugin.Settings.AiTextEntries.Add(new AiTextEntry
                {
                    Id = id,
                    Text = text,
                    Description = description
                });
                added = true;
                message = $"Entry '{id}' added.";
            });

            if (added)
                _logger?.LogInformation("新增组件文字条目: {Id}", id);

            return Result(added, message);
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "add_component_text");
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
