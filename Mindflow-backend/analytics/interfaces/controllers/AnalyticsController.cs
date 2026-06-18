using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Analytics.Application.Commands;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Application.Queries;
using Mindflow_backend.Analytics.Application.Services;
using System.Text.Json;

namespace Mindflow_backend.Analytics.Interfaces.Controllers;

[ApiController]
[Route("")]
[Authorize]
public sealed class AnalyticsController(
    IMediator mediator,
    AnalyticsComputationService computationService) : ControllerBase
{
    [HttpGet("analyticsCache")]
    public async Task<IActionResult> GetAnalyticsCache([FromQuery] DateOnly? weekStart)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var start = weekStart ?? GetCurrentWeekStart();

        var query = new GetAnalyticsCacheQuery { UserId = userId, WeekStart = start };
        var result = await mediator.QueryAsync(query);

        if (result.IsSuccess && result.Value!.Count == 0)
        {
            var cache = await computationService.ComputeAndSaveWeeklyAsync(userId, start);
            return Ok(new List<object>
            {
                new
                {
                    cache.Id,
                    cache.UserId,
                    cache.WeekStart,
                    cache.Score,
                    cache.TrendPercentage,
                    cache.StartDate,
                    cache.EndDate,
                    cache.AiInsight,
                    cache.AiInsightLocalized,
                    Kpis = cache.Kpis,
                    FluctuationData = cache.FluctuationData,
                    TrendData = cache.TrendData,
                    cache.CreatedAt,
                    cache.UpdatedAt
                }
            });
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPost("analyticsCache")]
    public async Task<IActionResult> CreateAnalyticsCache([FromBody] CreateAnalyticsCacheCommand command)
    {
        command.UserId = int.Parse(User.FindFirst("user_id")!.Value);
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPut("analyticsCache/{id}")]
    public async Task<IActionResult> UpdateAnalyticsCache(int id, [FromBody] UpdateAnalyticsCacheCommand command)
    {
        command.Id = id;
        command.UserId = int.Parse(User.FindFirst("user_id")!.Value);
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPost("analyticsCache/compute")]
    public async Task<IActionResult> ComputeWeeklyAnalytics([FromQuery] DateOnly? weekStart)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var start = weekStart ?? GetCurrentWeekStart();
        var cache = await computationService.ComputeAndSaveWeeklyAsync(userId, start);
        return Ok(new
        {
            cache.Id,
            cache.UserId,
            cache.WeekStart,
            cache.Score,
            cache.TrendPercentage,
            cache.StartDate,
            cache.EndDate,
            cache.AiInsight,
            cache.AiInsightLocalized,
            Kpis = Deserialize<List<KpiItemDto>>(cache.Kpis),
            FluctuationData = Deserialize<ChartDataDto>(cache.FluctuationData),
            TrendData = Deserialize<ChartDataDto>(cache.TrendData),
            CreatedAt = cache.CreatedAt,
            UpdatedAt = cache.UpdatedAt
        });
    }

    [HttpGet("wordCloud")]
    public async Task<IActionResult> GetWordCloud()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var query = new GetWordCloudQuery { UserId = userId };
        var result = await mediator.QueryAsync(query);

        if (result.IsSuccess && string.Equals(result.Value!.Words, "[]"))
        {
            var wordCloud = await computationService.ComputeAndSaveWordCloudAsync(userId);
            return Ok(new WordCloudDto
            {
                Id = wordCloud.Id,
                UserId = wordCloud.UserId,
                Words = wordCloud.Words
            });
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPost("wordCloud")]
    public async Task<IActionResult> CreateWordCloud([FromBody] CreateWordCloudCommand command)
    {
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpGet("moodCalendar")]
    public async Task<IActionResult> GetMoodCalendar([FromQuery] int? year, [FromQuery] int? month)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        var result = await computationService.ComputeMoodCalendarAsync(userId, y, m);
        return Ok(new { year = y, month = m, days = result });
    }

    [HttpPost("wordCloud/compute")]
    public async Task<IActionResult> ComputeWordCloud()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var wordCloud = await computationService.ComputeAndSaveWordCloudAsync(userId);
        return Ok(new
        {
            wordCloud.Id,
            wordCloud.UserId,
            Words = Deserialize<List<WordCloudItemDto>>(wordCloud.Words),
            CreatedAt = wordCloud.CreatedAt,
            UpdatedAt = wordCloud.UpdatedAt
        });
    }

    private static DateOnly GetCurrentWeekStart()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return today.AddDays(-diff);
    }

    private static T? Deserialize<T>(string? json) where T : class =>
        string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<T>(json);
}
