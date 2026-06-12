using System.Text.Json.Serialization;

namespace Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;

public record UpdateHabitLogResource(
    [property: JsonPropertyName("habit_name")] string HabitName,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("completed")] bool Completed,
    [property: JsonPropertyName("completed_at")] DateTime? CompletedAt
);
