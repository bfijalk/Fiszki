using Fiszki.Database.Entities;
using Fiszki.Services.Models.Generation;

namespace Fiszki.Services.Mapping;

public static class FlashcardMapper
{
    public static Flashcard ToEntity(FlashcardProposalDto dto, Guid userId)
    {
        return new Flashcard
        {
            Id = dto.Id,
            UserId = userId,
            FrontContent = dto.Front,
            BackContent = dto.Back,
            CreationSource = CreationSource.Ai,
            Tags = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static FlashcardProposalDto ToProposalDto(Flashcard entity)
    {
        return new FlashcardProposalDto(
            entity.Id,
            entity.FrontContent,
            entity.BackContent,
            null, // Example is not stored in the entity for MVP
            null, // Notes are not stored in the entity for MVP
            IsAccepted: true,
            IsRejected: false
        );
    }
}
