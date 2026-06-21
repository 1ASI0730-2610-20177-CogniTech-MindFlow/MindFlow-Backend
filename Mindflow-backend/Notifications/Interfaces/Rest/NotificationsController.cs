using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Notifications.Application.Dtos;
using Mindflow_backend.Notifications.Domain.Model.Entities;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Notifications.Interfaces.Rest;

[ApiController]
[Route("notifications")]
[Authorize]
public sealed class NotificationsController(
    AppDbContext dbContext,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Body,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(notifications);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

        if (notification is null)
            return NotFound();

        notification.IsRead = true;
        await unitOfWork.CompleteAsync(ct);
        return Ok(new { message = "Notificación marcada como leída." });
    }

    [HttpPost("register-device")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "El token del dispositivo es requerido." });

        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var existing = await dbContext.DeviceTokens
            .FirstOrDefaultAsync(dt => dt.Token == request.Token, ct);

        if (existing is not null)
        {
            existing.UserId = userId;
            existing.Platform = request.Platform;
        }
        else
        {
            dbContext.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = request.Token,
                Platform = request.Platform,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await unitOfWork.CompleteAsync(ct);
        return Ok(new { message = "Dispositivo registrado correctamente." });
    }

    [HttpDelete("unregister-device")]
    public async Task<IActionResult> UnregisterDevice([FromBody] UnregisterDeviceRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { error = "El token del dispositivo es requerido." });

        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var existing = await dbContext.DeviceTokens
            .FirstOrDefaultAsync(dt => dt.Token == request.Token && dt.UserId == userId, ct);

        if (existing is null) return NotFound();

        dbContext.DeviceTokens.Remove(existing);
        await unitOfWork.CompleteAsync(ct);
        return Ok(new { message = "Dispositivo eliminado correctamente." });
    }
}
