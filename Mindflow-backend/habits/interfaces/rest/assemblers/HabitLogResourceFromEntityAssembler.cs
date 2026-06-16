using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class HabitLogResourceFromEntityAssembler
{
    public static HabitLogResource ToResourceFromEntity(HabitCompletionLog entity)
    {
        return new HabitLogResource(
            entity.Id,
            entity.HabitId,
            entity.HabitName,
            entity.Category,
            entity.Date,
            entity.Completed,
            entity.CompletedAt,
            entity.CreatedAt
        );
    }

    public static IEnumerable<HabitLogResource> ToResourceListFromEntityList(IEnumerable<HabitCompletionLog> entities)
    {
        return entities.Select(ToResourceFromEntity);
    }
}
