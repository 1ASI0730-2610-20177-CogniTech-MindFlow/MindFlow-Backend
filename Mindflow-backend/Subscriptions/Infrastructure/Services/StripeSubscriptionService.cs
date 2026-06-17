using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Subscriptions.Application.Dtos;
using Mindflow_backend.Subscriptions.Application.Services;
using Mindflow_backend.Subscriptions.Domain.Model.Entities;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Stripe;
using Stripe.Checkout;
using AppSubscription = Mindflow_backend.Subscriptions.Domain.Model.Entities.Subscription;

namespace Mindflow_backend.Subscriptions.Infrastructure.Services;

public class StripeSubscriptionService(
    AppDbContext dbContext,
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ILogger<StripeSubscriptionService> logger) : ISubscriptionService
{
    public async Task<CheckoutSessionDto> CreateCheckoutSessionAsync(
        int userId, string userEmail, CancellationToken ct = default)
    {
        var client = new StripeClient(configuration["Stripe:SecretKey"]);
        var sessionService = new SessionService(client);

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            CustomerEmail = userEmail,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = configuration["Stripe:PremiumPriceId"],
                    Quantity = 1
                }
            ],
            SuccessUrl = $"{GetPrimaryFrontendUrl()}/subscription/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{GetPrimaryFrontendUrl()}/subscription/cancel",
            Metadata   = new Dictionary<string, string> { ["user_id"] = userId.ToString() }
        };

        var session = await sessionService.CreateAsync(options, cancellationToken: ct);
        return new CheckoutSessionDto { CheckoutUrl = session.Url };
    }

    public async Task HandleWebhookAsync(string payload, string stripeSignature, CancellationToken ct = default)
    {
        var webhookSecret = configuration["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, webhookSecret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            throw;
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                var session = (Session)stripeEvent.Data.Object;
                if (session.Metadata?.TryGetValue("user_id", out var userIdStr) == true
                    && int.TryParse(userIdStr, out var userId))
                {
                    await ActivateAsync(userId, session.CustomerId, session.SubscriptionId, ct);
                }
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                var deleted = (Stripe.Subscription)stripeEvent.Data.Object;
                await CancelByStripeCustomerAsync(deleted.CustomerId, ct);
                break;

            case EventTypes.InvoicePaymentFailed:
                var invoice = (Invoice)stripeEvent.Data.Object;
                await MarkPastDueByStripeCustomerAsync(invoice.CustomerId, ct);
                break;
        }
    }

    public async Task<SubscriptionDto> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        var sub = await dbContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (sub is null)
            return new SubscriptionDto { UserId = userId, Plan = "free", Status = "active", IsPremium = false };

        return new SubscriptionDto
        {
            UserId    = sub.UserId,
            Plan      = sub.Plan,
            Status    = sub.Status,
            IsPremium = sub.IsPremium,
            ExpiresAt = sub.ExpiresAt
        };
    }

    private async Task ActivateAsync(int userId, string customerId, string subscriptionId, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (sub is null)
        {
            sub = new AppSubscription { UserId = userId };
            dbContext.Subscriptions.Add(sub);
        }
        sub.Activate(customerId, subscriptionId);
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Premium activated for user {UserId}.", userId);
    }

    private async Task CancelByStripeCustomerAsync(string customerId, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeCustomerId == customerId, ct);
        if (sub is null) return;
        sub.Cancel();
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Subscription canceled for customer {CustomerId}.", customerId);
    }

    private string GetPrimaryFrontendUrl() =>
        configuration["FrontendUrl"]?.Split(',', StringSplitOptions.TrimEntries).FirstOrDefault()
        ?? "http://localhost:5173";

    private async Task MarkPastDueByStripeCustomerAsync(string customerId, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeCustomerId == customerId, ct);
        if (sub is null) return;
        sub.MarkPastDue();
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Subscription past_due for customer {CustomerId}.", customerId);
    }
}
