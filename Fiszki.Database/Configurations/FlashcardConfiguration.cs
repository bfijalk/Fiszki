using Fiszki.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiszki.Database.Configurations;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> b)
    {
        b.ToTable("flashcards");
        b.HasKey(f => f.Id);
        b.HasIndex(f => f.UserId);
        b.Property(f => f.FrontContent).HasColumnName("front_content").IsRequired();
        b.Property(f => f.BackContent).HasColumnName("back_content").IsRequired();
        b.Property(f => f.CreationSource).HasConversion<string>().HasColumnName("creation_source");
        b.Property(f => f.AiModel).HasColumnName("ai_model");
        b.Property(f => f.OriginalTextHash).HasColumnName("original_text_hash");
        b.Property(f => f.Tags).HasColumnName("tags");
        b.Property(f => f.CreatedAt).HasColumnName("created_at");
        b.Property(f => f.UpdatedAt).HasColumnName("updated_at");
        b.HasOne(f => f.User)
            .WithMany(u => u.Flashcards)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
