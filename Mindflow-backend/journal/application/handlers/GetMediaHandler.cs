using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetMediaHandler(AppDbContext dbContext)
    : IQueryHandler<GetMediaQuery, Result<IEnumerable<MediaDto>>>
{
    public async Task<Result<IEnumerable<MediaDto>>> Handle(GetMediaQuery request, CancellationToken ct)
    {
        var query = dbContext.Media
            .AsNoTracking()
            .Where(m => m.Entry.UserId == request.UserId);

        if (request.EntryId.HasValue)
            query = query.Where(m => m.EntryId == request.EntryId.Value);

        var media = await query
            .Select(m => new MediaDto
            {
                Id = m.Id,
                EntryId = m.EntryId,
                Type = m.Type,
                Url = m.Url,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);

        return Result<IEnumerable<MediaDto>>.Success(media);
    }
}
