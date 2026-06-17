using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Habits.Domain.Model.Aggregates;
using Mindflow_backend.Habits.Domain.Model.Entities;

namespace Mindflow_backend.Habits.Infrastructure.Persistence.Ef.Configuration;

public static class HabitsModelBuilderExtensions
{
    public static void ApplyHabitsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Habit>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(h => h.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(h => h.Frequency)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(h => h.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(h => h.Streak)
                .HasDefaultValue(0);

            entity.Property(h => h.PausedByAi)
                .HasDefaultValue(false);

            entity.HasMany(h => h.CompletionLogs)
                .WithOne()
                .HasForeignKey(l => l.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => h.UserId);
            entity.HasIndex(h => new { h.UserId, h.Status });
        });

        builder.Entity<HabitCompletionLog>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.HabitName)
                .HasMaxLength(200);

            entity.Property(l => l.Category)
                .HasMaxLength(100);

            entity.Property(l => l.Date)
                .IsRequired();

            entity.Property(l => l.Completed)
                .HasDefaultValue(true);

            entity.HasIndex(l => l.HabitId);
            entity.HasIndex(l => new { l.HabitId, l.Date });
            entity.HasIndex(l => l.Date);
        });
    }
}
