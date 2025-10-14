namespace Fiszki.Components.Pages.Generate;

public record SourceInputModel(
    string Text = "",
    string Language = "en",
    int MaxCards = GenerationValidation.DefaultMaxCards
);
