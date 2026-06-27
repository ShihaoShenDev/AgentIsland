using System.Globalization;
using AgentIsland.Helpers;
using AgentIsland.Models;
using AgentIsland.Services;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using ClassIsland.Shared.Models.Profile;
using DotNetCampus.ModelContextProtocol.CompilerServices;
using Microsoft.Extensions.Logging;

namespace AgentIsland.Mcp.Tools;

public sealed class ScheduleTools
{
    [McpServerTool(Name = "get_today_schedule", ReadOnly = true, Structured = true)]
    public ScheduleResult GetTodaySchedule()
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        return telemetry?.WithInstrumentation("get_today_schedule", GetTodayScheduleCore)
            ?? GetTodayScheduleCore();
    }

    private static ScheduleResult GetTodayScheduleCore()
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<ScheduleTools>>();
            _logger?.LogDebug("获取今日课程表");
            DateTime date = DateTime.Today;
            ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
            IProfileService profileService = IAppHost.GetService<IProfileService>();
            ClassPlan? classPlan = lessonsService.CurrentClassPlan
                ?? lessonsService.GetClassPlanByDate(date, out _);

            return classPlan is null
                ? new ScheduleResult("", date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), [])
                : BuildScheduleResult(classPlan, profileService, date);
        });
    }

    public ScheduleResult GetScheduleByDate(string dateString)
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<ScheduleTools>>();
            _logger?.LogDebug("获取 {Date} 的课程表", dateString);
            DateTime targetDate = ParseDate(dateString);
            ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
            IProfileService profileService = IAppHost.GetService<IProfileService>();
            ClassPlan? classPlan = lessonsService.GetClassPlanByDate(targetDate, out _);

            return classPlan is null
                ? new ScheduleResult("", targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), [])
                : BuildScheduleResult(classPlan, profileService, targetDate);
        });
    }

    public SwapResult SwapClasses(int classIndex1, int classIndex2, string date)
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            try
            {
                var _logger = IAppHost.GetService<ILogger<ScheduleTools>>();
                _logger?.LogInformation("交换课程: {Index1} <-> {Index2}, 日期: {Date}", classIndex1, classIndex2, date);
                DateTime targetDate = ParseDate(date);
                ILessonsService lessonsService = IAppHost.GetService<ILessonsService>();
                IProfileService profileService = IAppHost.GetService<IProfileService>();

                ClassPlan? classPlan = lessonsService.GetClassPlanByDate(targetDate, out Guid? classPlanId);
                if (classPlan is null || classPlanId is null)
                {
                    return new SwapResult(false, "No class plan found for the specified date.");
                }

                Guid originalClassPlanId = classPlan.IsOverlay && classPlan.OverlaySourceId is not null
                    ? classPlan.OverlaySourceId.Value
                    : classPlanId.Value;
                ClassPlan? targetClassPlan = GetOrCreateTempClassPlan(profileService, originalClassPlanId, targetDate);
                if (targetClassPlan is null)
                {
                    return new SwapResult(false, "Failed to create temporary class plan overlay.");
                }

                if (!IsValidIndex(targetClassPlan, classIndex1) || !IsValidIndex(targetClassPlan, classIndex2))
                {
                    return new SwapResult(false, "Class index is out of range.");
                }

                (targetClassPlan.Classes[classIndex1].SubjectId, targetClassPlan.Classes[classIndex2].SubjectId) =
                    (targetClassPlan.Classes[classIndex2].SubjectId, targetClassPlan.Classes[classIndex1].SubjectId);
                targetClassPlan.Classes[classIndex1].IsChangedClass = true;
                targetClassPlan.Classes[classIndex2].IsChangedClass = true;
                profileService.SaveProfile();

                return new SwapResult(true, "Classes swapped successfully.");
            }
            catch (Exception ex)
            {
                return new SwapResult(false, ex.Message);
            }
        });
    }

    [McpServerTool(Name = "list_subjects", ReadOnly = true, Structured = true)]
    public SubjectListResult ListSubjects()
    {
        var telemetry = IAppHost.GetService<SentryTelemetryService>();
        return telemetry?.WithInstrumentation("list_subjects", ListSubjectsCore)
            ?? ListSubjectsCore();
    }

    private static SubjectListResult ListSubjectsCore()
    {
        return UiThreadHelper.RunOnUi(() =>
        {
            var _logger = IAppHost.GetService<ILogger<ScheduleTools>>();
            _logger?.LogDebug("列出所有科目");
            IProfileService profileService = IAppHost.GetService<IProfileService>();
            List<SubjectEntry> subjects = profileService.Profile.Subjects
                .Select(pair => new SubjectEntry(
                    pair.Key.ToString(),
                    pair.Value.Name,
                    pair.Value.TeacherName,
                    pair.Value.Initial))
                .OrderBy(x => x.Name, StringComparer.CurrentCulture)
                .ToList();

            return new SubjectListResult(subjects);
        });
    }

    private static ScheduleResult BuildScheduleResult(ClassPlan classPlan, IProfileService profileService, DateTime date)
    {
        List<ScheduleClassEntry> classes = [];
        List<TimeLayoutItem> classTimeLayouts = classPlan.TimeLayout?.Layouts
            .Where(layout => Convert.ToInt32(layout.TimeType, CultureInfo.InvariantCulture) == 0)
            .ToList() ?? [];

        for (int i = 0; i < classPlan.Classes.Count; i++)
        {
            ClassInfo classInfo = classPlan.Classes[i];
            profileService.Profile.Subjects.TryGetValue(classInfo.SubjectId, out Subject? subject);
            TimeLayoutItem? timeLayoutItem = i < classTimeLayouts.Count ? classTimeLayouts[i] : null;

            classes.Add(new ScheduleClassEntry(
                i,
                subject?.Name ?? "",
                subject?.TeacherName ?? "",
                FormatTime(timeLayoutItem?.StartTime),
                FormatTime(timeLayoutItem?.EndTime),
                classInfo.IsChangedClass,
                classInfo.IsEnabled));
        }

        return new ScheduleResult(
            classPlan.Name,
            date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            classes);
    }

    private static ClassPlan? GetOrCreateTempClassPlan(IProfileService profileService, Guid originalClassPlanId, DateTime date)
    {
        if (profileService.Profile.OrderedSchedules.TryGetValue(date, out OrderedSchedule? orderedSchedule) &&
            profileService.Profile.ClassPlans.TryGetValue(orderedSchedule.ClassPlanId, out ClassPlan? existingOverlay) &&
            existingOverlay.IsOverlay &&
            existingOverlay.OverlaySourceId == originalClassPlanId)
        {
            return existingOverlay;
        }

        Guid? tempClassPlanId = profileService.CreateTempClassPlan(originalClassPlanId, enableDateTime: date);
        return tempClassPlanId is not null &&
               profileService.Profile.ClassPlans.TryGetValue(tempClassPlanId.Value, out ClassPlan? plan)
            ? plan
            : null;
    }

    private static bool IsValidIndex(ClassPlan classPlan, int index)
    {
        return index >= 0 && index < classPlan.Classes.Count;
    }

    private static DateTime ParseDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return DateTime.Today;
        }

        if (DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            return parsed.Date;
        }

        throw new ArgumentException("Invalid date format. Use yyyy-MM-dd.");
    }

    private static string? FormatTime(TimeSpan? time)
    {
        return time?.ToString(@"hh\:mm\:ss");
    }
}
