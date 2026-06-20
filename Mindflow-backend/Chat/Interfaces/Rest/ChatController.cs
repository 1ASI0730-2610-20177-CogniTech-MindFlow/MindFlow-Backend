using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Chat.Application.Dtos;
using Mindflow_backend.Chat.Application.Services;

namespace Mindflow_backend.Chat.Interfaces.Rest;

[ApiController]
[Route("chat")]
[Authorize]
public class ChatController(IChatService chatService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst("user_id")!.Value);

    private const int MaxContentLength = 5000;

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { message = "El contenido del mensaje es obligatorio." });

        if (request.Content.Length > MaxContentLength)
            return BadRequest(new { message = $"El mensaje no puede superar los {MaxContentLength} caracteres." });

        var result = await chatService.CreateConversationAsync(
            CurrentUserId, request.Content.Trim(), request.Category?.Trim(), ct);

        return StatusCode(201, result);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var conversations = await chatService.GetUserConversationsAsync(CurrentUserId, ct);
        return Ok(conversations);
    }

    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(int id, CancellationToken ct)
    {
        try
        {
            await chatService.DeleteConversationAsync(CurrentUserId, id, ct);
            return Ok(new { message = "Conversación eliminada exitosamente." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Conversación no encontrada." });
        }
    }

    [HttpPost("conversations/{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { message = "El contenido del mensaje es obligatorio." });

        if (request.Content.Length > MaxContentLength)
            return BadRequest(new { message = $"El mensaje no puede superar los {MaxContentLength} caracteres." });

        try
        {
            var result = await chatService.SendMessageAsync(
                CurrentUserId, id, request.Content.Trim(), ct);

            return StatusCode(201, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Conversación no encontrada." });
        }
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        try
        {
            var result = await chatService.GetConversationMessagesAsync(CurrentUserId, id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Conversación no encontrada." });
        }
    }
}
