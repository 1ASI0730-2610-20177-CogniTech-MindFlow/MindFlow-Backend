using Mindflow_backend.Habits.Application.Commands.Habits;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.CommandServices;

public class HabitCommandService : IHabitCommandService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HabitCommandService(IHabitRepository habitRepository, IUnitOfWork unitOfWork)
    {
        _habitRepository = habitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Habit>> Handle(CreateHabitCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var habit = new Habit(command.UserId, command.Name, command.Category, command.Frequency);
            await _habitRepository.AddAsync(habit, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result<Habit>.Success(habit);
        }
        catch (Exception ex)
        {
            return Result<Habit>.Failure(HabitsError.HabitCreationFailed, ex.Message);
        }
    }

    public async Task<Result<Habit>> Handle(UpdateHabitCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.FindByIdAsync(command.Id, cancellationToken);
        if (habit == null)
            return Result<Habit>.Failure(HabitsError.HabitNotFound, "Habit not found.");

        if (habit.UserId != command.UserId)
            return Result<Habit>.Failure(HabitsError.UserIdMismatch, "User ID mismatch.");

        try
        {
            habit.UpdateDetails(command.Name, command.Category, command.Frequency);
            _habitRepository.Update(habit);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result<Habit>.Success(habit);
        }
        catch (Exception ex)
        {
            return Result<Habit>.Failure(HabitsError.HabitUpdateFailed, ex.Message);
        }
    }

    public async Task<Result> Handle(DeleteHabitCommand command, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.FindByIdAsync(command.Id, cancellationToken);
        if (habit == null)
            return Result.Failure(HabitsError.HabitNotFound, "Habit not found.");

        if (habit.UserId != command.UserId)
            return Result.Failure(HabitsError.UserIdMismatch, "User ID mismatch.");

        try
        {
            _habitRepository.Remove(habit);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(HabitsError.HabitDeletionFailed, ex.Message);
        }
    }
}
