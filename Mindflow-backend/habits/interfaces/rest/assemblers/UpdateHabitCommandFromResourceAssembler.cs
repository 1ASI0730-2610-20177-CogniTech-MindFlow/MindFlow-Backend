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

        var statusNormalized = resource.Status?.Replace("_", "") ?? "Pending";
        if (!Enum.TryParse<HabitStatus>(statusNormalized, ignoreCase: true, out var status))
            throw new ArgumentException($"Invalid status: {resource.Status}");

        return new UpdateHabitCommand(id, userId, resource.Name, resource.Category, frequency, resource.Streak, status, resource.PausedByAi);
    }
}
