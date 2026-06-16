using Mindflow_backend.Habits.Application.Queries.HabitLogs;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.QueryServices;

public class HabitLogQueryService : IHabitLogQueryService
{
    private readonly IHabitCompletionLogRepository _logRepository;

    public HabitLogQueryService(IHabitCompletionLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<HabitCompletionLog?> Handle(GetHabitLogByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _logRepository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<HabitCompletionLog>> Handle(GetAllHabitLogsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.HabitId.HasValue)
            return await _logRepository.FindByHabitIdAsync(query.HabitId.Value, cancellationToken);

        if (query.UserId.HasValue)
            return await _logRepository.FindByUserIdAsync(query.UserId.Value, cancellationToken);

        return await _logRepository.ListAsync(cancellationToken);
    }
}
