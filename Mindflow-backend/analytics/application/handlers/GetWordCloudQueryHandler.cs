using Cortex.Mediator.Queries;
using Mindflow_backend.Analytics.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Application.Queries;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class GetWordCloudQueryHandler(AppDbContext dbContext)
    : IQueryHandler<GetWordCloudQuery, Result<WordCloudDto>>
{
    public async Task<Result<WordCloudDto>> Handle(GetWordCloudQuery request, CancellationToken ct)
    {
        var wordCloud = await dbContext.WordClouds
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, ct);

        if (wordCloud is null)
            return Result<WordCloudDto>.Failure(
                 AnalyticsError.WordCloudNotFound, "No word cloud found for the current user."); 

        return Result<WordCloudDto>.Success(new WordCloudDto
        {
            Id = wordCloud.Id,
            UserId = wordCloud.UserId,
            Words = wordCloud.Words
        });
    }
}