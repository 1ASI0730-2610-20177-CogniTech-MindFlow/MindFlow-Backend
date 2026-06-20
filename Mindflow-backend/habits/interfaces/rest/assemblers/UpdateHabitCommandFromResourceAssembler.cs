using Mindflow_backend.Habits.Application.Commands.Habits;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class UpdateHabitCommandFromResourceAssembler
{
    public static UpdateHabitCommand ToCommandFromResource(int id, int userId, UpdateHabitResource resource)
    {
        if (!Enum.TryParse<HabitFrequency>(resource.Frequency, ignoreCase: true, out var frequency))
            throw new ArgumentException($"Invalid frequency: {resource.Frequency}");

        return new UpdateHabitCommand(id, userId, resource.Name, resource.Category, frequency);
    }
}
