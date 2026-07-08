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

public sealed class ListComponentTextTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new { }
    });

    public string ToolName => "list_component_text";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext) => new()
    {
        Name = ToolName,
        Title = "List AI Text Entries",
        Description = "列出当前所有的 AI 文字条目（即 ClassIsland 主界面 AI 文字组件可显示的内容），包含每个条目的 ID、描述、文字内容。",
        InputSchema = InputSchema,
        Annotations = new ToolAnnotations { ReadOnlyHint = true }
    };

    public ValueTask<CallToolResult> CallTool(IMcpServerCallToolContext context)
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        telemetry?.AddBreadcrumb("Tool call: list_component_text", "mcp.tool", BreadcrumbLevel.Info);

        try
        {
            var _logger = IAppHost.GetService<ILogger<ListComponentTextTool>>();
            _logger?.LogDebug("调用 list_component_text");

            var entries = UiThreadHelper.RunOnUi(() =>
                Plugin.Settings.AiTextEntries
                    .Select(e => new ComponentTextEntry(e.Id, e.Description, e.Text, e.DisplayName))
                    .ToList());

            _logger?.LogInformation("列出组件文字条目，共 {Count} 条", entries.Count);

            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new ComponentTextListResult(entries),
                AgentIslandJsonContext.Default.ComponentTextListResult));
        }
        catch (Exception ex)
        {
            telemetry?.CaptureException(ex, "list_component_text");
            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new ComponentTextListResult(new List<ComponentTextEntry>()),
                AgentIslandJsonContext.Default.ComponentTextListResult));
        }
    }
}
