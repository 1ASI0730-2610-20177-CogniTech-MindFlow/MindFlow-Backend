using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Support.Application.Services;
using Mindflow_backend.Support.Domain.Model.Entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Support.Infrastructure.Services;

public class SupportService(AppDbContext db, IConfiguration configuration, ILogger<SupportService> logger)
    : ISupportService
{
    public async Task<SupportTicket> CreateTicketAsync(int userId, string userEmail, string subject, string message)
    {
        var recent = await db.SupportTickets
            .AnyAsync(t => t.UserId == userId && t.Subject == subject
                && t.CreatedAt > DateTimeOffset.UtcNow.AddMinutes(-1));

        if (recent)
            throw new InvalidOperationException("Ya enviaste un ticket con este asunto. Espera un momento antes de intentar de nuevo.");

        var ticket = new SupportTicket
        {
            UserId = userId,
            UserEmail = userEmail,
            Subject = subject,
            Message = message,
            Status = "open"
        };

        db.SupportTickets.Add(ticket);
        await db.SaveChangesAsync();

        _ = SendConfirmationEmailAsync(userEmail, ticket.Id, subject);

        return ticket;
    }

    public async Task<IEnumerable<SupportTicket>> GetUserTicketsAsync(int userId)
    {
        return await db.SupportTickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    private async Task SendConfirmationEmailAsync(string toEmail, int ticketId, string subject)
    {
        var smtp = configuration.GetSection("Email");
        var fromAddress = smtp["From"];

        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(smtp["Host"]))
        {
            logger.LogWarning("Email not configured — skipping support ticket confirmation for ticket #{TicketId}", ticketId);
            return;
        }

        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, "MindFlow Soporte"),
                Subject = $"[Ticket #{ticketId:D5}] Hemos recibido tu solicitud — MindFlow",
                IsBodyHtml = true,
                Body = $"""
                        <h2 style="color:#4F46E5">¡Recibimos tu mensaje!</h2>
                        <p>Hola, hemos recibido tu ticket de soporte y lo atenderemos a la brevedad.</p>
                        <table style="border-collapse:collapse;width:100%;margin-top:16px">
                          <tr>
                            <td style="padding:8px;background:#F5F3FF;font-weight:600;width:140px">N.° de ticket</td>
                            <td style="padding:8px;border-bottom:1px solid #E5E7EB">#{ticketId:D5}</td>
                          </tr>
                          <tr>
                            <td style="padding:8px;background:#F5F3FF;font-weight:600">Asunto</td>
                            <td style="padding:8px;border-bottom:1px solid #E5E7EB">{System.Web.HttpUtility.HtmlEncode(subject)}</td>
                          </tr>
                        </table>
                        <p style="color:#888;font-size:12px;margin-top:24px">
                            Responderemos a este correo. Por favor, no replies a correos automáticos.<br/>
                            — Equipo MindFlow
                        </p>
                        """
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"] ?? "587"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"])
            };

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send confirmation email for ticket #{TicketId}", ticketId);
        }
    }
}
