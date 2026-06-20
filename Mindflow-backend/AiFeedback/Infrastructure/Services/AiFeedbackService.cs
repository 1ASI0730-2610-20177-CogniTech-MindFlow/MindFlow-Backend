using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiFeedback.Application.Dtos;
using Mindflow_backend.AiFeedback.Application.Services;
using Mindflow_backend.AiFeedback.Domain.Model.Entities;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.AiFeedback.Infrastructure.Services;

public class AiFeedbackService(AppDbContext db, IUnitOfWork unitOfWork) : IAiFeedbackService
{
    private static readonly HashSet<string> ValidContentTypes = ["journal", "habit"];

    public async Task<AiFeedbackRating> SubmitRatingAsync(
        int userId, int contentId, string contentType, int rating, string? comment)
    {
        if (!ValidContentTypes.Contains(contentType))
            throw new ArgumentException($"Tipo de contenido inválido: {contentType}. Valores permitidos: journal, habit.");

        if (rating is < 1 or > 5)
            throw new ArgumentException("La calificación debe estar entre 1 y 5.");

        var contentExists = contentType switch
        {
            "journal" => await db.JournalEntries.AnyAsync(e => e.Id == contentId),
            "habit" => await db.Set<Mindflow_backend.Habits.Domain.Model.Aggregates.Habit>()
                .AnyAsync(h => h.Id == contentId),
            _ => false
        };

        if (!contentExists)
            throw new ArgumentException($"No se encontró el contenido con id {contentId} de tipo '{contentType}'.");

        var existing = await db.AiFeedbackRatings
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentId == contentId && f.ContentType == contentType);

        if (existing is not null)
        {
            existing.Rating = rating;
            existing.Comment = comment;
        }
        else
        {
            existing = new AiFeedbackRating
            {
                UserId = userId,
                ContentId = contentId,
                ContentType = contentType,
                Rating = rating,
                Comment = comment
            };
            db.AiFeedbackRatings.Add(existing);
        }

        await unitOfWork.CompleteAsync();
        return existing;
    }

    public async Task<IEnumerable<AiFeedbackRating>> GetUserRatingsAsync(int userId)
    {
        return await db.AiFeedbackRatings
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<AiFeedbackSummaryDto> GetSummaryAsync(int userId)
    {
        var stats = await db.AiFeedbackRatings
            .Where(f => f.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Average = g.Average(f => (double)f.Rating)
            })
            .FirstOrDefaultAsync();

        if (stats is null || stats.Total == 0)
            return new AiFeedbackSummaryDto(0, 0, new Dictionary<int, int>
            {
                [1] = 0, [2] = 0, [3] = 0, [4] = 0, [5] = 0
            });

        var distribution = await db.AiFeedbackRatings
            .Where(f => f.UserId == userId)
            .GroupBy(f => f.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Rating, g => g.Count);

        for (var i = 1; i <= 5; i++)
            distribution.TryAdd(i, 0);

        return new AiFeedbackSummaryDto(
            stats.Total,
            Math.Round(stats.Average, 2),
            distribution);
    }
}
