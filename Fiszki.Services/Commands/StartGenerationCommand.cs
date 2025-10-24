namespace Fiszki.Services.Commands;

public record StartGenerationCommand
{
    public required string SourceText { get; init; }
    public required string Language { get; init; }
    public required int MaxCards { get; init; }
}
