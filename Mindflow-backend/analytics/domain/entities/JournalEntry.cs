using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Analytics.Domain.Entities;

public class JournalEntry : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "neutral";
    public string Category { get; set; } = "Sin categoría";
    public bool HasPreview { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
