using System.Text.Json.Serialization;

namespace Mindflow_backend.Analytics.Application.Dtos;

public class AnalyticsCacheDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly WeekStart { get; set; }
    public int Score { get; set; }
    public string TrendPercentage { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    public string? AiInsight { get; set; }

    public object? AiInsightLocalized => AiInsight != null || AiInsightLocalizedRaw != null
        ? new { en = AiInsight, es = AiInsightLocalizedRaw }
        : null;

    [JsonIgnore]
    public string? AiInsightLocalizedRaw { get; set; }

    public string? Kpis { get; set; }
    public string? FluctuationData { get; set; }
    public string? TrendData { get; set; }
}
