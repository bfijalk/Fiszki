# View Implementation Plan: Generate Flashcards

## 1. Overview
The Generate Flashcards view enables authenticated users to submit source study text (articles, lecture notes, etc.) and receive AI-generated flashcard proposals. Users can iteratively refine, review, edit, select, and persist chosen flashcards to their personal collection. The view emphasizes accessibility, robust validation, responsive feedback, and resilience against latency and transient API failures.

Primary goals:
- Accept large textual input (1,000–10,000 chars) with validation and UX guidance.
- Initiate generation via backend AI endpoint and stream or poll proposals.
- Present proposals in an actionable list with Accept / Reject / Edit flows.
- Support bulk actions (Accept All, Reject All, Save Selected).
- Provide an in-place modal dialog for detailed editing/tagging per proposal.
- Persist accepted proposals to the Flashcards store and surface success/errors.

## 2. View Routing
Route: `/generate`  
- Protected route: requires authentication (`[Authorize]`).  
- Included in `NavMenu` after login/registration redirect.

## 3. Component Structure
Hierarchy:
```
<GenerateFlashcardsPage>
  <PageTitle />
  <SourceInputForm>
    <SourceTextArea />
    <GenerationControls />
  </SourceInputForm>
  <GenerationStatusBar />
  <ProposalsSection>
    <BulkActionsBar />
    <ProposalList>
      <ProposalCard /> * N
    </ProposalList>
  </ProposalsSection>
  <LoadingOverlay />
  <EmptyState />
  <ErrorState />
  <ProposalReviewDialog /> (MudDialog)
  <UnsavedChangesDialog />
</GenerateFlashcardsPage>
```
Supporting components: `TagInput`, `InlineValidationMessage`, `RetryErrorPanel`.

## 4. Component Details
### GenerateFlashcardsPage
- Description: Top-level page orchestrating state, API calls, and child component wiring.
- Main elements: Layout container (`MudPaper` / div grid), child components, toast triggers.
- Handled interactions: Generate, cancel generation, polling lifecycle, accept/reject/edit/save proposals, bulk actions, retry, navigation guard.
- Handled validation: Delegates text + proposal edits; enforces save preconditions (at least one accepted & selected).
- Types: `GenerateFlashcardsViewState`, `GenerateFlashcardsRequest`, `GenerationProgress`, `ProposalVM`, `SaveFlashcardsRequest`.
- Props: None (page).

### SourceInputForm
- Description: Collects source text and optional parameters.
- Main elements: `MudForm`, multiline `MudTextField` (with char counter), optional `MudSelect` (language), `MudNumericField` (max cards).
- Handled interactions: Change (debounced), submit, clear.
- Handled validation: Length (1000–10000), non-empty trimmed, language pattern `^[a-z]{2}(-[A-Z]{2})?$`, max cards 1–100.
- Types: `SourceInputModel`.
- Props: `{ Value, OnChange(SourceInputModel), OnSubmit(), IsGenerating }`.

### GenerationControls
- Description: Action buttons for generation lifecycle.
- Main elements: Generate, Cancel, Clear buttons.
- Handled interactions: Generate click, cancel click (if in-flight), clear click.
- Validation: Disabled states (invalid form, generating, empty).
- Types: None special.
- Props: `{ CanGenerate, IsGenerating, OnGenerate, OnCancel, OnClear }`.

### GenerationStatusBar
- Description: Shows current progress/status and retry option.
- Main elements: `MudProgressLinear`, status text, retry button (on failure).
- Handled interactions: Retry.
- Validation: N/A (read-only).
- Types: `GenerationProgress`, `GenerationStatusEnum`.
- Props: `{ Status, ErrorMessage?, OnRetry }`.

### ProposalsSection
- Description: Wrapper for proposal-related UI.
- Main elements: `BulkActionsBar`, `ProposalList`, state placeholders (empty/error).
- Handled interactions: Bulk events bubbled up.
- Validation: Disables actions while saving or none available.
- Types: `ProposalVM[]`, `SelectionStats`.
- Props: `{ Proposals, SelectionStats, Disabled, OnBulkAccept, OnBulkReject, OnSaveSelected }`.

### BulkActionsBar
- Description: Global list operations.
- Main elements: Select All checkbox, Accept/Reject Selected, Accept All, Reject All, Save Selected, counts display.
- Handled interactions: All bulk button clicks, toggle select all.
- Validation: Disable based on counts & status.
- Types: `SelectionStats`.
- Props: `{ Stats, Disabled, OnSelectAll(bool), OnBulkAccept, OnBulkReject, OnSaveSelected, OnAcceptAll, OnRejectAll }`.

### ProposalList
- Description: Virtualized container for proposals.
- Main elements: `Virtualize` / list wrapper.
- Handled interactions: Emits card actions upward.
- Validation: N/A (delegated to cards).
- Types: `ProposalVM`, `ProposalActionEvent`.
- Props: `{ Proposals, OnAction(ProposalActionEvent) }`.

### ProposalCard
- Description: Displays single proposal with interaction controls.
- Main elements: `MudCard`, preview front/back, tag chips, accept/reject/edit buttons, selection checkbox, status badge.
- Handled interactions: Accept, Reject, Edit, Select toggle.
- Validation: Mutual exclusivity (accept vs reject).
- Types: `ProposalVM`.
- Props: `{ Proposal, Disabled, OnAccept(id), OnReject(id), OnEdit(id), OnToggleSelect(id) }`.

### ProposalReviewDialog
- Description: Modal for editing a proposal.
- Main elements: `MudDialog`, `MudForm`, front/back fields, `TagInput`, Save/Cancel buttons.
- Handled interactions: Field edits, tag edits, save, cancel (with dirty confirm).
- Validation: Front/back length 1–300, non-whitespace; tag rules (<=10, 1–24 chars, alphanumeric + '-').
- Types: `ProposalEditModel`.
- Props: `{ Proposal, OnSave(ProposalEditModel), OnCancel }` (via dialog parameters).

### TagInput
- Description: Controlled tags entry component.
- Main elements: Tag chips, input field, optional add button.
- Handled interactions: Add on Enter, remove chip, duplicate detection.
- Validation: Count, uniqueness, pattern.
- Types: `TagListModel` (implicit: string[]).
- Props: `{ Value: string[], OnChange(string[]), Max }`.

### LoadingOverlay
- Description: Overlay while generation is active.
- Main elements: `MudOverlay`, spinner (`MudProgressCircular`), optional cancel.
- Handled interactions: Cancel (optional).
- Validation: N/A.
- Props: `{ Visible, OnCancel? }`.

### EmptyState
- Description: Instructional placeholder.
- Main elements: Icon + text.
- Props: None.

### ErrorState / RetryErrorPanel
- Description: Displays critical generation failure.
- Main elements: `MudAlert`, Retry button.
- Props: `{ Message, OnRetry }`.

### InlineValidationMessage
- Description: Standard error text wrapper.
- Props: `{ For, Message }`.

### UnsavedChangesDialog
- Description: Confirms abandoning unsaved accepted proposals.
- Props: Standard confirm callbacks.

## 5. Types
Backend / DTO (assumed or to be created):
- GenerateFlashcardsRequest { string SourceText; string? Language; int? MaxCards; }
- GenerateFlashcardsResponse { string JobId; DateTime QueuedAt; }
- GenerationStatusResponse { string JobId; GenerationStatusEnum Status; int ProposalsGenerated; List<FlashcardProposalDto>? Proposals; string? Error; }
- FlashcardProposalDto { string Id; string Front; string Back; List<string> Tags; double? ConfidenceScore; }
- SaveFlashcardsRequest { List<FlashcardCreateDto> Flashcards; }
- FlashcardCreateDto { string Front; string Back; List<string> Tags; string Source; }
- SaveFlashcardsResponse { int SavedCount; List<string> FlashcardIds; }
- ApiError { string Code; string Message; Dictionary<string,string[]>? FieldErrors; }

UI / ViewModel:
- GenerationStatusEnum { Idle, Queued, Generating, Completed, Failed, Canceled }
- ProposalStateEnum { Pending, Accepted, Rejected, Edited }
- ProposalVM { string Id; string Front; string Back; List<string> Tags; bool Accepted; bool Rejected; bool Selected; bool Edited; double? ConfidenceScore; DateTime ReceivedAt; }
- ProposalEditModel { string Front; string Back; List<string> Tags; }
- SourceInputModel { string Text; string? Language; int? MaxCards; }
- SelectionStats { int Total; int SelectedCount; int AcceptedCount; int RejectedCount; int EditableCount; }
- GenerateFlashcardsViewState { SourceInputModel Source; GenerationStatusEnum Status; List<ProposalVM> Proposals; bool IsSaving; string? ErrorMessage; string? ActiveProposalId; bool HasUnsavedEdits; }
- ProposalActionEvent { string ProposalId; string Action; }
- GenerationProgress { GenerationStatusEnum Status; int Count; string? Error; }

Helper:
- GenerationValidation (static): methods for source length, proposal field constraints, tag validation, save preconditions.

## 6. State Management
Centralized in page-level code-behind:
- Single `GenerateFlashcardsViewState` instance.
- Derived/computed selectors: `SelectionStats`, `CanGenerate`, `CanSaveSelected`.
- Proposal operations update list immutably or mutate followed by `StateHasChanged()`.
- Polling loop with `CancellationTokenSource`.
- Debounced source text updates (Timer or simple set + Task.Delay pattern).
- Navigation guard if `HasUnsavedEdits`.

Services:
- `IGenerationService` for start, status polling, cancel, save bulk.
- Optional `IBackoffPolicy` for retry timing (simple exponential inside service).

## 7. API Integration (Aligned with Fiszki.Services Architecture)
Current service layer (per existing `IFlashcardService`, `IUserService`, DI registration) uses:
- Command objects (e.g., `CreateFlashcardCommand`, `UpdateFlashcardCommand`).
- DTO response models (e.g., `FlashcardDto`, `UserDto`).
- FluentValidation validators registered in DI.
- Service interfaces abstracting data/domain operations.

To integrate generation consistently:
A. Introduce new Commands (in `Fiszki.Services/Commands`):
- `StartGenerationCommand { string SourceText; string? Language; int? MaxCards; Guid UserId; }`
- `SaveGeneratedFlashcardsCommand { Guid UserId; IReadOnlyCollection<GeneratedFlashcardSelection> Flashcards; }`
- `GeneratedFlashcardSelection { string Front; string Back; IReadOnlyCollection<string> Tags; string Source; }`

B. Introduce new DTOs (in `Fiszki.Services/Models`):
- `GenerationJobDto { Guid JobId; DateTime QueuedAt; GenerationStatusEnum Status; }`
- `GenerationStatusDto { Guid JobId; GenerationStatusEnum Status; int ProposalsGenerated; IReadOnlyCollection<FlashcardProposalDto>? Proposals; string? Error; }`
- `FlashcardProposalDto { Guid Id; string Front; string Back; IReadOnlyCollection<string> Tags; double? ConfidenceScore; }` (Guid for consistency with existing `FlashcardDto` IDs)

C. Introduce Service Interface (in `Interfaces`):
```
public interface IGenerationService
{
    Task<GenerationJobDto> StartAsync(StartGenerationCommand command, CancellationToken ct = default);
    Task<GenerationStatusDto> GetStatusAsync(Guid jobId, Guid userId, CancellationToken ct = default);
    Task CancelAsync(Guid jobId, Guid userId, CancellationToken ct = default);
    Task<int> SaveSelectedAsync(SaveGeneratedFlashcardsCommand command, CancellationToken ct = default);
}
```

D. Validators (in `Validation`):
- `StartGenerationCommandValidator`: source length, language pattern, max cards range.
- `SaveGeneratedFlashcardsCommandValidator`: at least one flashcard, per-flashcard front/back/tag rules.

E. DI Registration (`DependencyInjection`):
```
services.AddScoped<IGenerationService, GenerationService>();
services.AddScoped<IValidator<StartGenerationCommand>, StartGenerationCommandValidator>();
services.AddScoped<IValidator<SaveGeneratedFlashcardsCommand>, SaveGeneratedFlashcardsCommandValidator>();
```

F. UI Consumption Pattern:
1. On Generate button:
   - Map `SourceInputModel` -> `StartGenerationCommand` (include current userId from auth context/claim).
   - Call `StartAsync` to receive `GenerationJobDto`.
   - Enter polling loop calling `GetStatusAsync(jobId, userId)` until status terminal (Completed/Failed/Canceled).
2. Polling merges `FlashcardProposalDto` into local `ProposalVM` list (mapping extension in `Mapping/ServiceMapping.cs`).
3. Cancel generation: call `CancelAsync(jobId, userId)` then set local status to Canceled.
4. Save Selected:
   - Build `SaveGeneratedFlashcardsCommand` from accepted & selected `ProposalVM` entries.
   - Call `SaveSelectedAsync` -> returns count saved.
   - Remove or flag saved proposals; toast success.

G. Error Surface:
- Service methods throw domain exceptions (`DomainNotFoundException`, `ConflictException`, `UnauthorizedDomainException`) already present; catch in page and translate to user-friendly messages.
- Validation failures surfaced as `FluentValidation.ValidationException`; map field errors to UI.

H. Abstraction vs Http:
- The page never handles raw `HttpClient` calls; it only depends on `IGenerationService` (matching existing layering approach). The actual implementation may internally use EFCore, external AI API, or hosted background job—UI remains decoupled.

I. Mapping Functions:
- Add extension `ToProposalVM(this FlashcardProposalDto dto)` in `Mapping/ServiceMapping.cs`.
- Reverse mapping not required (proposals are edited locally then converted to `GeneratedFlashcardSelection`).

J. Status Enum Reuse:
- Ensure `GenerationStatusEnum` defined once in shared Models; UI uses same enum (avoid duplicate definitions). Add values: Idle, Queued, Generating, Completed, Failed, Canceled.

K. Concurrency & Polling:
- Poll interval constant (e.g., 1500 ms) inside page logic; stop on terminal state or after max attempts (timeout -> treat as Failed with error message).
- Provide cancellation via `CancellationTokenSource` passed into `GetStatusAsync` calls.

L. Security:
- Service methods accept `userId` to enforce multi-tenant isolation; extracted from authenticated claims principal in page or injected `IUserContextService` if available.

Result: Section 7 now reflects integration at the service abstraction layer consistent with existing `Fiszki.Services` patterns, minimizing direct endpoint coupling.

## 8. User Interactions
- Input typing/paste: live character count; invalid states visually indicated.
- Generate click: starts request; disables input; shows overlay + progress.
- Cancel generation: stops polling; retains existing proposals; status updates to Canceled.
- Accept/Reject proposal: toggles mutually exclusive flags; updates selection if previously selected and rejected (optional unselect logic).
- Edit proposal: opens dialog; saving sets Edited=true; updates text/tags.
- Select proposal: marks for bulk operations and eventual save.
- Bulk Accept/Reject Selected: apply state change to selected subset.
- Accept All / Reject All: operate on all proposals.
- Save Selected: persists only selected & accepted (rule).
- Retry: clears proposals (with confirm if unsaved accepted) and restarts generation.
- Navigation attempt with unsaved accepted: prompt confirm.
- Tag add/remove in dialog: validated inline.
- Keyboard: Escape closes dialog, Enter submits inside dialog if valid, Tab order logical.

## 9. Conditions and Validation
- Source text length: 1000–10000 inclusive.
- Source not whitespace-only.
- Optional language code pattern.
- MaxCards optional: 1–100.
- Proposal front/back: non-empty trimmed, 1–300 chars.
- Tags: <=10, each 1–24 chars, alphanumeric or hyphen, case-insensitive uniqueness.
- Cannot both accept and reject a proposal.
- Save disabled when none accepted & selected.
- Cancel only while Queued/Generating.
- Retry only when Failed or Completed (to start over).
- Navigation blocked when unsaved accepted proposals exist.

## 10. Error Handling
Scenarios & responses:
- Generation request validation (400 equivalent via validator): surface field errors inline (text area, language).
- Payload too large (if enforced): specific message and highlight length counter.
- Unauthorized: redirect to `/login` (global handler).
- Rate limit (if service signals): toast + disable Generate for backoff duration.
- Poll transient failures: retry up to 3 times, then mark Failed with retry option.
- Network/offline (if surfaced): show offline toast; manual retry.
- Partial save success: mark saved proposals removed; unsaved remain with error badge and tooltip.
- Cancel failure: log + local state treat as canceled.
- Dialog validation errors: block close, focus first invalid field.
- Unhandled exception: fallback `ErrorBoundary` display.

## 11. Implementation Steps
1. Create page `Pages/Generate.razor` with route and `[Authorize]`.
2. Add partial class `Generate.razor.cs` for state, injection, handlers.
3. Define generation commands & DTOs (`StartGenerationCommand`, `SaveGeneratedFlashcardsCommand`, `GenerationJobDto`, `GenerationStatusDto`, `FlashcardProposalDto`, enum) in Services project.
4. Implement validators for new commands; register in DI.
5. Add `IGenerationService` interface & `GenerationService` implementation; register in DI.
6. Implement mapping extensions `FlashcardProposalDto -> ProposalVM`.
7. Build `GenerationValidation` static utility for UI-only quick checks.
8. Implement `SourceInputForm` component with DataAnnotations & char counter.
9. Add `GenerationControls` component (buttons & disabled logic).
10. Create `GenerationStatusBar` with progress + retry display and aria-live region.
11. Implement `ProposalCard` (UI, accessibility labels, action callbacks).
12. Implement `ProposalList` with virtualization.
13. Add `BulkActionsBar` computing & displaying stats.
14. Build `ProposalReviewDialog` with form state copy + validation.
15. Implement `TagInput` for controlled tag editing.
16. Add `LoadingOverlay`, `EmptyState`, `ErrorState` components.
17. Wire generation flow via `IGenerationService`: start, poll loop, cancel.
18. Implement accept/reject/edit/select logic & derived `SelectionStats`.
19. Implement save selected flow with `SaveGeneratedFlashcardsCommand`.
20. Add unsaved changes guard leveraging `NavigationManager.LocationChanging`.
21. Integrate toasts for major lifecycle events (started, completed, saved, errors).
22. Add CSS / MudBlazor styling adjustments & responsive layout.
23. Add accessibility features (aria-live, focus management, descriptive aria-labels).
24. Add unit tests for validation & mapping (if test infra exists).
25. Manual QA: boundary lengths, rapid generate/cancel, retry after failure, partial save errors, navigation guard.
26. Performance tuning (virtualization, reduce re-renders using `@key`).
27. Code review & finalize documentation comments.

---
This plan outlines all components, types, flows, and error-handling strategies required to implement the Generate Flashcards view aligned with the service-layer architecture.
