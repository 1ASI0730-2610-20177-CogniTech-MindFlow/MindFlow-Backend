using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Application.Internal.CommandServices;
using Mindflow_backend.Habits.Application.Internal.QueryServices;
using Mindflow_backend.Habits.Application.Queries.HabitLogs;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Habits.Interfaces.Rest.Assemblers;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.HabitLogs;
using Mindflow_backend.Shared.Interfaces.Rest.ProblemDetails;

namespace Mindflow_backend.Habits.Interfaces.Rest.Controllers;

[ApiController]
[Route("habit-logs")]
[Authorize]
public class HabitLogsController : ControllerBase
{
    private readonly IHabitLogCommandService _habitLogCommandService;
    private readonly IHabitLogQueryService _habitLogQueryService;
    private readonly IHabitRepository _habitRepository;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public HabitLogsController(
        IHabitLogCommandService habitLogCommandService,
        IHabitLogQueryService habitLogQueryService,
        IHabitRepository habitRepository,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _habitLogCommandService = habitLogCommandService;
        _habitLogQueryService = habitLogQueryService;
        _habitRepository = habitRepository;
        _problemDetailsFactory = problemDetailsFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllHabitLogs([FromQuery] int? user_id, [FromQuery] int? habit_id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (user_id.HasValue && user_id.Value != userId)
            return _problemDetailsFactory.CreateProblemDetails(this, 403, (Enum?)null, "User ID mismatch.");

        if (habit_id.HasValue)
        {
            var habit = await _habitRepository.FindByIdAndUserIdAsync(habit_id.Value, userId, cancellationToken);
            if (habit is null)
                return NotFound(new { message = "Habit not found." });
        }

        var query = new GetAllHabitLogsQuery(userId, habit_id);
        var logs = await _habitLogQueryService.Handle(query, cancellationToken);
        var resources = HabitLogResourceFromEntityAssembler.ToResourceListFromEntityList(logs);
        return HabitsActionResultAssembler.ToActionResultFromGetAllResult(this, resources, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHabitLog([FromBody] CreateHabitLogResource resource, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var habit = await _habitRepository.FindByIdAndUserIdAsync(resource.HabitId, userId, cancellationToken);
        if (habit is null)
            return NotFound(new { message = "Habit not found." });

        var command = CreateHabitLogCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await _habitLogCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromCreateResult(this, result, _problemDetailsFactory,
            l => CreatedAtAction(nameof(GetHabitLogById), new { id = l.Id }, HabitLogResourceFromEntityAssembler.ToResourceFromEntity(l)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabitLogById(int id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var log = await _habitLogQueryService.Handle(new GetHabitLogByIdQuery(id), cancellationToken);
        if (log is null)
            return NotFound(new { message = "Habit log not found." });

        var habit = await _habitRepository.FindByIdAndUserIdAsync(log.HabitId, userId, cancellationToken);
        if (habit is null)
            return NotFound(new { message = "Habit log not found." });

        return Ok(HabitLogResourceFromEntityAssembler.ToResourceFromEntity(log));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHabitLog(int id, [FromBody] UpdateHabitLogResource resource, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var log = await _habitLogQueryService.Handle(new GetHabitLogByIdQuery(id), cancellationToken);
        if (log is null)
            return NotFound(new { message = "Habit log not found." });

        var habit = await _habitRepository.FindByIdAndUserIdAsync(log.HabitId, userId, cancellationToken);
        if (habit is null)
            return NotFound(new { message = "Habit log not found." });

        var command = UpdateHabitLogCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await _habitLogCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromCreateResult(this, result, _problemDetailsFactory,
            l => Ok(HabitLogResourceFromEntityAssembler.ToResourceFromEntity(l)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabitLog(int id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var log = await _habitLogQueryService.Handle(new GetHabitLogByIdQuery(id), cancellationToken);
        if (log is null)
            return NotFound(new { message = "Habit log not found." });

        var habit = await _habitRepository.FindByIdAndUserIdAsync(log.HabitId, userId, cancellationToken);
        if (habit is null)
            return NotFound(new { message = "Habit log not found." });

        var command = new DeleteHabitLogCommand(id);
        var result = await _habitLogCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromDeleteResult(this, result, _problemDetailsFactory);
    }

    private int GetAuthenticatedUserId()
    {
        var claim = User.FindFirst("user_id");
        return int.Parse(claim!.Value);
    }
}
