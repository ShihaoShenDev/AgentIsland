using System;
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

public sealed class SetComponentTextTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            id = new { type = "string", description = "条目 ID，对应在设置页中创建的 AI 文字条目。" },
            text = new { type = "string", description = "要显示的文字内容。" }
        },
        required = new[] { "id", "text" }
    });

    public string ToolName => "set_component_text";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext) => new()
    {
        Name = ToolName,
        Title = "Set AI Text Component",
        Description = "按 ID 更新 ClassIsland 主界面上 AI 文字组件显示的内容。",
        InputSchema = InputSchema,
        Annotations = new ToolAnnotations { ReadOnlyHint = false, IdempotentHint = true }
    };

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: set_component_text", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            JsonElement args = context.InputJsonArguments;
            if (!TryGetString(args, "id", out string id) || !TryGetString(args, "text", out string text))
                return Result(false, "Missing required parameters 'id' and 'text'.");

            var _logger = IAppHost.GetService<ILogger<SetComponentTextTool>>();
            _logger?.LogDebug("调用 set_component_text, 组件ID: {Id}", id);
            _logger?.LogInformation("设置组件文本: {Id} -> {Text}", id, text);

            UiThreadHelper.RunOnUi(() =>
            {
                var entry = Plugin.Settings.AiTextEntries.FirstOrDefault(e => e.Id == id);
                if (entry is not null)
                    entry.Text = text;
                else
                    Plugin.Settings.AiTextEntries.Add(new AiTextEntry { Id = id, Text = text });
            });

            return Result(true, "Text updated.");
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "set_component_text");
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
