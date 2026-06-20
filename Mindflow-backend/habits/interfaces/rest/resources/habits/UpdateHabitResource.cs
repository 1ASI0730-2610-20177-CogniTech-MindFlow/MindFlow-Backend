using System.Text.Json.Serialization;

namespace Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

public record UpdateHabitResource(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("frequency")] string Frequency
);
