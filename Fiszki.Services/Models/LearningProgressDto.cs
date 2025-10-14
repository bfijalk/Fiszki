namespace Fiszki.Services.Models;

public record LearningProgressDto(
    Guid FlashcardId,
    double EaseFactor,
    int Interval,
    int Repetitions,
    DateTime NextReviewDate,
    DateTime? LastReviewDate
);

