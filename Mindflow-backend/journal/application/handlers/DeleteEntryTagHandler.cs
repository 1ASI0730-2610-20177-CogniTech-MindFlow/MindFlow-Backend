using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class DeleteEntryTagHandler(
    IBaseRepository<EntryTag> repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteEntryTagCommand, Result>
{
    public async Task<Result> Handle(DeleteEntryTagCommand request, CancellationToken ct)
    {
        var entryTag = await repository.FindByIdAsync(request.Id, ct);
        if (entryTag is null)
            return Result.Failure(new Error("EntryTag.NotFound", "Entry tag not found"), "Entry tag not found");

        repository.Remove(entryTag);
        await unitOfWork.CompleteAsync(ct);

        return Result.Success();
    }
}