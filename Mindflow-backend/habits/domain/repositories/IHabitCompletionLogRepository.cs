using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Habits.Domain.Repositories;

public interface IHabitCompletionLogRepository : IBaseRepository<HabitCompletionLog>
{
    Task<IEnumerable<HabitCompletionLog>> FindByHabitIdAsync(int habitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<HabitCompletionLog>> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
