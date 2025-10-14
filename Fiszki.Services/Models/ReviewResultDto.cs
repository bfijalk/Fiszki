namespace Fiszki.Services.Models;

public record ReviewResultDto(
    Guid FlashcardId,
    int Quality,
    double NewEaseFactor,
    int NewInterval,
    int NewRepetitions,
    DateTime NextReviewDate
);
