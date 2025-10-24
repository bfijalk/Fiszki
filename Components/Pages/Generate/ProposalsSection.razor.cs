using Microsoft.AspNetCore.Components;
using Fiszki.Services.Models.Generation;

namespace Fiszki.Components.Pages.Generate;

public partial class ProposalsSection
{
    [Parameter, EditorRequired] public IReadOnlyList<FlashcardProposalViewModel> Proposals { get; set; } = null!;
    [Parameter, EditorRequired] public SelectionStats SelectionStats { get; set; } = null!;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnBulkAccept { get; set; }
    [Parameter] public EventCallback OnBulkReject { get; set; }
    [Parameter] public EventCallback OnSaveSelected { get; set; }
    [Parameter] public EventCallback<FlashcardProposalViewModel> OnEdit { get; set; }
    
    private Task OnEditProposal(FlashcardProposalViewModel proposal)
        => OnEdit.InvokeAsync(proposal);
        
    private void OnToggleReject(FlashcardProposalViewModel proposal)
    {
        proposal.Rejected = !proposal.Rejected;
        if (proposal.Rejected)
        {
            proposal.Selected = false;
        }
    }
}
