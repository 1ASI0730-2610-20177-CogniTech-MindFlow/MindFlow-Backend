using Cortex.Mediator.Commands;
using Mindflow_backend.Analytics.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Commands;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class UpdateAnalyticsCacheCommandHandler(AppDbContext dbContext)
    : ICommandHandler<UpdateAnalyticsCacheCommand, Result<AnalyticsCacheDto>>
{
    public async Task<Result<AnalyticsCacheDto>> Handle(UpdateAnalyticsCacheCommand request, CancellationToken ct)
    {
        var cache = await dbContext.AnalyticsCaches
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId, ct);

        if (cache is null)
            return Result<AnalyticsCacheDto>.Failure(
                AnalyticsError.AnalyticsCacheNotFound, "Analytics cache not found.");

        cache.Score = request.Score;
        cache.TrendPercentage = request.TrendPercentage;
        cache.StartDate = request.StartDate;
        cache.EndDate = request.EndDate;
        cache.AiInsight = request.AiInsight;
        cache.AiInsightLocalized = request.AiInsightLocalized;
        cache.Kpis = request.Kpis;
        cache.FluctuationData = request.FluctuationData;
        cache.TrendData = request.TrendData;

        await dbContext.SaveChangesAsync(ct);

        return Result<AnalyticsCacheDto>.Success(new AnalyticsCacheDto
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
        });
    }
}