namespace Mindflow_backend.Analytics.Application.Dtos;

public class WordCloudDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Words { get; set; }
}