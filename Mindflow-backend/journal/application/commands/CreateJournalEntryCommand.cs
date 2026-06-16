using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.Journal.Application.Commands;

public class CreateJournalEntryCommand : IRequest<Result<JournalEntryDto>>
{
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "neutral";
    public string Category { get; set; } = "Sin categoría";
}