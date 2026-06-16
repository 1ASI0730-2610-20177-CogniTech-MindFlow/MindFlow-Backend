namespace Mindflow_backend.Habits.Domain.Model.Entities;

public class HabitCompletionLog
{
    public int Id { get; private set; }
    public int HabitId { get; private set; }
    public string HabitName { get; private set; }
    public string Category { get; private set; }
    public DateTime Date { get; private set; }
    public bool Completed { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    protected HabitCompletionLog()
    {
        HabitName = null!;
        Category = null!;
    }

    public HabitCompletionLog(int habitId, string habitName, string category, DateTime date, DateTime? completedAt = null)
    {
        HabitId = habitId;
        HabitName = habitName;
        Category = category;
        Date = date;
        Completed = true;
        CompletedAt = completedAt ?? DateTime.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDetails(string habitName, string category, bool completed, DateTime? completedAt)
    {
        HabitName = habitName;
        Category = category;
        Completed = completed;
        CompletedAt = completedAt;
    }
}
