using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.Chat.Application.Dtos;
using Mindflow_backend.Chat.Application.Services;
using Mindflow_backend.Chat.Domain.Entities;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Chat.Infrastructure.Services;

public class ChatService(
    AppDbContext db,
    IUnitOfWork unitOfWork,
    IAiService aiService,
    ILogger<ChatService> logger) : IChatService
{
    private const int MaxHistoryMessages = 10;
    private const string FallbackResponse = "Lo siento, no pude procesar tu mensaje en este momento. Por favor, intenta de nuevo.";

    public async Task<ConversationDetailDto> CreateConversationAsync(
        int userId, string content, string? category, CancellationToken ct = default)
    {
        var title = content.Length <= 50 ? content : content[..50] + "...";

        var conversation = new Conversation
        {
            UserId = userId,
            Title = title,
            Category = category ?? "Personal"
        };
        db.Conversations.Add(conversation);
        await unitOfWork.CompleteAsync(ct);

        var userMessage = new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = content
        };
        db.ChatMessages.Add(userMessage);
        await unitOfWork.CompleteAsync(ct);

        var aiResponseText = await aiService.GenerateChatResponseAsync([("user", content)]);
        if (string.IsNullOrEmpty(aiResponseText))
            aiResponseText = FallbackResponse;

        var aiMessage = new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = aiResponseText
        };
        db.ChatMessages.Add(aiMessage);
        await unitOfWork.CompleteAsync(ct);

        logger.LogInformation("Conversation {ConversationId} created for user {UserId}.", conversation.Id, userId);

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Category = conversation.Category,
            CreatedAt = conversation.CreatedAt,
            Messages = [ToDto(userMessage), ToDto(aiMessage)]
        };
    }

    public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(
        int userId, CancellationToken ct = default)
    {
        var conversations = await db.Conversations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Category,
                MessageCount = c.Messages.Count,
                LastMsg = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync(ct);

        return conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            Category = c.Category,
            MessageCount = c.MessageCount,
            LastMessage = c.LastMsg != null && c.LastMsg.Length > 100
                ? c.LastMsg[..100] + "..."
                : c.LastMsg,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }

    public async Task DeleteConversationAsync(int userId, int conversationId, CancellationToken ct = default)
    {
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, ct);

        if (conversation is null)
            throw new KeyNotFoundException("Conversación no encontrada.");

        db.Conversations.Remove(conversation);
        await unitOfWork.CompleteAsync(ct);
    }

    public async Task<SendMessageResponseDto> SendMessageAsync(
        int userId, int conversationId, string content, CancellationToken ct = default)
    {
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, ct);

        if (conversation is null)
            throw new KeyNotFoundException("Conversación no encontrada.");

        var userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = content
        };
        db.ChatMessages.Add(userMessage);
        await unitOfWork.CompleteAsync(ct);

        var recentMessages = await db.ChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MaxHistoryMessages)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new { m.Role, m.Content })
            .ToListAsync(ct);

        var history = recentMessages.Select(m => (m.Role, m.Content)).ToList();

        var aiResponseText = await aiService.GenerateChatResponseAsync(history);
        if (string.IsNullOrEmpty(aiResponseText))
            aiResponseText = FallbackResponse;

        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = aiResponseText
        };
        db.ChatMessages.Add(aiMessage);
        await unitOfWork.CompleteAsync(ct);

        return new SendMessageResponseDto
        {
            UserMessage = ToDto(userMessage),
            AiMessage = ToDto(aiMessage)
        };
    }

    public async Task<ConversationDetailDto> GetConversationMessagesAsync(
        int userId, int conversationId, CancellationToken ct = default)
    {
        var conversation = await db.Conversations
            .AsNoTracking()
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId, ct);

        if (conversation is null)
            throw new KeyNotFoundException("Conversación no encontrada.");

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Category = conversation.Category,
            CreatedAt = conversation.CreatedAt,
            Messages = conversation.Messages.Select(ToDto).ToList()
        };
    }

    private static ChatMessageDto ToDto(ChatMessage m) => new()
    {
        Id = m.Id,
        Role = m.Role,
        Content = m.Content,
        CreatedAt = m.CreatedAt
    };
}
