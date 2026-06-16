namespace Mindflow_backend.Analytics.Application.Dtos;

public class KpiItemDto
{
    public string? LabelKey { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? ValueKey { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? UnitKey { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
}