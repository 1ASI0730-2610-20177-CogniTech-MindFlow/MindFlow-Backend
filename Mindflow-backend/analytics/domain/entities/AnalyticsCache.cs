using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Analytics.Domain.Entities;

public class AnalyticsCache : IAuditableEntity
{
    public int Id { get; set; }
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
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}