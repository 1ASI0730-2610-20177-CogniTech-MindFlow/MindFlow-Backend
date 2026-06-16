namespace Mindflow_backend.Notifications.Application.Dtos;

public class RegisterDeviceRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "web";
}
