namespace Mindflow_backend.Notifications.Application.Services;

public interface INotificationService
{
    Task SendToAllAsync(string title, string body, CancellationToken ct = default);
    Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default);
}
