using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Chat.Domain.Entities;

public class Conversation : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Personal";
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
