using Mindflow_backend.Habits.Application.Internal.CommandServices;
using Mindflow_backend.Habits.Application.Queries.Habits;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.QueryServices;

public class HabitQueryService(
    IHabitRepository habitRepository,
    IHabitCompletionLogRepository logRepository) : IHabitQueryService
{
    public async Task<Habit?> Handle(GetHabitByIdQuery query, CancellationToken cancellationToken = default)
    {
        var habit = await habitRepository.FindByIdAndUserIdAsync(query.Id, query.UserId, cancellationToken);
        if (habit is not null)
            await RecalculateAtQueryTime(habit, cancellationToken);
        return habit;
    }

    public async Task<IEnumerable<Habit>> Handle(GetAllHabitsByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        var habits = (await habitRepository.FindByUserIdAsync(query.UserId, cancellationToken)).ToList();
        foreach (var habit in habits)
            await RecalculateAtQueryTime(habit, cancellationToken);
        return habits;
    }

    private async Task RecalculateAtQueryTime(Habit habit, CancellationToken ct)
    {
        var logs = await logRepository.FindByHabitIdAsync(habit.Id, ct);
        var completedDates = logs
            .Where(l => l.Completed)
            .Select(l => l.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var streak = HabitLogCommandService.ComputeStreak(completedDates, habit.Frequency);
        habit.SetStreak(streak);

        if (habit.Status == HabitStatus.Completed && habit.Frequency == HabitFrequency.Daily)
        {
            var today = DateTime.UtcNow.Date;
            var hasLogToday = completedDates.Any(d => d == today);
            if (!hasLogToday)
                habit.MarkPending();
        }
        else if (habit.Status == HabitStatus.Completed && habit.Frequency == HabitFrequency.Weekly)
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = GetWeekStart(today);
            var hasLogThisWeek = completedDates.Any(d => GetWeekStart(d) == weekStart);
            if (!hasLogThisWeek)
                habit.MarkPending();
        }
        else if (habit.Status == HabitStatus.Completed && habit.Frequency == HabitFrequency.Monthly)
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var hasLogThisMonth = completedDates.Any(d => new DateTime(d.Year, d.Month, 1) == monthStart);
            if (!hasLogThisMonth)
                habit.MarkPending();
        }
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff).Date;
    }
}
