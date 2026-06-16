using Mindflow_backend.Habits.Domain.Model.ValueObjects;

namespace Mindflow_backend.Habits.Application.Commands.Habits;

public record UpdateHabitCommand(int Id, int UserId, string Name, string Category, HabitFrequency Frequency, int Streak, HabitStatus Status, bool PausedByAi);
