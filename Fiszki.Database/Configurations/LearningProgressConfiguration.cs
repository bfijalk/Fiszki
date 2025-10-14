using Fiszki.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiszki.Database.Configurations;

public class LearningProgressConfiguration : IEntityTypeConfiguration<LearningProgress>
{
    public void Configure(EntityTypeBuilder<LearningProgress> b)
    {
        b.ToTable("learning_progress");
        b.HasKey(lp => lp.Id);
        b.HasIndex(lp => new { lp.UserId, lp.NextReviewDate });
        b.HasIndex(lp => lp.FlashcardId).IsUnique();
        b.Property(lp => lp.EaseFactor).HasColumnName("ease_factor");
        b.Property(lp => lp.Interval).HasColumnName("interval");
        b.Property(lp => lp.Repetitions).HasColumnName("repetitions");
        b.Property(lp => lp.NextReviewDate).HasColumnName("next_review_date");
        b.Property(lp => lp.LastReviewDate).HasColumnName("last_review_date");
        b.Property(lp => lp.CreatedAt).HasColumnName("created_at");
        b.Property(lp => lp.UpdatedAt).HasColumnName("updated_at");
        b.HasOne(lp => lp.Flashcard)
            .WithOne(f => f.LearningProgress)
            .HasForeignKey<LearningProgress>(lp => lp.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(lp => lp.User)
            .WithMany(u => u.LearningProgressEntries)
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
