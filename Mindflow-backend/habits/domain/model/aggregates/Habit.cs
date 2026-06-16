using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.Habits.Domain.Model.Aggregates;

public class Habit : IAuditableEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public HabitFrequency Frequency { get; private set; }
    public int Streak { get; private set; }
    public HabitStatus Status { get; private set; }
    public bool PausedByAi { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public ICollection<HabitCompletionLog> CompletionLogs { get; private set; } = new List<HabitCompletionLog>();

    protected Habit()
    {
        Name = null!;
        Category = null!;
        Frequency = default;
        Status = default;
        CompletionLogs = new List<HabitCompletionLog>();
    }

    public Habit(int userId, string name, string category, HabitFrequency frequency)
    {
        UserId = userId;
        Name = name;
        Category = category;
        Frequency = frequency;
        Streak = 0;
        Status = HabitStatus.Pending;
        PausedByAi = false;
    }

    public void MarkCompleted()
    {
        Status = HabitStatus.Completed;
        Streak++;
    }

    public void MarkPending()
    {
        Status = HabitStatus.Pending;
    }

    public void PauseByAi()
    {
        Status = HabitStatus.PausedByAi;
        PausedByAi = true;
    }

    public void Resume()
    {
        Status = HabitStatus.Pending;
        PausedByAi = false;
    }

    public void UpdateDetails(string name, string category, HabitFrequency frequency)
    {
        Name = name;
        Category = category;
        Frequency = frequency;
    }

    public void UpdateFull(string name, string category, HabitFrequency frequency, int streak, HabitStatus status, bool pausedByAi)
    {
        Name = name;
        Category = category;
        Frequency = frequency;
        Streak = streak;
        Status = status;
        PausedByAi = pausedByAi;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
