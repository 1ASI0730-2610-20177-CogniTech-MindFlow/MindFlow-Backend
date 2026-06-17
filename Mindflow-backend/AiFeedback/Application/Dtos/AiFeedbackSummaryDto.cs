namespace Mindflow_backend.AiFeedback.Application.Dtos;

public record AiFeedbackSummaryDto(
    int TotalRatings,
    double AverageRating,
    Dictionary<int, int> Distribution);