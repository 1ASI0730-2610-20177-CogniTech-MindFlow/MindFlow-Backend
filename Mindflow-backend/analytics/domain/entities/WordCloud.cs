using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Analytics.Domain.Entities;

public class WordCloud : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Words { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}