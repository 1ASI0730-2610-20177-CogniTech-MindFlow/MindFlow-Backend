using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Journal.Domain.Entities;

public class Media : IAuditableEntity
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public JournalEntry Entry { get; set; } = null!;
}