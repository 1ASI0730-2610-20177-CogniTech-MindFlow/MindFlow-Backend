using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetTagsHandler(IBaseRepository<Tag> repository)
    : IQueryHandler<GetTagsQuery, Result<IEnumerable<TagDto>>>
{
    public async Task<Result<IEnumerable<TagDto>>> Handle(GetTagsQuery request, CancellationToken ct)
    {
        var tags = await repository.ListAsync(ct);
        var filtered = tags.Where(t => t.UserId == request.UserId).ToList();

        return Result<IEnumerable<TagDto>>.Success(filtered.Select(Map));
    }

    private static TagDto Map(Tag t) => new()
    {
        Id = t.Id,
        UserId = t.UserId,
        Name = t.Name
    };
}