using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Queries;

public class GetJournalEntryByIdQuery : IRequest<Result<JournalEntryDto>>
{
    public int Id { get; set; }
}