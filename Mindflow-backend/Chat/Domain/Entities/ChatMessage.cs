using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Chat.Domain.Entities;

public class ChatMessage : IAuditableEntity
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
