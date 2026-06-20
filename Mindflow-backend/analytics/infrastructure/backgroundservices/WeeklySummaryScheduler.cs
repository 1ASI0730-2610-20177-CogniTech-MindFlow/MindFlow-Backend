using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Services;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Infrastructure.BackgroundServices;

public class WeeklySummaryScheduler(
    IServiceScopeFactory scopeFactory,
    ILogger<WeeklySummaryScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Weekly summary scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextSunday();
            logger.LogInformation("Next weekly summary computation scheduled at {Time}.", DateTime.UtcNow.Add(delay));

            await Task.Delay(delay, stoppingToken);

            await ComputeForAllUsersAsync(stoppingToken);
        }
    }

    private async Task ComputeForAllUsersAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var computationService = scope.ServiceProvider.GetRequiredService<AnalyticsComputationService>();

            var userIds = await dbContext.Users
                .AsNoTracking()
                .Select(u => u.Id)
                .ToListAsync(ct);

            var weekStart = GetCurrentWeekStart();

            logger.LogInformation("Computing weekly summary for {Count} users (week {Week}).", userIds.Count, weekStart);

            foreach (var userId in userIds)
            {
                try
                {
                    await computationService.ComputeAndSaveWeeklyAsync(userId, weekStart);
                    await computationService.ComputeAndSaveWordCloudAsync(userId);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error computing weekly summary for user {UserId}.", userId);
                }
            }

            logger.LogInformation("Weekly summary computation completed for {Count} users.", userIds.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error during weekly summary batch computation.");
        }
    }

    private static TimeSpan GetDelayUntilNextSunday()
    {
        var now = DateTime.UtcNow;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && now.Hour >= 6)
            daysUntilSunday = 7;

        var nextSunday = now.Date.AddDays(daysUntilSunday).AddHours(6);
        return nextSunday - now;
    }

    private static DateOnly GetCurrentWeekStart() => Shared.Domain.DateHelper.GetCurrentWeekStart();
}