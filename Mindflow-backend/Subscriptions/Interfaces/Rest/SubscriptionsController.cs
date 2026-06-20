using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Subscriptions.Application.Services;
using Stripe;

namespace Mindflow_backend.Subscriptions.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var email  = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        try
        {
            var result = await subscriptionService.CreateCheckoutSessionAsync(userId, email, ct);
            return Ok(result);
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var result = await subscriptionService.GetByUserIdAsync(userId, ct);
        return Ok(result);
    }

    [HttpPost("verify-session")]
    [Authorize]
    public async Task<IActionResult> VerifySession([FromQuery(Name = "session_id")] string sessionId, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        try
        {
            var result = await subscriptionService.VerifySessionAsync(userId, sessionId, ct);
            return Ok(result);
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        try
        {
            await subscriptionService.CancelAsync(userId, ct);
            return Ok(new { message = "Subscription canceled successfully." });
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [RequestSizeLimit(65_536)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var payload   = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;

        try
        {
            await subscriptionService.HandleWebhookAsync(payload, signature, ct);
        }
        catch (StripeException)
        {
            return BadRequest();
        }

        return Ok();
    }
}
