namespace Fiszki.Services.Commands;

public record CreateFlashcardCommand(string FrontContent, string BackContent, IReadOnlyCollection<string>? Tags);

