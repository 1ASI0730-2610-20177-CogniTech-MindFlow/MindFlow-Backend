using System.Text.Json.Serialization;

namespace Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;

public record CreateHabitLogResource(
    [property: JsonPropertyName("habit_id")] int HabitId,
    [property: JsonPropertyName("habit_name")] string HabitName,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("completed")] bool Completed,
    [property: JsonPropertyName("completed_at")] DateTime? CompletedAt
);
