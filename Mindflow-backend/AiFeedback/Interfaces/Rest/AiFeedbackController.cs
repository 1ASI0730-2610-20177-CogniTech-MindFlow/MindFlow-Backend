using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.AiFeedback.Application.Services;

namespace Mindflow_backend.AiFeedback.Interfaces.Rest;

[ApiController]
[Route("api/v1/ai-feedback")]
[Authorize]
public class AiFeedbackController(IAiFeedbackService feedbackService) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst("user_id")!.Value);

    [HttpPost]
    public async Task<IActionResult> SubmitRating([FromBody] SubmitRatingRequest request)
    {
        try
        {
            var rating = await feedbackService.SubmitRatingAsync(
                CurrentUserId,
                request.ContentId,
                request.ContentType.ToLowerInvariant(),
                request.Rating,
                request.Comment);

            return Ok(new
            {
                rating.Id,
                rating.ContentId,
                rating.ContentType,
                rating.Rating,
                rating.Comment,
                rating.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRatings()
    {
        var ratings = await feedbackService.GetUserRatingsAsync(CurrentUserId);

        return Ok(ratings.Select(r => new
        {
            r.Id,
            r.ContentId,
            r.ContentType,
            r.Rating,
            r.Comment,
            r.CreatedAt
        }));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await feedbackService.GetSummaryAsync(CurrentUserId);
        return Ok(new
        {
            summary.TotalRatings,
            summary.AverageRating,
            summary.Distribution
        });
    }
}

public record SubmitRatingRequest(int ContentId, string ContentType, int Rating, string? Comment);