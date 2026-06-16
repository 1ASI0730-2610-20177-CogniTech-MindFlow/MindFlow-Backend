namespace Mindflow_backend.Journal.Domain.Entities;

public class EntryTag
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int TagId { get; set; }

    public JournalEntry Entry { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}