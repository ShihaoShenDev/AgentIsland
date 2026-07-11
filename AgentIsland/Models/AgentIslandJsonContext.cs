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
[JsonSerializable(typeof(SetTextResult))]
[JsonSerializable(typeof(EchoCaveResult))]
[JsonSerializable(typeof(ComponentTextListResult))]
[JsonSerializable(typeof(ComponentTextEntry))]
[JsonSerializable(typeof(AiTextEntry))]
[JsonSerializable(typeof(List<ScheduleClassEntry>))]
[JsonSerializable(typeof(List<SubjectEntry>))]
[JsonSerializable(typeof(List<ComponentTextEntry>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AgentIslandJsonContext : JsonSerializerContext;
