using Cortex.Mediator.Commands;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class CreateEntryTagHandler(
    IBaseRepository<EntryTag> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateEntryTagCommand, Result<EntryTagDto>>
{
    public async Task<Result<EntryTagDto>> Handle(CreateEntryTagCommand request, CancellationToken ct)
    {
        var entryTag = new EntryTag
        {
            EntryId = request.EntryId,
            TagId = request.TagId
        };

        await repository.AddAsync(entryTag, ct);
        await unitOfWork.CompleteAsync(ct);

        return Result<EntryTagDto>.Success(Map(entryTag));
    }

    private static EntryTagDto Map(EntryTag et) => new()
    {
        Id = et.Id,
        EntryId = et.EntryId,
        TagId = et.TagId
    };
}