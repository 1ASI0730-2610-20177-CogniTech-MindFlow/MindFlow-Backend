using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetEntryTagsQuery : IRequest<Result<IEnumerable<EntryTagDto>>>
{
    public int? EntryId { get; set; }
}