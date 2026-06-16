namespace Mindflow_backend.iam.application.services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string resetToken);
}
