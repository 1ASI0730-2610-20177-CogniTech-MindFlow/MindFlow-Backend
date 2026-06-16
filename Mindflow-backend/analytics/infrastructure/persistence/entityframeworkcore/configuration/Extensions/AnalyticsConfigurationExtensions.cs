using Microsoft.EntityFrameworkCore;

namespace Mindflow_backend.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class AnalyticsConfigurationExtensions
{
    public static void ApplyAnalyticsConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new AnalyticsCacheConfiguration());
        builder.ApplyConfiguration(new WordCloudConfiguration());
    }
}