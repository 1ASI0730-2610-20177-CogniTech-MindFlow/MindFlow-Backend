using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;

namespace Mindflow_backend.Journal.Interfaces.Controllers;

[ApiController]
[Route("journal")]
[Authorize]
public sealed class JournalController(IMediator mediator) : ControllerBase
{
    [HttpGet("entries")]
    public async Task<IActionResult> GetEntries(
    [FromQuery(Name = "_sort")] string? sort,
    [FromQuery(Name = "_order")] string? order,
    [FromQuery(Name = "_limit")] int? limit,
    [FromQuery(Name = "q")] string? q)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var query = new GetJournalEntriesQuery
        {
            UserId = userId,
            Sort = sort,
            Order = order,
            Limit = limit,
            Q = q
        };
        var result = await mediator.QueryAsync(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpGet("entries/{id}")]
    public async Task<IActionResult> GetEntryById(int id)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var query = new GetJournalEntryByIdQuery { Id = id };
        var result = await mediator.QueryAsync(query);
        if (!result.IsSuccess)
            return NotFound(result.Message);
        if (result.Value!.UserId != userId)
            return NotFound();
        return Ok(result.Value);
    }

    [HttpPost("entries")]
    public async Task<IActionResult> CreateEntry([FromBody] CreateJournalEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new CreateJournalEntryCommand
        {
            UserId = userId,
            Date = request.Date,
            Title = request.Title,
            Content = request.Content,
            Sentiment = request.Sentiment,
            Category = request.Category
        };
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPut("entries/{id}")]
    public async Task<IActionResult> UpdateEntry(int id, [FromBody] UpdateJournalEntryRequest request)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var entryQuery = new GetJournalEntryByIdQuery { Id = id };
        var entryResult = await mediator.QueryAsync(entryQuery);
        if (!entryResult.IsSuccess || entryResult.Value!.UserId != userId)
            return NotFound();

        var command = new UpdateJournalEntryCommand
        {
            Id = id,
            Title = request.Title,
            Content = request.Content,
            Sentiment = request.Sentiment,
            Category = request.Category
        };
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpDelete("entries/{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var entryQuery = new GetJournalEntryByIdQuery { Id = id };
        var entryResult = await mediator.QueryAsync(entryQuery);
        if (!entryResult.IsSuccess || entryResult.Value!.UserId != userId)
            return NotFound();

        var command = new DeleteJournalEntryCommand { Id = id };
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Message);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var query = new GetTagsQuery { UserId = userId };
        var result = await mediator.QueryAsync(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpGet("entry-tags")]
    public async Task<IActionResult> GetEntryTags([FromQuery] int? entryId)
    {
        var query = new GetEntryTagsQuery { EntryId = entryId };
        var result = await mediator.QueryAsync(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPost("entry-tags")]
    public async Task<IActionResult> CreateEntryTag([FromBody] CreateEntryTagCommand command)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var entryQuery = new GetJournalEntryByIdQuery { Id = command.EntryId };
        var entryResult = await mediator.QueryAsync(entryQuery);
        if (!entryResult.IsSuccess || entryResult.Value!.UserId != userId)
            return NotFound();

        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpDelete("entry-tags/{id}")]
    public async Task<IActionResult> DeleteEntryTag(int id)
    {
        var command = new DeleteEntryTagCommand { Id = id };
        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Message);
    }

    [HttpGet("media")]
    public async Task<IActionResult> GetMedia([FromQuery] int? entryId)
    {
        var query = new GetMediaQuery { EntryId = entryId };
        var result = await mediator.QueryAsync(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }

    [HttpPost("media")]
    public async Task<IActionResult> CreateMedia([FromBody] CreateMediaCommand command)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);

        var entryQuery = new GetJournalEntryByIdQuery { Id = command.EntryId };
        var entryResult = await mediator.QueryAsync(entryQuery);
        if (!entryResult.IsSuccess || entryResult.Value!.UserId != userId)
            return NotFound();

        var result = await mediator.SendAsync(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
    }
}
