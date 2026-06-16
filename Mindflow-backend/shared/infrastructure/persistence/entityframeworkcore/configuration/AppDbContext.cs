using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Configuration;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Mindflow_backend.Support.Domain.Model.Entities;
using JournalEntry = Mindflow_backend.Journal.Domain.Entities.JournalEntry;
using EntryTag = Mindflow_backend.Journal.Domain.Entities.EntryTag;
using Tag = Mindflow_backend.Journal.Domain.Entities.Tag;
using Media = Mindflow_backend.Journal.Domain.Entities.Media;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AnalyticsCache> AnalyticsCaches => Set<AnalyticsCache>();
    public DbSet<WordCloud> WordClouds => Set<WordCloud>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EntryTag> EntryTags => Set<EntryTag>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
        builder.Entity<User>().Property(u => u.Name).HasMaxLength(100);
        builder.Entity<User>().Property(u => u.Occupation).HasMaxLength(100);
        builder.Entity<User>().Property(u => u.GoogleId).HasMaxLength(255);
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>().HasIndex(u => u.GoogleId).IsUnique();

        builder.Entity<JournalEntry>(entity =>
        {
            entity.HasMany(e => e.EntryTags)
                  .WithOne(et => et.Entry)
                  .HasForeignKey(et => et.EntryId);

            entity.HasMany(e => e.Media)
                  .WithOne(m => m.Entry)
                  .HasForeignKey(m => m.EntryId);

            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Date });
        });

        builder.Entity<Tag>(entity =>
        {
            entity.HasMany(t => t.EntryTags)
                  .WithOne(et => et.Tag)
                  .HasForeignKey(et => et.TagId);
        });

        builder.Entity<EntryTag>(entity =>
        {
            entity.HasIndex(et => new { et.EntryId, et.TagId }).IsUnique();
        });

        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Token).IsRequired().HasMaxLength(64);
            entity.Property(t => t.ExpiresAt).IsRequired();
            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasIndex(t => t.UserId);
        });

        builder.ApplyHabitsConfiguration();
        builder.ApplyAnalyticsConfiguration();

        builder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserEmail).IsRequired().HasMaxLength(255);
            entity.Property(t => t.Subject).IsRequired().HasMaxLength(255);
            entity.Property(t => t.Message).IsRequired();
            entity.Property(t => t.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(t => t.UserId);
        });

        builder.UseSnakeCaseNamingConvention();

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d));

        var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
            d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateOnly))
                    property.SetValueConverter(dateOnlyConverter);
                else if (property.ClrType == typeof(DateOnly?))
                    property.SetValueConverter(nullableDateOnlyConverter);
            }
        }
    }
}
