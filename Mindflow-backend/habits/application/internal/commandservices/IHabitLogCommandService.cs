using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Habits.Application.Internal.CommandServices;

public interface IHabitLogCommandService
{
    Task<Result<HabitCompletionLog>> Handle(CreateHabitLogCommand command, CancellationToken cancellationToken = default);
    Task<Result<HabitCompletionLog>> Handle(UpdateHabitLogCommand command, CancellationToken cancellationToken = default);
    Task<Result> Handle(DeleteHabitLogCommand command, CancellationToken cancellationToken = default);
}
