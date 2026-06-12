using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class UpdateHabitLogCommandFromResourceAssembler
{
    public static UpdateHabitLogCommand ToCommandFromResource(int id, UpdateHabitLogResource resource)
    {
        return new UpdateHabitLogCommand(
            id,
            resource.HabitName,
            resource.Category,
            resource.Completed,
            resource.CompletedAt
        );
    }
}
