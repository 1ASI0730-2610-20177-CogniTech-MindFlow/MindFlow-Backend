using Cortex.Mediator;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;

namespace Mindflow_backend.Analytics.Application.Commands;

public class CreateAnalyticsCacheCommand : IRequest<Result<AnalyticsCacheDto>>
{
    public int UserId { get; set; }
    public DateOnly WeekStart { get; set; }
    public int Score { get; set; }
    public string TrendPercentage { get; set; } = "+0%";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? AiInsight { get; set; }
    public string? AiInsightLocalized { get; set; }
    public string? Kpis { get; set; }
    public string? FluctuationData { get; set; }
    public string? TrendData { get; set; }
}