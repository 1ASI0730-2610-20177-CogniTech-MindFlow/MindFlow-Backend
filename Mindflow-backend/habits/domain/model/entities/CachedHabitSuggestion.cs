namespace Mindflow_backend.Habits.Domain.Model.Entities;

public class CachedHabitSuggestion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SuggestionsJson { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
}
