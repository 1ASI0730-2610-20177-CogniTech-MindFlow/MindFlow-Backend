using System.Text.Json.Serialization;

namespace Mindflow_backend.Analytics.Application.Dtos;

public class ChartDataDto
{
    [JsonPropertyName("labels_keys")]
    public string[]? LabelsKeys { get; set; }
    public string[] Labels { get; set; } = [];
    [JsonPropertyName("dataset_label")]
    public string DatasetLabel { get; set; } = string.Empty;
    public ChartDatasetDto[] Datasets { get; set; } = [];
}

public class ChartDatasetDto
{
    [JsonPropertyName("label_key")]
    public string? LabelKey { get; set; }
    public string Label { get; set; } = string.Empty;
    public double[] Data { get; set; } = [];
    public string? BackgroundColor { get; set; }
    public string? HoverBackgroundColor { get; set; }
    public int? BorderRadius { get; set; }
    public bool? BorderSkipped { get; set; }
    public double? BarPercentage { get; set; }
    public double? CategoryPercentage { get; set; }
    public bool? Fill { get; set; }
    public string? BorderColor { get; set; }
    public double? Tension { get; set; }
    public string? PointBackgroundColor { get; set; }
    public string? PointBorderColor { get; set; }
    public int? PointBorderWidth { get; set; }
    public int? PointRadius { get; set; }
}