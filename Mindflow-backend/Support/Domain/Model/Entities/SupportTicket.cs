using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Support.Domain.Model.Entities;

public class SupportTicket : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void MarkInProgress() => Status = "in_progress";
    public void Close() => Status = "closed";
}
