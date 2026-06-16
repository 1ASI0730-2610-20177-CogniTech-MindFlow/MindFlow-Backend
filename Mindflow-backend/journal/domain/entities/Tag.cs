namespace Mindflow_backend.Journal.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EntryTag> EntryTags { get; set; } = [];
}