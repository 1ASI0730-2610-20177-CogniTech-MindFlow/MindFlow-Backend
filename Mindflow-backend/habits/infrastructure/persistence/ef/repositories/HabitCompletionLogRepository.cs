using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Repositories;

public class HabitCompletionLogRepository : BaseRepository<HabitCompletionLog>, IHabitCompletionLogRepository
{
    public HabitCompletionLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HabitCompletionLog>> FindByHabitIdAsync(int habitId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<HabitCompletionLog>()
            .Where(l => l.HabitId == habitId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HabitCompletionLog>> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var habitIds = await Context.Set<Habit>()
            .Where(h => h.UserId == userId)
            .Select(h => h.Id)
            .ToListAsync(cancellationToken);

        return await Context.Set<HabitCompletionLog>()
            .Where(l => habitIds.Contains(l.HabitId))
            .ToListAsync(cancellationToken);
    }
}
