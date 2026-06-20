using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetTagsHandler(AppDbContext dbContext)
    : IQueryHandler<GetTagsQuery, Result<IEnumerable<TagDto>>>
{
    public async Task<Result<IEnumerable<TagDto>>> Handle(GetTagsQuery request, CancellationToken ct)
    {
        var tags = await dbContext.Tags
            .AsNoTracking()
            .Where(t => t.UserId == request.UserId)
            .Select(t => new TagDto { Id = t.Id, UserId = t.UserId, Name = t.Name })
            .ToListAsync(ct);

        return Result<IEnumerable<TagDto>>.Success(tags);
    }
}
