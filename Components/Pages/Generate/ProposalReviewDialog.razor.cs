using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fiszki.Components.Pages.Generate;

public partial class ProposalReviewDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public FlashcardProposalViewModel Proposal { get; set; } = null!;
    
    private MudForm _form = null!;
    private FlashcardProposalViewModel _model = null!;
    
    protected override void OnParametersSet()
    {
        _model = Proposal with { };
    }
    
    private void Submit()
    {
        if (_form.IsValid)
        {
            MudDialog.Close(DialogResult.Ok(_model));
        }
    }
    
    private void Cancel() => MudDialog.Cancel();
    
    private string ValidateFront(string value)
        => string.IsNullOrWhiteSpace(value) 
            ? "Front side is required" 
            : string.Empty;
            
    private string ValidateBack(string value)
        => string.IsNullOrWhiteSpace(value) 
            ? "Back side is required" 
            : string.Empty;
}
