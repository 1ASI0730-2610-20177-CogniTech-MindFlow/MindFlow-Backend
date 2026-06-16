using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Commands;

public class CreateEntryTagCommand : IRequest<Result<EntryTagDto>>
{
    public int EntryId { get; set; }
    public int TagId { get; set; }
}