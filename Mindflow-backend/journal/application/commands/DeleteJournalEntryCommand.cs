using Cortex.Mediator.Requests;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Commands;

public class DeleteJournalEntryCommand : IRequest<Result>
{
    public int Id { get; set; }
}