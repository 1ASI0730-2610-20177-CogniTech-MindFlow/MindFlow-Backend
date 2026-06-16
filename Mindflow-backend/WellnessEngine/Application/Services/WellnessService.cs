using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Mindflow_backend.WellnessEngine.Application.Dtos;

namespace Mindflow_backend.WellnessEngine.Application.Services;

public class WellnessService(
    AppDbContext dbContext,
    IHabitRepository habitRepository,
    IUnitOfWork unitOfWork,
    IAiService aiService) : IWellnessService
{
    public async Task<StressCheckResultDto> RunStressCheckAsync(int userId, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7);
        var entries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= cutoff)
            .OrderByDescending(e => e.Date)
            .Take(10)
            .ToListAsync(ct);

        var score = entries.Count == 0
            ? 50
            : (int)entries.Average(e => e.Sentiment?.ToLower() switch
            {
                "positive" => 100,
                "negative" => 0,
                _ => 50
            });

        var stressLevel = score >= 70 ? "low" : score >= 40 ? "medium" : "high";

        var habits = (await habitRepository.FindByUserIdAsync(userId, ct))
            .Where(h => h.DeletedAt == null)
            .ToList();

        var paused = new List<HabitAdjustmentDto>();
        var resumed = new List<HabitAdjustmentDto>();
        var changed = false;

        if (stressLevel == "high")
        {
            foreach (var h in habits.Where(h => h.Status != HabitStatus.PausedByAi))
            {
                h.PauseByAi();
                habitRepository.Update(h);
                paused.Add(new HabitAdjustmentDto { Id = h.Id, Name = h.Name, Category = h.Category });
                changed = true;
            }
        }
        else if (stressLevel == "low")
        {
            foreach (var h in habits.Where(h => h.PausedByAi))
            {
                h.Resume();
                habitRepository.Update(h);
                resumed.Add(new HabitAdjustmentDto { Id = h.Id, Name = h.Name, Category = h.Category });
                changed = true;
            }
        }

        if (changed)
            await unitOfWork.CompleteAsync(ct);

        var habitNames = habits.Select(h => h.Name);
        var geminiAdvice = await aiService.GenerateStressAdviceAsync(stressLevel, score, entries.Count, habitNames);
        var advice = string.IsNullOrEmpty(geminiAdvice) ? DefaultAdvice(stressLevel) : geminiAdvice;

        return new StressCheckResultDto
        {
            StressLevel = stressLevel,
            Score = score,
            AnalyzedEntries = entries.Count,
            PausedHabits = paused,
            ResumedHabits = resumed,
            Advice = advice
        };
    }

    private static string DefaultAdvice(string stressLevel) => stressLevel switch
    {
        "high" => "Detectamos un nivel de estrés elevado esta semana. Hemos pausado temporalmente tus hábitos para que puedas descansar. Cuídate y recuerda que el descanso también es parte del progreso.",
        "low" => "¡Tienes una energía emocional excelente esta semana! Hemos reactivado tus hábitos pausados. Es un gran momento para retomar tu rutina.",
        _ => "Tu bienestar emocional está equilibrado. Continúa con tus hábitos y recuerda tomarte pequeños momentos para cuidarte."
    };
}
