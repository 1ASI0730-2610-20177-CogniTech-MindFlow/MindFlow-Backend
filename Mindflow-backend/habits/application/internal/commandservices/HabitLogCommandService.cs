using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.CommandServices;

public class HabitLogCommandService : IHabitLogCommandService
{
    private readonly IHabitCompletionLogRepository _logRepository;
    private readonly IHabitRepository _habitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HabitLogCommandService(
        IHabitCompletionLogRepository logRepository,
        IHabitRepository habitRepository,
        IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _habitRepository = habitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HabitCompletionLog>> Handle(CreateHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Date.Date > DateTime.UtcNow.Date)
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogCreationFailed, "No se pueden registrar logs con fecha futura.");

        try
        {
            var log = new HabitCompletionLog(command.HabitId, command.HabitName, command.Category, command.Date, command.CompletedAt);
            await _logRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            await RecalculateStreakAsync(command.HabitId, cancellationToken);

            return Result<HabitCompletionLog>.Success(log);
        }
        catch (Exception ex)
        {
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogCreationFailed, ex.Message);
        }
    }

    public async Task<Result<HabitCompletionLog>> Handle(UpdateHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.FindByIdAsync(command.Id, cancellationToken);
        if (log == null)
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogNotFound, "Habit log not found.");

        try
        {
            log.UpdateDetails(command.HabitName, command.Category, command.Completed, command.CompletedAt);
            _logRepository.Update(log);
            await _unitOfWork.CompleteAsync(cancellationToken);

            await RecalculateStreakAsync(log.HabitId, cancellationToken);

            return Result<HabitCompletionLog>.Success(log);
        }
        catch (Exception ex)
        {
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogUpdateFailed, ex.Message);
        }
    }

    public async Task<Result> Handle(DeleteHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.FindByIdAsync(command.Id, cancellationToken);
        if (log == null)
            return Result.Failure(HabitsError.HabitLogNotFound, "Habit log not found.");

        var habitId = log.HabitId;

        try
        {
            _logRepository.Remove(log);
            await _unitOfWork.CompleteAsync(cancellationToken);

            await RecalculateStreakAsync(habitId, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(HabitsError.HabitLogDeletionFailed, ex.Message);
        }
    }

    private async Task RecalculateStreakAsync(int habitId, CancellationToken ct)
    {
        var habit = await _habitRepository.FindByIdAsync(habitId, ct);
        if (habit is null) return;

        var logs = await _logRepository.FindByHabitIdAsync(habitId, ct);
        var completedDates = logs
            .Where(l => l.Completed)
            .Select(l => l.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var streak = ComputeStreak(completedDates, habit.Frequency);
        habit.SetStreak(streak);
        _habitRepository.Update(habit);
        await _unitOfWork.CompleteAsync(ct);
    }

    internal static int ComputeStreak(List<DateTime> sortedDatesDesc, HabitFrequency frequency)
    {
        if (sortedDatesDesc.Count == 0) return 0;

        var today = DateTime.UtcNow.Date;

        return frequency switch
        {
            HabitFrequency.Daily => ComputeDailyStreak(sortedDatesDesc, today),
            HabitFrequency.Weekly => ComputeWeeklyStreak(sortedDatesDesc, today),
            HabitFrequency.Monthly => ComputeMonthlyStreak(sortedDatesDesc, today),
            _ => 0
        };
    }

    private static int ComputeDailyStreak(List<DateTime> dates, DateTime today)
    {
        if (dates[0] != today && dates[0] != today.AddDays(-1))
            return 0;

        var expected = dates[0];
        var streak = 0;
        foreach (var date in dates)
        {
            if (date == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (date < expected)
            {
                break;
            }
        }

        return streak;
    }

    private static int ComputeWeeklyStreak(List<DateTime> dates, DateTime today)
    {
        var weeks = dates
            .Select(d => GetWeekStart(d))
            .Distinct()
            .OrderByDescending(w => w)
            .ToList();

        if (weeks.Count == 0) return 0;

        var currentWeek = GetWeekStart(today);
        var previousWeek = currentWeek.AddDays(-7);
        if (weeks[0] != currentWeek && weeks[0] != previousWeek)
            return 0;

        var expected = weeks[0];
        var streak = 0;
        foreach (var week in weeks)
        {
            if (week == expected)
            {
                streak++;
                expected = expected.AddDays(-7);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private static int ComputeMonthlyStreak(List<DateTime> dates, DateTime today)
    {
        var months = dates
            .Select(d => new DateTime(d.Year, d.Month, 1))
            .Distinct()
            .OrderByDescending(m => m)
            .ToList();

        if (months.Count == 0) return 0;

        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);
        if (months[0] != currentMonth && months[0] != previousMonth)
            return 0;

        var expected = months[0];
        var streak = 0;
        foreach (var month in months)
        {
            if (month == expected)
            {
                streak++;
                expected = expected.AddMonths(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff).Date;
    }
}
