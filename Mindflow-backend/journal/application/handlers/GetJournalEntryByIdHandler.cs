using Cortex.Mediator.Requests;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Journal.Domain.Model;  
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetJournalEntryByIdHandler(AppDbContext dbContext)
    : IRequestHandler<GetJournalEntryByIdQuery, Result<JournalEntryDto>>
{
    public async Task<Result<JournalEntryDto>> Handle(GetJournalEntryByIdQuery request, CancellationToken ct)
    {
        var entry = await dbContext.JournalEntries
            .AsNoTracking()
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Media)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

        if (entry is null)
            return Result<JournalEntryDto>.Failure(
                JournalError.JournalEntryNotFound, "Entry not found");
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
        Tags = e.EntryTags?.Select(et => new TagDto
        {
            Id = et.Tag.Id,
            UserId = et.Tag.UserId,
            Name = et.Tag.Name
        }).ToList() ?? [],
        Media = e.Media?.Select(m => new MediaDto
        {
            Id = m.Id,
            EntryId = m.EntryId,
            Type = m.Type,
            Url = m.Url,
            CreatedAt = m.CreatedAt
        }).ToList() ?? [],
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        DeletedAt = e.DeletedAt
    };
}