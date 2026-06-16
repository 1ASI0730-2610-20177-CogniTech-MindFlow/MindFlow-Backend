using Cortex.Mediator.Commands;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Journal.Domain.Model;  
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class DeleteJournalEntryHandler(
    IBaseRepository<JournalEntry> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteJournalEntryCommand, Result>
{
    public async Task<Result> Handle(DeleteJournalEntryCommand request, CancellationToken ct)
    {
        var entry = await repository.FindByIdAsync(request.Id, ct);
        if (entry is null)
            return Result.Failure(
                JournalError.JournalEntryNotFound, "Entry not found");

        entry.DeletedAt = DateTimeOffset.UtcNow;
        repository.Update(entry);
        await unitOfWork.CompleteAsync(ct);

        return Result.Success();
    }
}