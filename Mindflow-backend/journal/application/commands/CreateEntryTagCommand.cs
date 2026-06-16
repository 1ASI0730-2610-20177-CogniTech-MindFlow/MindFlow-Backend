using Cortex.Mediator.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Commands;

public class CreateEntryTagCommand : ICommand<Result<EntryTagDto>>
{
    public int EntryId { get; set; }
    public int TagId { get; set; }
}