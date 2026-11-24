namespace Fiszki.Services.Models;

/// <summary>Which flashcards of a user a list returns.</summary>
public enum FlashcardListState
{
    /// <summary>Flashcards that are not archived (the main list).</summary>
    Active = 0,

    /// <summary>Flashcards the user marked as learned.</summary>
    Archived = 1
}

/// <summary>
/// Counts over all flashcards of a user, archived or not. <paramref name="Ai"/> plus <paramref name="Manual"/>
/// equals <paramref name="Total"/>.
/// </summary>
/// <param name="LearningProgress">Archived share of all flashcards in whole percent, rounded down.</param>
public record FlashcardSummaryDto(int Total, int Ai, int Manual, int Archived, int LearningProgress);
