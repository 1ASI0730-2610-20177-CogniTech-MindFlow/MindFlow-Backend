namespace Mindflow_backend.Shared.Domain;

public static class DateHelper
{
    public static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff);
    }

    public static DateOnly GetCurrentWeekStart() =>
        GetWeekStart(DateOnly.FromDateTime(DateTime.UtcNow));
}
