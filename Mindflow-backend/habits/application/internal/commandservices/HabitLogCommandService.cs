using Mindflow_backend.Habits.Application.Commands.HabitLogs;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Model.ValueObjects;
using Mindflow_backend.Habits.Domain.Repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Habits.Application.Internal.CommandServices;

public class HabitLogCommandService : IHabitLogCommandService
{
    private readonly IHabitCompletionLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HabitLogCommandService(IHabitCompletionLogRepository logRepository, IUnitOfWork unitOfWork)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HabitCompletionLog>> Handle(CreateHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new HabitCompletionLog(command.HabitId, command.HabitName, command.Category, command.Date, command.CompletedAt);
            await _logRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result<HabitCompletionLog>.Success(log);
        }
        catch (Exception ex)
        {
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogCreationFailed, ex.Message);
        }
    }

    public async Task<Result<HabitCompletionLog>> Handle(UpdateHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.FindByIdAsync(command.Id, cancellationToken);
        if (log == null)
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogNotFound, "Habit log not found.");

        try
        {
            log.UpdateDetails(command.HabitName, command.Category, command.Completed, command.CompletedAt);
            _logRepository.Update(log);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result<HabitCompletionLog>.Success(log);
        }
        catch (Exception ex)
        {
            return Result<HabitCompletionLog>.Failure(HabitsError.HabitLogUpdateFailed, ex.Message);
        }
    }

    public async Task<Result> Handle(DeleteHabitLogCommand command, CancellationToken cancellationToken = default)
    {
        var log = await _logRepository.FindByIdAsync(command.Id, cancellationToken);
        if (log == null)
            return Result.Failure(HabitsError.HabitLogNotFound, "Habit log not found.");

        try
        {
            _logRepository.Remove(log);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(HabitsError.HabitLogDeletionFailed, ex.Message);
        }
    }
}
