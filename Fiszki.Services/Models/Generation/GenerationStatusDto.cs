namespace Fiszki.Services.Models.Generation;

public record GenerationStatusDto(
    Guid JobId,
    GenerationStatusEnum Status,
    int ProposalsGenerated,
    IReadOnlyCollection<FlashcardProposalDto>? Proposals,
    string? Error
);
