using Cortex.Mediator.Commands;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class CreateJournalEntryHandler(
    IBaseRepository<JournalEntry> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateJournalEntryCommand, Result<JournalEntryDto>>
{
    public async Task<Result<JournalEntryDto>> Handle(CreateJournalEntryCommand request, CancellationToken ct)
    {
        var entry = new JournalEntry
        {
            UserId = request.UserId,
            Date = request.Date,
            Title = request.Title,
            Content = request.Content,
            Sentiment = request.Sentiment,
            Category = request.Category,
            HasPreview = request.Content.Length > 200
        };

        await repository.AddAsync(entry, ct);
        await unitOfWork.CompleteAsync(ct);

        return Result<JournalEntryDto>.Success(Map(entry));
    }

    private static JournalEntryDto Map(JournalEntry e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Date = e.Date,
        Title = e.Title,
        Content = e.Content,
        Sentiment = e.Sentiment,
        Category = e.Category,
        HasPreview = e.HasPreview,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}