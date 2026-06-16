using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Subscriptions.Domain.Model.Entities;

public class Subscription : IAuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Plan { get; set; } = "free";
    public string Status { get; set; } = "active";
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsPremium => Plan == "premium" && Status == "active";

    public void Activate(string stripeCustomerId, string stripeSubscriptionId)
    {
        Plan = "premium";
        Status = "active";
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
    }

    public void Cancel() => Status = "canceled";

    public void MarkPastDue() => Status = "past_due";
}
