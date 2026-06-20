using Cortex.Mediator.Commands;
using Mindflow_backend.Analytics.Application.Services;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Journal.Domain.Model;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class UpdateJournalEntryHandler(
    IBaseRepository<JournalEntry> repository,
    IUnitOfWork unitOfWork,
    IAnalyticsCacheInvalidator cacheInvalidator) : ICommandHandler<UpdateJournalEntryCommand, Result<JournalEntryDto>>
{
    private static readonly string[] PositiveWords =
        ["feliz", "bien", "genial", "excelente", "alegre", "contento", "motivado", "logré",
         "happy", "great", "good", "amazing", "wonderful"];

    private static readonly string[] NegativeWords =
        ["triste", "mal", "terrible", "ansioso", "estresado", "frustrado", "agotado",
         "sad", "bad", "stressed", "anxious", "exhausted", "frustrated"];

    public async Task<Result<JournalEntryDto>> Handle(UpdateJournalEntryCommand request, CancellationToken ct)
    {
        var entry = await repository.FindByIdAsync(request.Id, ct);
        if (entry is null)
            return Result<JournalEntryDto>.Failure(
                 JournalError.JournalEntryNotFound, "Entry not found");

        var sentiment = request.Sentiment;
        if (string.IsNullOrWhiteSpace(sentiment)
            || string.Equals(sentiment, "auto", StringComparison.OrdinalIgnoreCase))
        {
            var text = $"{request.Content} {request.Title}".ToLowerInvariant();
            sentiment = PositiveWords.Any(text.Contains) ? "positive"
                      : NegativeWords.Any(text.Contains) ? "negative"
                      : "neutral";
        }

        entry.Title = request.Title;
        entry.Content = request.Content;
        entry.Sentiment = sentiment;
        entry.Category = request.Category;
        entry.HasPreview = request.Content.Length > 200; 

        repository.Update(entry);
        await unitOfWork.CompleteAsync(ct);
        await cacheInvalidator.InvalidateAsync(entry.UserId, entry.Date, ct);

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
        UpdatedAt = e.UpdatedAt,
        DeletedAt = e.DeletedAt
    };
}