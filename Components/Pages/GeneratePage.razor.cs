using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing; // For LocationChangingEventArgs
using Microsoft.AspNetCore.Components.Authorization; // added for AuthenticationStateProvider
using Fiszki.Services.Commands;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Models.Generation; // Generation models
using MudBlazor;
using Fiszki.Components.Pages;
using Fiszki.Components.Pages.Generate; // Support view models & helpers
using System.Security.Claims; // added for user id claim

namespace Fiszki.Components.Pages; // Reverted to match .razor generated namespace

public partial class GeneratePage : IDisposable
{
    [Inject] private IGenerationService GenerationService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!; // new
    
    private GenerateFlashcardsViewState _state = new();
    private CancellationTokenSource? _pollCts;
    private bool _isNavigating;
    
    protected override void OnInitialized()
    {
        //NavigationManager.LocationChanging += OnLocationChanging;
    }
    
    // private async void OnLocationChanging(object? sender,  LocationChangingEventArgs e)
    // {
    //     if (_isNavigating || !_state.HasUnsavedEdits)
    //     {
    //         return;
    //     }
    //
    //     e.PreventNavigation();
    //     
    //     var parameters = new DialogParameters();
    //     var dialog = await DialogService.ShowAsync<UnsavedChangesDialog>("Unsaved Changes", parameters);
    //     var result = await dialog.Result;
    //
    //     if (!result.Canceled)
    //     {
    //         _isNavigating = true;
    //         NavigationManager.NavigateTo(e.DestinationLocation);
    //     }
    // }
    //
    private async Task HandleGenerateSubmit()
    {
        try
        {
            _state.Status = GenerationStatusEnum.Queued;
            _state.ErrorMessage = null;
            
            var command = new StartGenerationCommand
            {
                SourceText = _state.Source.Text,
                Language = _state.Source.Language,
                MaxCards = _state.Source.MaxCards
            };
            
            var job = await GenerationService.StartAsync(command);
            _pollCts = new CancellationTokenSource();
            
            await PollGenerationStatus(job.JobId, _pollCts.Token);
        }
        catch (Exception ex)
        {
            _state.Status = GenerationStatusEnum.Failed;
            _state.ErrorMessage = ex.Message;
        }
    }
    
    private async Task PollGenerationStatus(Guid jobId, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var status = await GenerationService.GetStatusAsync(jobId, ct);
                
                _state.Status = status.Status;
                if (status.Proposals != null)
                {
                    _state.Proposals = status.Proposals.Select(FlashcardProposalViewModel.FromDto).ToList();
                }
                
                if (status.Status is GenerationStatusEnum.Completed or GenerationStatusEnum.Failed)
                {
                    break;
                }
                
                await Task.Delay(1500, ct);
            }
        }
        catch when (ct.IsCancellationRequested)
        {
            _state.Status = GenerationStatusEnum.Canceled;
        }
        catch (Exception ex)
        {
            _state.Status = GenerationStatusEnum.Failed;
            _state.ErrorMessage = ex.Message;
        }
    }
    
    private SelectionStats GetSelectionStats() => new(
        _state.Proposals.Count,
        _state.Proposals.Count(p => p.Selected),
        _state.Proposals.Count(p => p.Accepted),
        _state.Proposals.Count(p => p.Rejected),
        _state.Proposals.Count(p => !p.Rejected)
    );
        
    private Task HandleSourceChange(SourceInputModel model)
    {
        _state.Source = model;
        return Task.CompletedTask;
    }
    
    private void HandleCancelGeneration()
    {
        _pollCts?.Cancel();
        _state.Status = GenerationStatusEnum.Canceled;
    }

    private void HandleRetry()
    {
        _state.Status = GenerationStatusEnum.Idle;
        _state.ErrorMessage = null;
        _state.Proposals.Clear();
    }

    private void HandleBulkAccept()
    {
        foreach (var proposal in _state.Proposals.Where(p => !p.Rejected))
        {
            proposal.Selected = true;
            proposal.Accepted = true;
        }
    }

    private void HandleBulkReject()
    {
        foreach (var proposal in _state.Proposals.Where(p => !p.Rejected))
        {
            proposal.Selected = false;
            proposal.Rejected = true;
        }
    }

    private async Task HandleSaveSelected()
    {
        try
        {
            _state.IsSaving = true;
            var selectedProposals = _state.Proposals
                .Where(p => p.Selected && !p.Rejected)
                .Select(p => p.ToDto())
                .ToList();

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                await DialogService.ShowMessageBox(
                    "Error",
                    "Unable to determine current user. Please re-login.",
                    yesText: "OK");
                return;
            }

            var command = new SaveProposalsCommand { Proposals = selectedProposals };
            await GenerationService.SaveProposalsAsync(userId, command); // pass real user id

            _state.Proposals.Clear();
            _state.Status = GenerationStatusEnum.Idle;
            
            await DialogService.ShowMessageBox(
                "Success",
                "Selected flashcards have been saved successfully.",
                yesText: "OK");
        }
        catch (Exception ex)
        {
            await DialogService.ShowMessageBox(
                "Error",
                $"Failed to save flashcards: {ex.Message}",
                yesText: "OK");
        }
        finally
        {
            _state.IsSaving = false;
        }
    }

    private Task HandleProposalEdit(FlashcardProposalViewModel proposal)
    {
        _state.ActiveProposalId = proposal.Id;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void HandleCloseEditDialog()
    {
        _state.ActiveProposalId = null;
    }

    private void HandleProposalSave(FlashcardProposalViewModel editedProposal)
    {
        var existingProposal = _state.Proposals.First(p => p.Id == editedProposal.Id);
        existingProposal.Front = editedProposal.Front;
        existingProposal.Back = editedProposal.Back;
        existingProposal.Example = editedProposal.Example;
        existingProposal.Notes = editedProposal.Notes;
        existingProposal.Selected = true;
        existingProposal.Accepted = true;
        existingProposal.Rejected = false;
        
        _state.ActiveProposalId = null;
    }
    
    public void Dispose()
    {
        //NavigationManager.LocationChanging -= OnLocationChanging;
        _pollCts?.Cancel();
        _pollCts?.Dispose();
    }
}
