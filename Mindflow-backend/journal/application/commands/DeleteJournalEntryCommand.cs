using Cortex.Mediator.Commands;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Commands;

public class DeleteJournalEntryCommand : ICommand<Result>
{
    public int Id { get; set; }
}