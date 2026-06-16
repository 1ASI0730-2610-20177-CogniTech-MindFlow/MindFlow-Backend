using Mindflow_backend.Notifications.Application.Services;

namespace Mindflow_backend.Notifications.Infrastructure.BackgroundServices;

public class HydrationReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<HydrationReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Hydration reminder service started.");

        using var timer = new PeriodicTimer(TimeSpan.FromHours(2));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                await notificationService.SendToAllAsync(
                    title: "💧 ¡Hora de hidratarte!",
                    body: "Recuerda tomar agua. Tu bienestar es importante.",
                    ct: stoppingToken);

                logger.LogInformation("Hydration reminder sent at {Time}.", DateTimeOffset.UtcNow);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error sending hydration reminder.");
            }
        }
    }
}
