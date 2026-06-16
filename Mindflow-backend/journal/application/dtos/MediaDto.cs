namespace Mindflow_backend.Journal.Application.Dtos;

public class MediaDto
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
}