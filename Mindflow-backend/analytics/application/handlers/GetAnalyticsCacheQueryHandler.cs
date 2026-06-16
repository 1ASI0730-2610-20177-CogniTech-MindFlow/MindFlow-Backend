using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Application.Queries;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class GetAnalyticsCacheQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetAnalyticsCacheQuery, Result<List<AnalyticsCacheDto>>>
{
    public async Task<Result<List<AnalyticsCacheDto>>> Handle(GetAnalyticsCacheQuery request, CancellationToken ct)
    {
        var caches = await dbContext.AnalyticsCaches
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.WeekStart)
            .ToListAsync(ct);

        var dtos = caches.Select(MapToDto).ToList();
        return Result<List<AnalyticsCacheDto>>.Success(dtos);
    }

    private static AnalyticsCacheDto MapToDto(AnalyticsCache cache) => new()
    {
        Id = cache.Id,
        UserId = cache.UserId,
        WeekStart = cache.WeekStart,
        Score = cache.Score,
        TrendPercentage = cache.TrendPercentage,
        StartDate = cache.StartDate,
        EndDate = cache.EndDate,
        AiInsight = cache.AiInsight,
        AiInsightLocalized = cache.AiInsightLocalized,
        Kpis = cache.Kpis,
        FluctuationData = cache.FluctuationData,
        TrendData = cache.TrendData
    };
}