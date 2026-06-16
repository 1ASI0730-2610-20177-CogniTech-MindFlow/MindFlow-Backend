using Cortex.Mediator.Queries;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetJournalEntryByIdQuery : IQuery<Result<JournalEntryDto>>
{
    public int Id { get; set; }
}