using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mindflow_backend.Analytics.Domain.Entities;

namespace Mindflow_backend.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AnalyticsCacheConfiguration : IEntityTypeConfiguration<AnalyticsCache>
{
    public void Configure(EntityTypeBuilder<AnalyticsCache> builder)
    {
        builder.ToTable("analytics_caches");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.WeekStart).IsRequired();
        builder.Property(c => c.Score).IsRequired();
        builder.Property(c => c.TrendPercentage).HasMaxLength(20).IsRequired();

        builder.Property(c => c.StartDate);          // nullable por defecto
        builder.Property(c => c.EndDate);            // nullable por defecto

        builder.Property(c => c.AiInsight).HasColumnType("text");
        builder.Property(c => c.AiInsightLocalized).HasColumnType("text");
        builder.Property(c => c.Kpis).HasColumnType("jsonb");
        builder.Property(c => c.FluctuationData).HasColumnType("jsonb");
        builder.Property(c => c.TrendData).HasColumnType("jsonb");

        builder.HasIndex(c => new { c.UserId, c.WeekStart }).IsUnique();
    }
}