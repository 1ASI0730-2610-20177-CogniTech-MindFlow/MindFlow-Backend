using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Analytics.Application.Services;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Analytics.Infrastructure.Services;

public class AnalyticsCacheInvalidator(AppDbContext dbContext) : IAnalyticsCacheInvalidator
{
    public async Task InvalidateAsync(int userId, CancellationToken ct = default)
    {
        await dbContext.AnalyticsCaches
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync(ct);

        await dbContext.WordClouds
            .Where(w => w.UserId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
