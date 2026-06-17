using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.AiFeedback.Domain.Model.Entities;

public class AiFeedbackRating : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContentId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}