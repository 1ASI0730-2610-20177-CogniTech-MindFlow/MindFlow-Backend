namespace Mindflow_backend.Journal.Application.Dtos;

public class JournalEntryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "neutral";
    public string Category { get; set; } = "Sin categoría";
    public bool HasPreview { get; set; }
    public string? AiResponse { get; set; }
    public List<TagDto> Tags { get; set; } = [];
    public List<MediaDto> Media { get; set; } = [];
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}