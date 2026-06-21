using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Notifications.Domain.Model.Entities;

public class Notification : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
