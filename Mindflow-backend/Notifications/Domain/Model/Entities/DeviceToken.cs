namespace Mindflow_backend.Notifications.Domain.Model.Entities;

public class DeviceToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "web";
    public DateTimeOffset CreatedAt { get; set; }
}
