using Mindflow_backend.Habits.Application.Queries.Habits;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.QueryServices;

public class HabitQueryService : IHabitQueryService
{
    private readonly IHabitRepository _habitRepository;

    public HabitQueryService(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async Task<Habit?> Handle(GetHabitByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _habitRepository.FindByIdAndUserIdAsync(query.Id, query.UserId, cancellationToken);
    }

    public async Task<IEnumerable<Habit>> Handle(GetAllHabitsByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _habitRepository.FindByUserIdAsync(query.UserId, cancellationToken);
    }
}
