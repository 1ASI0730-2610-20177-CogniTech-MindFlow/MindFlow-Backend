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

        logger.LogInformation("Stripe webhook received: {EventType}, id={EventId}", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted((Session)stripeEvent.Data.Object, ct);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                var updated = (Stripe.Subscription)stripeEvent.Data.Object;
                if (updated.Status == "active")
                    await ActivateByStripeCustomerAsync(updated.CustomerId, updated.Id, ct);
                else if (updated.Status == "past_due")
                    await MarkPastDueByStripeCustomerAsync(updated.CustomerId, ct);
                else if (updated.Status == "canceled" || updated.Status == "unpaid")
                    await CancelByStripeCustomerAsync(updated.CustomerId, ct);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                var deleted = (Stripe.Subscription)stripeEvent.Data.Object;
                await CancelByStripeCustomerAsync(deleted.CustomerId, ct);
                break;

            case EventTypes.InvoicePaymentFailed:
                var invoice = (Invoice)stripeEvent.Data.Object;
                await MarkPastDueByStripeCustomerAsync(invoice.CustomerId, ct);
                break;

            case EventTypes.InvoicePaymentSucceeded:
                var paidInvoice = (Invoice)stripeEvent.Data.Object;
                await ReactivateByStripeCustomerAsync(paidInvoice.CustomerId, ct);
                break;

            default:
                logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleCheckoutSessionCompleted(Session session, CancellationToken ct)
    {
        logger.LogInformation(
            "Checkout session completed: sessionId={SessionId}, customerId={CustomerId}, subscriptionId={SubscriptionId}, paymentStatus={PaymentStatus}, metadataKeys={MetadataKeys}",
            session.Id, session.CustomerId, session.SubscriptionId, session.PaymentStatus,
            session.Metadata != null ? string.Join(",", session.Metadata.Keys) : "null");

        if (session.Metadata?.TryGetValue("user_id", out var userIdStr) == true
            && int.TryParse(userIdStr, out var userId))
        {
            await ActivateAsync(userId, session.CustomerId, session.SubscriptionId, ct);
        }
        else
        {
            logger.LogWarning(
                "Could not extract user_id from checkout session metadata. SessionId={SessionId}",
                session.Id);
        }
    }

    public async Task<SubscriptionDto> VerifySessionAsync(int userId, string sessionId, CancellationToken ct = default)
    {
        logger.LogInformation("VerifySession called: userId={UserId}, sessionId={SessionId}", userId, sessionId);

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new StripeException("session_id is required.");

        var client = new StripeClient(configuration["Stripe:SecretKey"]);
        var sessionService = new SessionService(client);
        var session = await sessionService.GetAsync(sessionId, cancellationToken: ct);

        logger.LogInformation(
            "Stripe session retrieved: paymentStatus={PaymentStatus}, customerId={CustomerId}, subscriptionId={SubscriptionId}, mode={Mode}, metadataKeys={Keys}",
            session.PaymentStatus, session.CustomerId, session.SubscriptionId, session.Mode,
            session.Metadata != null ? string.Join(",", session.Metadata.Keys) : "null");

        if (session.PaymentStatus != "paid")
        {
            logger.LogWarning("Session {SessionId} payment not completed: {Status}", sessionId, session.PaymentStatus);
            throw new StripeException($"Payment not completed. Status: {session.PaymentStatus}");
        }

        var metadataUserId = session.Metadata?.GetValueOrDefault("user_id");
        if (!string.Equals(metadataUserId, userId.ToString(), StringComparison.Ordinal))
        {
            logger.LogWarning("Session {SessionId} user_id mismatch: expected {Expected}, got {Got}",
                sessionId, userId, metadataUserId);
            throw new StripeException("Session does not belong to this user.");
        }

        await ActivateAsync(userId, session.CustomerId, session.SubscriptionId, ct);

        var result = await GetByUserIdAsync(userId, ct);
        logger.LogInformation("VerifySession result: userId={UserId}, plan={Plan}, isPremium={IsPremium}",
            userId, result.Plan, result.IsPremium);
        return result;
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
        logger.LogInformation(
            "ActivateAsync starting: userId={UserId}, customerId={CustomerId}, subscriptionId={SubscriptionId}",
            userId, customerId, subscriptionId);

        var sub = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (sub is null)
        {
            sub = new AppSubscription { UserId = userId };
            dbContext.Subscriptions.Add(sub);
        }
        sub.Activate(customerId, subscriptionId);

        try
        {
            await unitOfWork.CompleteAsync(ct);
        }
        catch (DbUpdateException) when (sub.Id == 0)
        {
            dbContext.Entry(sub).State = EntityState.Detached;
            sub = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
            if (sub is null) throw;
            sub.Activate(customerId, subscriptionId);
            await unitOfWork.CompleteAsync(ct);
        }

        logger.LogInformation(
            "Premium activated for user {UserId}: plan={Plan}, status={Status}",
            userId, sub.Plan, sub.Status);
    }

    public async Task CancelAsync(int userId, CancellationToken ct = default)
    {
        var sub = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (sub is null || sub.Status == "canceled")
            throw new StripeException("No active subscription found.");

        if (!string.IsNullOrEmpty(sub.StripeSubscriptionId))
        {
            var client = new StripeClient(configuration["Stripe:SecretKey"]);
            var service = new Stripe.SubscriptionService(client);
            await service.CancelAsync(sub.StripeSubscriptionId, cancellationToken: ct);
        }

        sub.Cancel();
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Subscription canceled by user {UserId}.", userId);
    }

    private async Task ActivateByStripeCustomerAsync(string customerId, string subscriptionId, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeCustomerId == customerId, ct);
        if (sub is null)
        {
            logger.LogWarning("No subscription found for Stripe customer {CustomerId} — skipping activation.", customerId);
            return;
        }
        sub.Activate(customerId, subscriptionId);
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Premium re-activated for user {UserId} via customer {CustomerId}.", sub.UserId, customerId);
    }

    private async Task ReactivateByStripeCustomerAsync(string customerId, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeCustomerId == customerId, ct);
        if (sub is null) return;
        sub.Plan = "premium";
        sub.Status = "active";
        await unitOfWork.CompleteAsync(ct);
        logger.LogInformation("Subscription reactivated via invoice.payment_succeeded for customer {CustomerId}.", customerId);
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
