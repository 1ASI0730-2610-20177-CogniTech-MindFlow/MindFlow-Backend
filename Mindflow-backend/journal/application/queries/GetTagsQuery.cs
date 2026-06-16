using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetTagsQuery : IQuery<Result<IEnumerable<TagDto>>>
{
    public int? UserId { get; set; }
}