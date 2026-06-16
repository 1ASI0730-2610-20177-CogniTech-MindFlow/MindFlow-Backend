using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.Habits.Application.Internal.CommandServices;
using Mindflow_backend.Habits.Application.Internal.QueryServices;
using Mindflow_backend.Habits.Application.Queries.Habits;
using Mindflow_backend.Habits.Interfaces.Rest.Assemblers;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;
using Mindflow_backend.Shared.Interfaces.Rest.ProblemDetails;

namespace Mindflow_backend.Habits.Interfaces.Rest.Controllers;

[ApiController]
[Route("habits")]
[Authorize]
public class HabitsController : ControllerBase
{
    private readonly IHabitCommandService _habitCommandService;
    private readonly IHabitQueryService _habitQueryService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public HabitsController(
        IHabitCommandService habitCommandService,
        IHabitQueryService habitQueryService,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _habitCommandService = habitCommandService;
        _habitQueryService = habitQueryService;
        _problemDetailsFactory = problemDetailsFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllHabits([FromQuery] int? user_id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (user_id.HasValue && user_id.Value != userId)
            return _problemDetailsFactory.CreateProblemDetails(this, 403, (Enum?)null, "User ID mismatch.");

        var query = new GetAllHabitsByUserIdQuery(userId);
        var habits = await _habitQueryService.Handle(query, cancellationToken);
        var resources = HabitResourceFromEntityAssembler.ToResourceListFromEntityList(habits);
        return HabitsActionResultAssembler.ToActionResultFromGetAllResult(this, resources, Ok);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabitById(int id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var query = new GetHabitByIdQuery(id, userId);
        var habit = await _habitQueryService.Handle(query, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromGetResult(this, habit, _problemDetailsFactory,
            h => Ok(HabitResourceFromEntityAssembler.ToResourceFromEntity(h)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateHabit([FromBody] CreateHabitResource resource, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var resourceWithUser = resource with { UserId = userId };
        var command = CreateHabitCommandFromResourceAssembler.ToCommandFromResource(resourceWithUser);
        var result = await _habitCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromCreateResult(this, result, _problemDetailsFactory,
            h => CreatedAtAction(nameof(GetHabitById), new { id = h.Id }, HabitResourceFromEntityAssembler.ToResourceFromEntity(h)));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHabit(int id, [FromBody] UpdateHabitResource resource, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var command = UpdateHabitCommandFromResourceAssembler.ToCommandFromResource(id, userId, resource);
        var result = await _habitCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromCreateResult(this, result, _problemDetailsFactory,
            h => Ok(HabitResourceFromEntityAssembler.ToResourceFromEntity(h)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabit(int id, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var command = new Habits.Application.Commands.Habits.DeleteHabitCommand(id, userId);
        var result = await _habitCommandService.Handle(command, cancellationToken);
        return HabitsActionResultAssembler.ToActionResultFromDeleteResult(this, result, _problemDetailsFactory);
    }

    private int GetAuthenticatedUserId()
    {
        var claim = User.FindFirst("user_id");
        return int.Parse(claim!.Value);
    }
}
