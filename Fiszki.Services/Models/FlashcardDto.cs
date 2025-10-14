namespace Fiszki.Services.Models;

public record FlashcardDto(
    Guid Id,
    string FrontContent,
    string BackContent,
    string CreationSource,
    string? AiModel,
    IReadOnlyCollection<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

