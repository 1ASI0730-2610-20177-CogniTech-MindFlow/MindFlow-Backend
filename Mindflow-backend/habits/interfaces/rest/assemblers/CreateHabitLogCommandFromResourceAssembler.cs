using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class CreateHabitLogCommandFromResourceAssembler
{
    public static CreateHabitLogCommand ToCommandFromResource(CreateHabitLogResource resource)
    {
        return new CreateHabitLogCommand(
            resource.HabitId,
            resource.HabitName,
            resource.Category,
            resource.Date,
            resource.CompletedAt
        );
    }
}
