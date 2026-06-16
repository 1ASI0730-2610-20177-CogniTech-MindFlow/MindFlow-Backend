using System.Text.Json.Serialization;

namespace Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

public record HabitResource(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("streak")] int Streak,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("paused_by_ai")] bool PausedByAi,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt,
    [property: JsonPropertyName("deleted_at")] DateTimeOffset? DeletedAt
);
