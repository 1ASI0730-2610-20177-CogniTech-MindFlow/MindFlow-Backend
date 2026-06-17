using System.Net;
using System.Net.Mail;
using Mindflow_backend.iam.application.services;

namespace Mindflow_backend.iam.infrastructure.services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendPasswordResetAsync(string toEmail, string resetToken)
    {
        var smtp = configuration.GetSection("Email");
        var frontendUrl = configuration["FrontendUrl"]?.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault()
                         ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

        var message = new MailMessage
        {
            From = new MailAddress(smtp["From"]!, "MindFlow"),
            Subject = "Recuperación de contraseña — MindFlow",
            IsBodyHtml = true,
            Body = $"""
                    <h2>Recupera tu contraseña</h2>
                    <p>Haz clic en el siguiente enlace para restablecer tu contraseña.
                    El enlace expira en <strong>15 minutos</strong>.</p>
                    <a href="{resetLink}" style="background:#6366f1;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;display:inline-block">
                        Restablecer contraseña
                    </a>
                    <p style="color:#888;font-size:12px;margin-top:24px">
                        Si no solicitaste esto, ignora este correo.
                    </p>
                    """
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]!))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtp["Username"], smtp["Password"])
        };

        await client.SendMailAsync(message);
    }
}
