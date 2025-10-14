namespace Fiszki.Services.Commands;

public record UpdateFlashcardCommand(Guid FlashcardId, string FrontContent, string BackContent, IReadOnlyCollection<string>? Tags, DateTime? ExpectedUpdatedAtUtc);

