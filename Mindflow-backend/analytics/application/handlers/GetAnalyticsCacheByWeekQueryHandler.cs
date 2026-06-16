using Cortex.Mediator.Queries;
using Mindflow_backend.Analytics.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Application.Queries;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class GetAnalyticsCacheByWeekQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetAnalyticsCacheByWeekQuery, Result<AnalyticsCacheDto>>
{
    public async Task<Result<AnalyticsCacheDto>> Handle(GetAnalyticsCacheByWeekQuery request, CancellationToken ct)
    {
        var cache = await dbContext.AnalyticsCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.WeekStart == request.WeekStart, ct);

        if (cache is null)
            return Result<AnalyticsCacheDto>.Failure(
                AnalyticsError.AnalyticsCacheNotFound, "No analytics cache found for the specified week."); 

        return Result<AnalyticsCacheDto>.Success(MapToDto(cache));
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