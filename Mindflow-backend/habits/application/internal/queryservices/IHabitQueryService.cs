using Mindflow_backend.Habits.Application.Queries.Habits;
using Mindflow_backend.Habits.Domain.Model.Aggregates;

namespace Mindflow_backend.Habits.Application.Internal.QueryServices;

public interface IHabitQueryService
{
    Task<Habit?> Handle(GetHabitByIdQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<Habit>> Handle(GetAllHabitsByUserIdQuery query, CancellationToken cancellationToken = default);
}
