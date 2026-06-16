using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class HabitResourceFromEntityAssembler
{
    public static HabitResource ToResourceFromEntity(Habit entity)
    {
        return new HabitResource(
            entity.Id,
            entity.UserId,
            entity.Name,
            entity.Category,
            entity.Frequency.ToString().ToLower(),
            entity.Streak,
            entity.Status.ToString().ToLower(),
            entity.PausedByAi,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt
        );
    }

    public static IEnumerable<HabitResource> ToResourceListFromEntityList(IEnumerable<Habit> entities)
    {
        return entities.Select(ToResourceFromEntity);
    }
}
