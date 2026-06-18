namespace Mindflow_backend.Chat.Application.Dtos;

public record CreateConversationRequest(string Content, string? Category);

public record SendMessageRequest(string Content);

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
}

public class ConversationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public string? LastMessage { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ConversationDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = [];
}

public class SendMessageResponseDto
{
    public ChatMessageDto UserMessage { get; set; } = null!;
    public ChatMessageDto AiMessage { get; set; } = null!;
}
