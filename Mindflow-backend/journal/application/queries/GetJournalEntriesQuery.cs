using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetJournalEntriesQuery : IQuery<Result<IEnumerable<JournalEntryDto>>>
{
    public int UserId { get; set; }
    public string? Sort { get; set; }
    public string? Order { get; set; }
    public int? Limit { get; set; }
    public string? Q { get; set; }
}