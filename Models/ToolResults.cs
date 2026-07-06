namespace AgentIsland.Models;

public sealed record CurrentClassResult(
    string SubjectName,
    string TeacherName,
    string? StartTime,
    string? EndTime,
    int RemainingSeconds,
    bool IsInClass);

public sealed record NextClassResult(
    string SubjectName,
    string TeacherName,
    string? StartTime,
    string? EndTime,
    int SecondsUntilStart,
    bool HasNextClass);

public sealed record TimeStatusResult(
    string CurrentState,
    int RemainingSeconds,
    string CurrentTime);

public sealed record ScheduleResult(
    string ClassPlanName,
    string Date,
    List<ScheduleClassEntry> Classes);

public sealed record ScheduleClassEntry(
    int Index,
    string SubjectName,
    string TeacherName,
    string? StartTime,
    string? EndTime,
    bool IsChangedClass,
    bool IsEnabled);

public sealed record SwapResult(
    bool Success,
    string Message);

public sealed record SubjectListResult(
    List<SubjectEntry> Subjects);

public sealed record SubjectEntry(
    string Id,
    string Name,
    string TeacherName,
    string Initial);

public sealed record NotificationResult(
    bool Success,
    string Message);

public sealed record SetTextResult(
    bool Success,
    string Message);

public sealed record EchoCaveResult(
    string Content);

