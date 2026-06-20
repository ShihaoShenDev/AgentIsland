using System.Text.Json.Serialization;

namespace AgentIsland.Models;

[JsonSerializable(typeof(CurrentClassResult))]
[JsonSerializable(typeof(NextClassResult))]
[JsonSerializable(typeof(TimeStatusResult))]
[JsonSerializable(typeof(ScheduleResult))]
[JsonSerializable(typeof(ScheduleClassEntry))]
[JsonSerializable(typeof(SwapResult))]
[JsonSerializable(typeof(SubjectListResult))]
[JsonSerializable(typeof(SubjectEntry))]
[JsonSerializable(typeof(NotificationResult))]
[JsonSerializable(typeof(List<ScheduleClassEntry>))]
[JsonSerializable(typeof(List<SubjectEntry>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AgentIslandJsonContext : JsonSerializerContext;
