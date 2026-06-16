namespace Mindflow_backend.Habits.Application.Commands.HabitLogs;

public record UpdateHabitLogCommand(int Id, string HabitName, string Category, bool Completed, DateTime? CompletedAt);
