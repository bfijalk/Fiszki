using Fiszki.Services.Commands;
using Fiszki.Services.Models;

namespace Fiszki.Services.Interfaces;

public interface IFlashcardService
{
    Task<FlashcardDto> CreateAsync(Guid userId, CreateFlashcardCommand command, CancellationToken ct = default);
    Task<FlashcardDto?> GetAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
    Task<IReadOnlyCollection<FlashcardDto>> ListAsync(Guid userId, int skip = 0, int take = 100, CancellationToken ct = default);
    Task<FlashcardDto> UpdateAsync(Guid userId, UpdateFlashcardCommand command, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid flashcardId, CancellationToken ct = default);
}

