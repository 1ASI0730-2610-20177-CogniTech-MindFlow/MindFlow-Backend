using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mindflow_backend.Analytics.Domain.Entities;

namespace Mindflow_backend.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class WordCloudConfiguration : IEntityTypeConfiguration<WordCloud>
{
    public void Configure(EntityTypeBuilder<WordCloud> builder)
    {
        builder.ToTable("word_clouds");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();

        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.Words).HasColumnType("jsonb");

        builder.HasIndex(w => w.UserId).IsUnique();
    }
}