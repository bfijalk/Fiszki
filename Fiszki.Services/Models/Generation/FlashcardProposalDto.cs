namespace Fiszki.Services.Models.Generation;

public record FlashcardProposalDto(
    Guid Id,
    string Front,
    string Back,
    string? Example,
    string? Notes,
    bool IsAccepted = false,
    bool IsRejected = false
);
