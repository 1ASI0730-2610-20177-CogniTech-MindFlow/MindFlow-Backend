using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;

namespace Mindflow_backend.Tests.Domain;

public class HabitTests
{
    [Fact]
    public void Constructor_InitializesWithCorrectDefaults()
    {
        var habit = new Habit(1, "Exercise", "Health", HabitFrequency.Daily);

        Assert.Equal(1, habit.UserId);
        Assert.Equal("Exercise", habit.Name);
        Assert.Equal("Health", habit.Category);
        Assert.Equal(HabitFrequency.Daily, habit.Frequency);
        Assert.Equal(0, habit.Streak);
        Assert.Equal(HabitStatus.Pending, habit.Status);
        Assert.False(habit.PausedByAi);
    }

    [Fact]
    public void MarkCompleted_IncrementsStreak()
    {
        var habit = new Habit(1, "Read", "Education", HabitFrequency.Daily);

        habit.MarkCompleted();
        Assert.Equal(1, habit.Streak);
        Assert.Equal(HabitStatus.Completed, habit.Status);

        habit.MarkCompleted();
        Assert.Equal(2, habit.Streak);
    }

    [Fact]
    public void PauseByAi_And_Resume_ToggleState()
    {
        var habit = new Habit(1, "Gym", "Health", HabitFrequency.Daily);

        habit.PauseByAi();
        Assert.Equal(HabitStatus.PausedByAi, habit.Status);
        Assert.True(habit.PausedByAi);

        habit.Resume();
        Assert.Equal(HabitStatus.Pending, habit.Status);
        Assert.False(habit.PausedByAi);
    }

    [Fact]
    public void UpdateFull_SetsAllProperties()
    {
        var habit = new Habit(1, "Run", "Health", HabitFrequency.Daily);

        habit.UpdateFull("Meditate", "Wellness", HabitFrequency.Weekly, 5, HabitStatus.Completed, true);

        Assert.Equal("Meditate", habit.Name);
        Assert.Equal("Wellness", habit.Category);
        Assert.Equal(HabitFrequency.Weekly, habit.Frequency);
        Assert.Equal(5, habit.Streak);
        Assert.Equal(HabitStatus.Completed, habit.Status);
        Assert.True(habit.PausedByAi);
    }

    [Fact]
    public void UpdateDetails_OnlyChangesNameCategoryFrequency()
    {
        var habit = new Habit(1, "Run", "Health", HabitFrequency.Daily);
        habit.MarkCompleted();

        habit.UpdateDetails("Walk", "Fitness", HabitFrequency.Weekly);

        Assert.Equal("Walk", habit.Name);
        Assert.Equal("Fitness", habit.Category);
        Assert.Equal(HabitFrequency.Weekly, habit.Frequency);
        Assert.Equal(1, habit.Streak);
    }

    [Fact]
    public void SoftDelete_SetsDeletedAt()
    {
        var habit = new Habit(1, "Read", "Education", HabitFrequency.Daily);
        Assert.Null(habit.DeletedAt);

        habit.SoftDelete();
        Assert.NotNull(habit.DeletedAt);
    }
}
