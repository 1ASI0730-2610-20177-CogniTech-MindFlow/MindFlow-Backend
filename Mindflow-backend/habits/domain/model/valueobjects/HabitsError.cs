namespace Mindflow_backend.Habits.Domain.Model.ValueObjects;

public enum HabitsError
{
    HabitNotFound,
    HabitCreationFailed,
    HabitUpdateFailed,
    HabitDeletionFailed,
    HabitLogNotFound,
    HabitLogCreationFailed,
    HabitLogUpdateFailed,
    HabitLogDeletionFailed,
    InvalidHabitFrequency,
    InvalidHabitStatus,
    InvalidHabitCategory,
    UserIdMismatch
}
