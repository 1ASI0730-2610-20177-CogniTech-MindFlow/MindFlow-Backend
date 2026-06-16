using Mindflow_backend.Habits.Domain.Model.ValueObjects;

namespace Mindflow_backend.Habits.Application.Commands.Habits;

public record CreateHabitCommand(int UserId, string Name, string Category, HabitFrequency Frequency);
