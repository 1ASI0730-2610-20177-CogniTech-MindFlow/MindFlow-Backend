using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetEntryTagsHandler(AppDbContext dbContext)
    : IQueryHandler<GetEntryTagsQuery, Result<IEnumerable<EntryTagDto>>>
{
    public async Task<Result<IEnumerable<EntryTagDto>>> Handle(GetEntryTagsQuery request, CancellationToken ct)
    {
        var query = dbContext.EntryTags.AsNoTracking().AsQueryable();

        if (request.EntryId.HasValue)
            query = query.Where(et => et.EntryId == request.EntryId.Value);

        var entryTags = await query
            .Select(et => new EntryTagDto { Id = et.Id, EntryId = et.EntryId, TagId = et.TagId })
            .ToListAsync(ct);

        return Result<IEnumerable<EntryTagDto>>.Success(entryTags);
    }
}
