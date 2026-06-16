using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Application.Queries;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Journal.Application.Handlers;

public class GetJournalEntriesHandler(AppDbContext dbContext)
    : IQueryHandler<GetJournalEntriesQuery, Result<IEnumerable<JournalEntryDto>>>
{
    public async Task<Result<IEnumerable<JournalEntryDto>>> Handle(GetJournalEntriesQuery request, CancellationToken ct)
    {
        IQueryable<JournalEntry> query = dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == request.UserId)
            .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
            .Include(e => e.Media);

        if (!string.IsNullOrWhiteSpace(request.Q))
            query = query.Where(e => e.Title.Contains(request.Q) || e.Content.Contains(request.Q));

        bool ascending = request.Order?.ToLower() == "asc";

        query = (request.Sort?.ToLower()) switch
        {
            "date" => ascending ? query.OrderBy(e => e.Date) : query.OrderByDescending(e => e.Date),
            _ => ascending ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt)
        };

        var entries = await query.Take(request.Limit ?? 3).ToListAsync(ct);

        return Result<IEnumerable<JournalEntryDto>>.Success(entries.Select(Map));
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
        AiResponse = e.AiResponse,
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