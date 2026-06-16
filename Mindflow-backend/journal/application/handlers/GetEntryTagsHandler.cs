using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetEntryTagsHandler(IBaseRepository<EntryTag> repository)
    : IQueryHandler<GetEntryTagsQuery, Result<IEnumerable<EntryTagDto>>>
{
    public async Task<Result<IEnumerable<EntryTagDto>>> Handle(GetEntryTagsQuery request, CancellationToken ct)
    {
        var entryTags = await repository.ListAsync(ct);
        var filtered = entryTags.Where(et => et.EntryId == request.EntryId).ToList();

        return Result<IEnumerable<EntryTagDto>>.Success(filtered.Select(Map));
    }

    private static EntryTagDto Map(EntryTag et) => new()
    {
        Id = et.Id,
        EntryId = et.EntryId,
        TagId = et.TagId
    };
}