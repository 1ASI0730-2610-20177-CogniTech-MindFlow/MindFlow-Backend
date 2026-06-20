using System.Text.Json;
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

    [JsonIgnore]
    public string? KpisRaw { get; set; }

    public object? Kpis => DeserializeJson(KpisRaw);

    [JsonIgnore]
    public string? FluctuationDataRaw { get; set; }

    public object? FluctuationData => DeserializeJson(FluctuationDataRaw);

    [JsonIgnore]
    public string? TrendDataRaw { get; set; }

    public object? TrendData => DeserializeJson(TrendDataRaw);

    private static object? DeserializeJson(string? json) =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<JsonElement>(json);
}
