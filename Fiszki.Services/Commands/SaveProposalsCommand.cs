using Fiszki.Services.Models.Generation;

namespace Fiszki.Services.Commands;

public record SaveProposalsCommand
{
    public required IReadOnlyList<FlashcardProposalDto> Proposals { get; init; }
}
