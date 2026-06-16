namespace Mindflow_backend.Subscriptions.Application.Dtos;

public class SubscriptionDto
{
    public int UserId { get; set; }
    public string Plan { get; set; } = "free";
    public string Status { get; set; } = "active";
    public bool IsPremium { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class CheckoutSessionDto
{
    public string CheckoutUrl { get; set; } = string.Empty;
}
