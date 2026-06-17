using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.Habits.Application.Internal.CommandServices;
using Mindflow_backend.Habits.Application.Internal.QueryServices;
using Mindflow_backend.Habits.Application.Queries.Habits;
using Mindflow_backend.Habits.Interfaces.Rest.Assemblers;
using Mindflow_backend.Habits.Interfaces.Rest.Resources.Habits;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
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
    private readonly IAiService _aiService;
    private readonly AppDbContext _dbContext;

    public HabitsController(
        IHabitCommandService habitCommandService,
        IHabitQueryService habitQueryService,
        ProblemDetailsFactory problemDetailsFactory,
        IAiService aiService,
        AppDbContext dbContext)
    {
        _habitCommandService = habitCommandService;
        _habitQueryService = habitQueryService;
        _problemDetailsFactory = problemDetailsFactory;
        _aiService = aiService;
        _dbContext = dbContext;
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

    [HttpGet("streak-summary")]
    public async Task<IActionResult> GetStreakSummary(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        var query = new GetAllHabitsByUserIdQuery(userId);
        var habits = await _habitQueryService.Handle(query, cancellationToken);
        var summary = habits
            .OrderByDescending(h => h.Streak)
            .Select(h => new
            {
                habitId = h.Id,
                name = h.Name,
                streak = h.Streak,
                status = h.Status.ToString()
            });
        return Ok(summary);
    }

    [HttpPost("suggestions")]
    public async Task<IActionResult> GetSuggestions(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var habits = await _habitQueryService.Handle(new GetAllHabitsByUserIdQuery(userId), cancellationToken);
        var habitNames = habits.Select(h => h.Name).ToList();

        var totalLogs = await _dbContext.Set<Mindflow_backend.Habits.Domain.Model.Entities.HabitCompletionLog>()
            .CountAsync(l => habits.Select(h => h.Id).Contains(l.HabitId), cancellationToken);
        var completionRate = habits.Any() ? (int)((double)totalLogs / (habits.Count() * 7) * 100) : 0;
        if (completionRate > 100) completionRate = 100;

        var recentEntries = await _dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(5)
            .Select(e => e.Content)
            .ToListAsync(cancellationToken);

        var sentiments = await _dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(10)
            .Select(e => e.Sentiment)
            .ToListAsync(cancellationToken);

        var negativeCount = sentiments.Count(s => s.Equals("negative", StringComparison.OrdinalIgnoreCase));
        var stressLevel = negativeCount >= 5 ? "high" : negativeCount >= 2 ? "medium" : "low";

        var aiResponse = await _aiService.GenerateHabitSuggestionsAsync(habitNames, completionRate, stressLevel, recentEntries);

        if (string.IsNullOrEmpty(aiResponse))
            return Ok(new { suggestions = FallbackSuggestions(stressLevel, habitNames) });

        try
        {
            var cleaned = aiResponse.Trim();
            if (cleaned.StartsWith("```")) cleaned = cleaned.Split('\n', 3).Length > 1
                ? cleaned[(cleaned.IndexOf('\n') + 1)..cleaned.LastIndexOf("```")] : cleaned;

            var suggestions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(cleaned.Trim());
            return Ok(new { suggestions });
        }
        catch
        {
            return Ok(new { suggestions = FallbackSuggestions(stressLevel, habitNames) });
        }
    }

    private static List<object> FallbackSuggestions(string stressLevel, List<string> currentHabits)
    {
        var suggestions = new List<object>();
        var hasPhysical = currentHabits.Any(h => h.Contains("ejercicio", StringComparison.OrdinalIgnoreCase) || h.Contains("caminar", StringComparison.OrdinalIgnoreCase));
        var hasMental = currentHabits.Any(h => h.Contains("meditar", StringComparison.OrdinalIgnoreCase) || h.Contains("respirar", StringComparison.OrdinalIgnoreCase));

        if (!hasMental)
            suggestions.Add(new { name = stressLevel == "high" ? "Respiracion profunda 5 min" : "Meditacion 10 min", category = "Salud Mental", frequency = "Daily", reason = "La meditacion y respiracion ayudan a reducir el estres y mejorar la concentracion." });
        if (!hasPhysical)
            suggestions.Add(new { name = "Caminata al aire libre 20 min", category = "Salud Fisica", frequency = "Daily", reason = "La actividad fisica mejora el estado de animo y la energia." });

        suggestions.Add(new { name = "Escribir 3 cosas positivas del dia", category = "Bienestar", frequency = "Daily", reason = "La gratitud diaria mejora la perspectiva emocional." });

        return suggestions.Take(3).ToList();
    }

    private int GetAuthenticatedUserId()
    {
        var claim = User.FindFirst("user_id");
        return int.Parse(claim!.Value);
    }
}
