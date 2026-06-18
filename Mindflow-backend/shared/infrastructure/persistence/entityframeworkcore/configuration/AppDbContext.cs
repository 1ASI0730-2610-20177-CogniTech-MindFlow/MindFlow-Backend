using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Configuration;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Mindflow_backend.Notifications.Domain.Model.Entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Encryption;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Mindflow_backend.AiFeedback.Domain.Model.Entities;
using Mindflow_backend.AiIntegration.Domain.Model.Entities;
using Mindflow_backend.Support.Domain.Model.Entities;
using Mindflow_backend.Habits.Domain.Model.Entities;
using Mindflow_backend.Subscriptions.Domain.Model.Entities;
using Mindflow_backend.Chat.Domain.Entities;
using JournalEntry = Mindflow_backend.Journal.Domain.Entities.JournalEntry;
using EntryTag = Mindflow_backend.Journal.Domain.Entities.EntryTag;
using Tag = Mindflow_backend.Journal.Domain.Entities.Tag;
using Media = Mindflow_backend.Journal.Domain.Entities.Media;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public class AppDbContext(DbContextOptions options, AesEncryptionService encryptionService) : DbContext(options)
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
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<AiFeedbackRating> AiFeedbackRatings => Set<AiFeedbackRating>();
    public DbSet<AiMetricLog> AiMetricLogs => Set<AiMetricLog>();
    public DbSet<CachedHabitSuggestion> CachedHabitSuggestions => Set<CachedHabitSuggestion>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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
        builder.Entity<User>().Property(u => u.PinHash).HasMaxLength(255);
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>().HasIndex(u => u.GoogleId).IsUnique();

        var encryptedConverter = new EncryptedStringConverter(encryptionService);

        builder.Entity<JournalEntry>(entity =>
        {
            entity.Property(e => e.Content).HasColumnType("LONGTEXT").HasConversion(encryptedConverter);
            entity.Property(e => e.AiResponse).HasColumnType("TEXT");

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

        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Plan).IsRequired().HasMaxLength(20);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(20);
            entity.Property(s => s.StripeCustomerId).HasMaxLength(100);
            entity.Property(s => s.StripeSubscriptionId).HasMaxLength(100);
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasIndex(s => s.StripeCustomerId);
        });

        builder.Entity<DeviceToken>(entity =>
        {
            entity.HasKey(dt => dt.Id);
            entity.Property(dt => dt.Token).IsRequired().HasMaxLength(512);
            entity.Property(dt => dt.Platform).IsRequired().HasMaxLength(20);
            entity.HasIndex(dt => dt.Token).IsUnique();
            entity.HasIndex(dt => dt.UserId);
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

        builder.Entity<AiFeedbackRating>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.ContentType).IsRequired().HasMaxLength(20);
            entity.Property(f => f.Rating).IsRequired();
            entity.Property(f => f.Comment).HasMaxLength(500);
            entity.HasIndex(f => f.UserId);
            entity.HasIndex(f => new { f.UserId, f.ContentId, f.ContentType }).IsUnique();
        });

        builder.Entity<AiMetricLog>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Operation).IsRequired().HasMaxLength(50);
            entity.Property(m => m.ErrorMessage).HasMaxLength(500);
            entity.HasIndex(m => m.CreatedAt);
        });

        builder.Entity<CachedHabitSuggestion>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.SuggestionsJson).IsRequired().HasColumnType("TEXT");
            entity.HasIndex(c => c.UserId).IsUnique();
        });

        builder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Category).IsRequired().HasMaxLength(50);
            entity.HasIndex(c => c.UserId);
            entity.HasMany(c => c.Messages)
                  .WithOne(m => m.Conversation)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Role).IsRequired().HasMaxLength(20);
            entity.Property(m => m.Content).IsRequired().HasColumnType("LONGTEXT")
                  .HasConversion(encryptedConverter);
            entity.HasIndex(m => m.ConversationId);
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
