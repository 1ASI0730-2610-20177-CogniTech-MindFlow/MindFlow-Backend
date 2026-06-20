using Cortex.Mediator.Commands;
using Mindflow_backend.Analytics.Application.Commands;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class CreateAnalyticsCacheCommandHandler(AppDbContext dbContext)
    : ICommandHandler<CreateAnalyticsCacheCommand, Result<AnalyticsCacheDto>>
{
    public async Task<Result<AnalyticsCacheDto>> Handle(CreateAnalyticsCacheCommand request, CancellationToken ct)
    {
        var cache = new AnalyticsCache
        {
            UserId = request.UserId,
            WeekStart = request.WeekStart,
            Score = request.Score,
            TrendPercentage = request.TrendPercentage,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AiInsight = request.AiInsight,
            AiInsightLocalized = request.AiInsightLocalized,
            Kpis = request.Kpis,
            FluctuationData = request.FluctuationData,
            TrendData = request.TrendData
        };

        dbContext.AnalyticsCaches.Add(cache);
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
            AiInsightLocalizedRaw = cache.AiInsightLocalized,
            Kpis = cache.Kpis,
            FluctuationData = cache.FluctuationData,
            TrendData = cache.TrendData
        });
    }
}