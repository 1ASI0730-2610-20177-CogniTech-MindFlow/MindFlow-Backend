using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiFeedback.Application.Dtos;
using Mindflow_backend.AiFeedback.Application.Services;
using Mindflow_backend.AiFeedback.Domain.Model.Entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.AiFeedback.Infrastructure.Services;

public class AiFeedbackService(AppDbContext db) : IAiFeedbackService
{
    private static readonly HashSet<string> ValidContentTypes = ["journal", "habit"];

    public async Task<AiFeedbackRating> SubmitRatingAsync(
        int userId, int contentId, string contentType, int rating, string? comment)
    {
        if (!ValidContentTypes.Contains(contentType))
            throw new ArgumentException($"Tipo de contenido inválido: {contentType}. Valores permitidos: journal, habit.");

        if (rating is < 1 or > 5)
            throw new ArgumentException("La calificación debe estar entre 1 y 5.");

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

        await db.SaveChangesAsync();
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
        var ratings = await db.AiFeedbackRatings
            .Where(f => f.UserId == userId)
            .ToListAsync();

        if (ratings.Count == 0)
            return new AiFeedbackSummaryDto(0, 0, new Dictionary<int, int>
            {
                [1] = 0, [2] = 0, [3] = 0, [4] = 0, [5] = 0
            });

        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(r => r, r => ratings.Count(f => f.Rating == r));

        return new AiFeedbackSummaryDto(
            ratings.Count,
            Math.Round(ratings.Average(f => f.Rating), 2),
            distribution);
    }
}