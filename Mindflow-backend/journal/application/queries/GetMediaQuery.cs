using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetMediaQuery : IQuery<Result<IEnumerable<MediaDto>>>
{
    public int? EntryId { get; set; }
}