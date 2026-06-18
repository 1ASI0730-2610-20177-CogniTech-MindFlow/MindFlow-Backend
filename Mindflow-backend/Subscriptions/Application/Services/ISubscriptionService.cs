using Mindflow_backend.Subscriptions.Application.Dtos;

namespace Mindflow_backend.Subscriptions.Application.Services;

public interface ISubscriptionService
{
    Task<CheckoutSessionDto> CreateCheckoutSessionAsync(int userId, string userEmail, CancellationToken ct = default);
    Task HandleWebhookAsync(string payload, string stripeSignature, CancellationToken ct = default);
    Task<SubscriptionDto> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<SubscriptionDto> VerifySessionAsync(int userId, string sessionId, CancellationToken ct = default);
    Task CancelAsync(int userId, CancellationToken ct = default);
}
