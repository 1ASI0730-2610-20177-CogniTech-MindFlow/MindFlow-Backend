using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetMediaHandler(IBaseRepository<Media> repository)
    : IRequestHandler<GetMediaQuery, Result<IEnumerable<MediaDto>>>
{
    public async Task<Result<IEnumerable<MediaDto>>> Handle(GetMediaQuery request, CancellationToken ct)
    {
        var mediaList = await repository.ListAsync(ct);
        var filtered = mediaList.Where(m => m.EntryId == request.EntryId).ToList();

        return Result<IEnumerable<MediaDto>>.Success(filtered.Select(Map));
    }

    private static MediaDto Map(Media m) => new()
    {
        Id = m.Id,
        EntryId = m.EntryId,
        Type = m.Type,
        Url = m.Url,
        CreatedAt = m.CreatedAt
    };
}