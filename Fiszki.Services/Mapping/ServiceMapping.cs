using Fiszki.Database.Entities;
using Fiszki.Services.Models;

namespace Fiszki.Services.Mapping;

internal static class ServiceMapping
{
    public static UserDto ToDto(this User u) => new(
        u.Id,
        u.Email,
        u.Role.ToString(),
        u.IsActive,
        u.CreatedAt,
        u.UpdatedAt,
        u.TotalCardsGenerated,
        u.TotalCardsAccepted);

    public static FlashcardDto ToDto(this Flashcard f) => new(
        f.Id,
        f.FrontContent,
        f.BackContent,
        f.CreationSource.ToString(),
        f.AiModel,
        f.Tags.AsReadOnly(),
        f.CreatedAt,
        f.UpdatedAt);

    public static LearningProgressDto ToDto(this LearningProgress lp) => new(
        lp.FlashcardId,
        lp.EaseFactor,
        lp.Interval,
        lp.Repetitions,
        lp.NextReviewDate,
        lp.LastReviewDate);
}

