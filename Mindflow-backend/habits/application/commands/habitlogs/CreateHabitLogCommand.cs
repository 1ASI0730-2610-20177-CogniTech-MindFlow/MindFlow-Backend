namespace Mindflow_backend.Habits.Application.Commands.HabitLogs;

public record CreateHabitLogCommand(int HabitId, string HabitName, string Category, DateTime Date, DateTime? CompletedAt = null);
