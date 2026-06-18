using Mindflow_backend.Chat.Application.Dtos;

namespace Mindflow_backend.Chat.Application.Services;

public interface IChatService
{
    Task<ConversationDetailDto> CreateConversationAsync(int userId, string content, string? category, CancellationToken ct = default);
    Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId, CancellationToken ct = default);
    Task DeleteConversationAsync(int userId, int conversationId, CancellationToken ct = default);
    Task<SendMessageResponseDto> SendMessageAsync(int userId, int conversationId, string content, CancellationToken ct = default);
    Task<ConversationDetailDto> GetConversationMessagesAsync(int userId, int conversationId, CancellationToken ct = default);
}
