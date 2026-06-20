using System;
using System.Text.Json;
using System.Threading.Tasks;
using AgentIsland.Models;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using DotNetCampus.ModelContextProtocol.Protocol.Messages;
using DotNetCampus.ModelContextProtocol.Servers;

namespace AgentIsland.Mcp.Tools;

public sealed class SendNotificationTool : IMcpServerTool
{
    private static readonly JsonElement InputSchema = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            message = new
            {
                type = "string",
                description = "通知的主要标题/遮罩文字。"
            },
            body = new
            {
                type = "string",
                description = "通知的详细内容/正文，可选。"
            },
            maskDuration = new
            {
                type = "number",
                description = "遮罩的显示时间（秒），默认 3.0。"
            },
            overlayDuration = new
            {
                type = "number",
                description = "正文的显示时间（秒），默认 5.0。"
            }
        },
        required = new[] { "message" }
    });

    public string ToolName => "send_notification";

    public Tool GetToolDefinition(CompiledSchemaJsonContext jsonContext)
    {
        return new Tool
        {
            Name = ToolName,
            Title = "Send Notification",
            Description = "在 ClassIsland 界面上显示一条提醒通知。",
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
        try
        {
            JsonElement arguments = context.InputJsonArguments;
            string message = ReadRequiredString(arguments, "message");
            string body = ReadOptionalString(arguments, "body") ?? "";
            double maskDuration = ReadOptionalDouble(arguments, "maskDuration") ?? 3.0;
            double overlayDuration = ReadOptionalDouble(arguments, "overlayDuration") ?? 5.0;

            if (AgentIslandNotificationProvider.Instance is null)
            {
                return ValueTask.FromResult(CallToolResult.FromResultStructured(
                    new NotificationResult(false, "Notification provider is not initialized yet."),
                    AgentIslandJsonContext.Default.NotificationResult));
            }

            AgentIslandNotificationProvider.Instance.Notify(message, body, maskDuration, overlayDuration);

            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new NotificationResult(true, "Notification sent successfully."),
                AgentIslandJsonContext.Default.NotificationResult));
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(CallToolResult.FromResultStructured(
                new NotificationResult(false, ex.Message),
                AgentIslandJsonContext.Default.NotificationResult));
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

    private static string? ReadOptionalString(JsonElement arguments, string name)
    {
        return arguments.ValueKind == JsonValueKind.Object &&
               arguments.TryGetProperty(name, out JsonElement value) &&
               value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static double? ReadOptionalDouble(JsonElement arguments, string name)
    {
        return arguments.ValueKind == JsonValueKind.Object &&
               arguments.TryGetProperty(name, out JsonElement value) &&
               value.TryGetDouble(out double val)
            ? val
            : null;
    }
}
