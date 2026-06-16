using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Support.Application.Services;

namespace Mindflow_backend.Support.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SupportController(ISupportService supportService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst("user_id")!.Value);
    private string CurrentUserEmail => User.FindFirst(ClaimTypes.Email)!.Value;

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "El asunto y el mensaje son obligatorios." });

        if (request.Subject.Length > 255)
            return BadRequest(new { message = "El asunto no puede superar los 255 caracteres." });

        var ticket = await supportService.CreateTicketAsync(
            CurrentUserId,
            CurrentUserEmail,
            request.Subject.Trim(),
            request.Message.Trim());

        return StatusCode(201, new
        {
            ticket.Id,
            ticket_number = $"#{ticket.Id:D5}",
            ticket.Subject,
            ticket.Status,
            ticket.CreatedAt,
            message = "Tu ticket ha sido creado. Recibirás una confirmación por email."
        });
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var tickets = await supportService.GetUserTicketsAsync(CurrentUserId);

        return Ok(tickets.Select(t => new
        {
            t.Id,
            ticket_number = $"#{t.Id:D5}",
            t.Subject,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt
        }));
    }
}

public record CreateTicketRequest(string Subject, string Message);
