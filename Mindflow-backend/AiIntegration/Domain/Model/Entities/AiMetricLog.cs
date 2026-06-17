namespace Mindflow_backend.AiIntegration.Domain.Model.Entities;

public class AiMetricLog
{
    public int Id { get; set; }
    public string Operation { get; set; } = string.Empty;
    public int LatencyMs { get; set; }
    public bool Success { get; set; }
    public int PromptLength { get; set; }
    public int ResponseLength { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
