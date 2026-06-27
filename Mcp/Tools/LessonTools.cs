using AgentIsland.Helpers;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using ClassIsland.Shared.Models.Profile;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using Microsoft.Extensions.Logging;

namespace AgentIsland.Mcp.Tools;

public sealed class LessonTools
{
    [McpServerTool(Name = "get_current_class", ReadOnly = true, Structured = true)]
    public CurrentClassResult GetCurrentClass()
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        return telemetry?.WithInstrumentation("get_current_class", GetCurrentClassCore)
            ?? GetCurrentClassCore();
    }

    private static CurrentClassResult GetCurrentClassCore()
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<LessonTools>>();
            _logger?.LogDebug("调用 get_current_class");
            ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
            Subject? subject = lessonsService.CurrentSubject;
            if (subject is null || NormalizeState(lessonsService.CurrentState) != "InClass")
            {
                return new CurrentClassResult("", "", null, null, 0, false);
            }

            TimeLayoutItem? timeLayoutItem = lessonsService.CurrentTimeLayoutItem;

            return new CurrentClassResult(
                subject.Name,
                subject.TeacherName,
                FormatTime(timeLayoutItem?.StartTime),
                FormatTime(timeLayoutItem?.EndTime),
                ToSeconds(lessonsService.OnClassLeftTime),
                true);
        });
    }

    [McpServerTool(Name = "get_next_class", ReadOnly = true, Structured = true)]
    public NextClassResult GetNextClass()
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        return telemetry?.WithInstrumentation("get_next_class", GetNextClassCore)
            ?? GetNextClassCore();
    }

    private static NextClassResult GetNextClassCore()
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<LessonTools>>();
            _logger?.LogDebug("调用 get_next_class");
            ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
            IExactTimeService exactTimeService = IAppHost.GetService<IExactTimeService>();

            Subject? subject = lessonsService.NextClassSubject;
            TimeLayoutItem? timeLayoutItem = lessonsService.NextClassTimeLayoutItem;
            if (subject is null || timeLayoutItem is null)
            {
                return new NextClassResult("", "", null, null, 0, false);
            }

            DateTime now = exactTimeService.GetCurrentLocalDateTime();
            DateTime startTime = now.Date + timeLayoutItem.StartTime;
            int secondsUntilStart = Math.Max(0, (int)(startTime - now).TotalSeconds);

            return new NextClassResult(
                subject.Name,
                subject.TeacherName,
                FormatTime(timeLayoutItem.StartTime),
                FormatTime(timeLayoutItem.EndTime),
                secondsUntilStart,
                true);
        });
    }

    [McpServerTool(Name = "get_time_status", ReadOnly = true, Structured = true)]
    public TimeStatusResult GetTimeStatus()
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        return telemetry?.WithInstrumentation("get_time_status", GetTimeStatusCore)
            ?? GetTimeStatusCore();
    }

    private static TimeStatusResult GetTimeStatusCore()
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<LessonTools>>();
            _logger?.LogDebug("调用 get_time_status");
            ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
            IExactTimeService exactTimeService = IAppHost.GetService<IExactTimeService>();
            string state = NormalizeState(lessonsService.CurrentState);
            TimeSpan leftTime = state == "Breaking"
                ? lessonsService.OnBreakingTimeLeftTime
                : state == "InClass"
                    ? lessonsService.OnClassLeftTime
                    : TimeSpan.Zero;

            return new TimeStatusResult(
                state,
                ToSeconds(leftTime),
                exactTimeService.GetCurrentLocalDateTime().ToString("O"));
        });
    }

    private static string NormalizeState(object currentState)
    {
        string value = currentState.ToString() ?? "";
        if (value.Contains("Break", StringComparison.OrdinalIgnoreCase))
        {
            return "Breaking";
        }

        if (value.Contains("After", StringComparison.OrdinalIgnoreCase))
        {
            return "AfterSchool";
        }

        if (value.Contains("Class", StringComparison.OrdinalIgnoreCase))
        {
            return "InClass";
        }

        return value;
    }

    private static int ToSeconds(TimeSpan timeSpan)
    {
        return Math.Max(0, (int)timeSpan.TotalSeconds);
    }

    private static string? FormatTime(TimeSpan? time)
    {
        return time?.ToString(@"hh\:mm\:ss");
    }
}
