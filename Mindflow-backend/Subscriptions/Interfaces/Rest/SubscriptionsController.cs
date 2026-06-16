using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Subscriptions.Application.Services;
using Stripe;

namespace Mindflow_backend.Subscriptions.Interfaces.Rest;

[ApiController]
[Route("subscriptions")]
public sealed class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var email  = User.FindFirst("email")?.Value ?? string.Empty;

        var result = await subscriptionService.CreateCheckoutSessionAsync(userId, email, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var result = await subscriptionService.GetByUserIdAsync(userId, ct);
        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var payload   = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        try
        {
            await subscriptionService.HandleWebhookAsync(payload, signature, ct);
            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
    }
}
