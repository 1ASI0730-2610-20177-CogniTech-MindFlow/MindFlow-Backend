using Mindflow_backend.Habits.Application.Queries.HabitLogs;
using Mindflow_backend.Habits.Domain.Model.Entities;

namespace Mindflow_backend.Habits.Application.Internal.QueryServices;

public interface IHabitLogQueryService
{
    Task<HabitCompletionLog?> Handle(GetHabitLogByIdQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<HabitCompletionLog>> Handle(GetAllHabitLogsQuery query, CancellationToken cancellationToken = default);
}
