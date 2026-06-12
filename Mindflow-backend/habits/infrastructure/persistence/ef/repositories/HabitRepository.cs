using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Repositories;

public class HabitRepository : BaseRepository<Habit>, IHabitRepository
{
    public HabitRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Habit>> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Habit>()
            .Where(h => h.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Habit?> FindByIdAndUserIdAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Habit>()
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId, cancellationToken);
    }
}
