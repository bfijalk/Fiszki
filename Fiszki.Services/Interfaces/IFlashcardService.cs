using Fiszki.Services.Commands;
using Fiszki.Services.Models;

namespace Fiszki.Services.Interfaces;

public interface IFlashcardService
{
    Task<FlashcardDto> CreateAsync(Guid userId, CreateFlashcardCommand command, CancellationToken ct = default);
    Task<FlashcardDto?> GetAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
    /// <summary>Lists the user's flashcards, newest first (archived ones: most recently archived first).</summary>
    Task<IReadOnlyCollection<FlashcardDto>> ListAsync(Guid userId, int skip = 0, int take = 100, FlashcardListState state = FlashcardListState.Active, CancellationToken ct = default);
    /// <summary>Counts over all of the user's flashcards, archived or not, with the learning progress.</summary>
    Task<FlashcardSummaryDto> GetSummaryAsync(Guid userId, CancellationToken ct = default);
    /// <summary>Marks the user's flashcard as learned. Does nothing if it is missing, not the user's or already archived.</summary>
    Task ArchiveAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
    /// <summary>Returns an archived flashcard to the active list. Does nothing if it is missing, not the user's or not archived.</summary>
    Task RestoreAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
    Task<FlashcardDto> UpdateAsync(Guid userId, UpdateFlashcardCommand command, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
}

