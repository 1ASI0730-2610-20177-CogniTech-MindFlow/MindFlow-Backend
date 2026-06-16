using Mindflow_backend.Habits.Application.Commands.Habits;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class UpdateHabitCommandFromResourceAssembler
{
    public static UpdateHabitCommand ToCommandFromResource(int id, int userId, UpdateHabitResource resource)
    {
        var frequency = resource.Frequency switch
        {
            "daily" => HabitFrequency.Daily,
            "weekly" => HabitFrequency.Weekly,
            "monthly" => HabitFrequency.Monthly,
            _ => throw new ArgumentException($"Invalid frequency: {resource.Frequency}")
        };

        var status = resource.Status switch
        {
            "pending" => HabitStatus.Pending,
            "completed" => HabitStatus.Completed,
            "paused_by_ai" => HabitStatus.PausedByAi,
            _ => throw new ArgumentException($"Invalid status: {resource.Status}")
        };

        return new UpdateHabitCommand(id, userId, resource.Name, resource.Category, frequency, resource.Streak, status, resource.PausedByAi);
    }
}
