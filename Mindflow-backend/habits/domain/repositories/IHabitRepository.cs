using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Habits.Domain.Repositories;

public interface IHabitRepository : IBaseRepository<Habit>
{
    Task<IEnumerable<Habit>> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Habit?> FindByIdAndUserIdAsync(int id, int userId, CancellationToken cancellationToken = default);
}
