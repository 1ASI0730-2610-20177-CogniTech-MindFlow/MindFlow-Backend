using Cortex.Mediator;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Commands;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Shared.Domain.Model.Errors;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Application.Handlers;

public class CreateWordCloudCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateWordCloudCommand, Result<WordCloudDto>>
{
    public async Task<Result<WordCloudDto>> Handle(CreateWordCloudCommand request, CancellationToken ct)
    {
        var existing = await dbContext.WordClouds
            .FirstOrDefaultAsync(w => w.UserId == request.UserId, ct);

        if (existing is not null)
        {
            existing.Words = request.Words;
        }
        else
        {
            existing = new WordCloud
            {
                UserId = request.UserId,
                Words = request.Words
            };
            dbContext.WordClouds.Add(existing);
        }

        await dbContext.SaveChangesAsync(ct);

        return Result<WordCloudDto>.Success(new WordCloudDto
        {
            Id = existing.Id,
            UserId = existing.UserId,
            Words = existing.Words
        });
    }
}