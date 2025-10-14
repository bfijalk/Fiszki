using Fiszki.Services.Models.Generation;

namespace Fiszki.Components.Pages.Generate;

public class GenerateFlashcardsViewState
{
    public SourceInputModel Source { get; set; } = new();
    public GenerationStatusEnum Status { get; set; } = GenerationStatusEnum.Idle;
    public string? ErrorMessage { get; set; }
    public List<FlashcardProposalViewModel> Proposals { get; set; } = new();
    public Guid? ActiveProposalId { get; set; }
    public bool IsSaving { get; set; }
    
    public bool HasUnsavedEdits => Proposals.Any(p => p.Selected && !p.Rejected);
}
