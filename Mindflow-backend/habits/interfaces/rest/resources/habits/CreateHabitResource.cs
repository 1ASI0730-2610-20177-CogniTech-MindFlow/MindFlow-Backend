using System.Text.Json.Serialization;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;

namespace Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

public record CreateHabitResource(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("frequency")] string Frequency,
    [property: JsonPropertyName("user_id")] int UserId
);
