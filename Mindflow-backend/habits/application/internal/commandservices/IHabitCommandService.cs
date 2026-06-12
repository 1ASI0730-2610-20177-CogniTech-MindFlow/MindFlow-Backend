using Mindflow_backend.Habits.Application.Commands.Habits;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Habits.Application.Internal.CommandServices;

public interface IHabitCommandService
{
    Task<Result<Habit>> Handle(CreateHabitCommand command, CancellationToken cancellationToken = default);
    Task<Result<Habit>> Handle(UpdateHabitCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteHabitCommand command, CancellationToken cancellationToken = default);
}
