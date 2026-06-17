using Mindflow_backend.AiFeedback.Application.Dtos;
using Mindflow_backend.AiFeedback.Domain.Model.Entities;

namespace Mindflow_backend.AiFeedback.Application.Services;

public interface IAiFeedbackService
{
    Task<AiFeedbackRating> SubmitRatingAsync(int userId, int contentId, string contentType, int rating, string? comment);
    Task<IEnumerable<AiFeedbackRating>> GetUserRatingsAsync(int userId);
    Task<AiFeedbackSummaryDto> GetSummaryAsync(int userId);
}