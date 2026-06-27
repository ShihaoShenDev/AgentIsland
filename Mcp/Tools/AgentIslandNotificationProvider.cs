using System;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;
using Microsoft.Extensions.Logging;

namespace AgentIsland.Mcp.Tools;

[NotificationProviderInfo("687EFB6B-0639-44F4-A0EC-29177F8FE582", "智能体通知", "\uE3E4", "通过智能体发送通知提醒")]
[NotificationChannelInfo("A4D4B4A0-34D1-4E12-9E54-4F76F12AE77B", "智能体消息", "\uE3E4", "智能体消息渠道")]
public class AgentIslandNotificationProvider : NotificationProviderBase
{
    private const string MessageChannelId = "A4D4B4A0-34D1-4E12-9E54-4F76F12AE77B";
    private static AgentIslandNotificationProvider? _instance;
    public static AgentIslandNotificationProvider? Instance => _instance;

    private readonly ILogger<AgentIslandNotificationProvider>? _logger;

    public AgentIslandNotificationProvider(ILogger<AgentIslandNotificationProvider>? logger = null)
    {
        _logger = logger;
        _instance = this;
        _logger?.LogDebug("通知提供方已初始化");
    }

    public void Notify(string maskText, string? overlayText = null, double maskDuration = 3.0, double overlayDuration = 5.0)
    {
        _logger?.LogDebug("发送通知: {MaskText}", maskText);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var mask = NotificationContent.CreateTwoIconsMask(maskText);
            mask.Duration = TimeSpan.FromSeconds(maskDuration);

            var request = new NotificationRequest
            {
                MaskContent = mask
            };

            if (!string.IsNullOrWhiteSpace(overlayText) && overlayDuration > 0)
            {
                var overlay = NotificationContent.CreateSimpleTextContent(overlayText);
                overlay.Duration = TimeSpan.FromSeconds(overlayDuration);
                request.OverlayContent = overlay;
            }

            Channel(MessageChannelId).ShowNotification(request);
        });
    }
}
