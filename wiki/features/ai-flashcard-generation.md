---
title: AI flashcard generation
type: feature
summary: How a logged-in user generates flashcard proposals with AI on the Generate page, reviews, edits and rejects them, and saves the accepted ones as flashcards; limits, states, fallback behavior and what is not saved.
tags: [generation, generate, ai, proposals, flashcards, openrouter, source-text, language, max-cards, accept-all, reject, save-selected, review, edit, fallback, placeholder]
sources:
  - Fiszki.FunctionalTests/Features/FlashcardGeneration.feature
  - Components/Pages/GeneratePage.razor
  - Components/Pages/GeneratePage.razor.cs
  - Components/Pages/Generate/SourceInputForm.razor
  - Components/Pages/Generate/SourceInputForm.razor.cs
  - Components/Pages/Generate/ProposalsSection.razor
  - Components/Pages/Generate/ProposalsSection.razor.cs
  - Components/Pages/Generate/ProposalReviewDialog.razor
  - Components/Pages/Generate/FlashcardProposalViewModel.cs
  - Components/Pages/Generate/GenerateFlashcardsViewState.cs
  - Components/Pages/Generate/GenerationValidation.cs
  - Components/Pages/Generate/ErrorState.razor
  - Components/Pages/Generate/LoadingOverlay.razor
  - Fiszki.Services/Interfaces/IGenerationService.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Commands/StartGenerationCommand.cs
  - Fiszki.Services/Commands/SaveProposalsCommand.cs
  - Fiszki.Services/Validation/StartGenerationCommandValidator.cs
  - Fiszki.Services/Validation/SaveProposalsCommandValidator.cs
  - Fiszki.Services/Models/Generation/FlashcardProposalDto.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
  - Fiszki.Tests/Services/GenerationServiceTests.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
  - Fiszki.FunctionalTests/Steps/FlashcardSteps.cs
  - Fiszki.FunctionalTests/PageObjects/FlashcardGenerationPage.cs
  - README.md
  - Components/Pages/AboutTechnicalities.razor
verified_commit: 52f627aae9f0b25fa16da531aa2d9e66b421bcb9
confidence: high
---

# AI flashcard generation

## Summary

On the Generate page (`/generate`) a logged-in user pastes a text, picks a language and a
maximum number of cards, and starts generation. The AI returns flashcard **proposals**. The
user reviews them (accept all, reject, edit), then saves the accepted ones, which become
regular flashcards with creation source `Ai`. How the AI is called is in
[[openrouter-integration]]; how a generation job runs in the background is in
[[generation-job-flow]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

### Input

1. **Login is required.** Only authenticated users can open the page. Backed by: code only
   (`Components/Pages/GeneratePage.razor` `Authorize`).
2. **Source text must be 50 to 5000 characters.** The form disables **Generate Flashcards**
   until at least 50 characters are typed and stops typing at 5000; the service enforces the
   same limits. Backed by: unit test for the minimum
   (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `StartGenerationCommandValidatorTests`),
   code for the maximum
   (`Fiszki.Services/Validation/StartGenerationCommandValidator.cs` `StartGenerationCommandValidator`).
3. **Language is one of `en`, `pl`, `es`, `de`, `fr`** (English, Polish, Spanish, German,
   French); the default is `en`. An unsupported code is rejected. Backed by: unit test
   (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `StartGenerationCommandValidatorTests`).
   **The chosen language does not change what is generated**, see "Behavior worth knowing".
4. **Maximum cards is 1 to 50; the default is 20.** Typing a value outside the range in the
   form clamps it to 1 or 50; the service rejects values outside the range. Backed by:
   unit test for 0 (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `StartGenerationCommandValidatorTests`),
   Gherkin sets it to 5
   (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`, scenario "Generate flashcards and validate they appear on generation page").
5. **The form is locked while generating.** The text, language and cards fields and the
   Generate button are disabled during generation. Backed by: code only
   (`Components/Pages/Generate/SourceInputForm.razor` `IsGenerating`).

### Generation

6. **Generation runs in the background and the page shows its state.** The page polls about
   every 1.5 seconds and shows "Preparing to generate flashcards...", "Generating
   flashcards...", "Generation completed", "Generation failed" or "Generation canceled".
   Backed by: Gherkin for "Generation completed"
   (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`, scenario "Generate flashcards and validate they appear on generation page");
   code for the polling (`Components/Pages/GeneratePage.razor.cs` `PollGenerationStatus`).
7. **The AI is asked for at most the requested number of cards.** The request tells the model
   to produce exactly the requested count, and the response schema allows 1 up to that count.
   The code does not check how many cards came back. Backed by: code only
   (`Fiszki.Services/Services/GenerationService.cs` `GenerateFlashcardsWithOpenRouter`).
8. **Each proposal has a front, a back and an example.** The AI writes all three; notes start
   empty. Backed by: code only
   (`Fiszki.Services/Models/Generation/FlashcardProposalDto.cs` `FlashcardProposalDto`).
9. **If the AI call fails, the user sees placeholder cards, not an error.** Any failure of the
   AI call (no or wrong API key, network, rate limit, bad response) is swallowed and replaced
   by generated placeholders ("Polish Word 1", "English Translation 1", "Example sentence 1
   in Polish with English translation", and so on), the job still **completes**, and the page
   says "Generation completed". Backed by: a unit test that accepts either outcome
   (`Fiszki.Tests/Services/GenerationServiceTests.cs` `StartAsync_WhenOpenRouterFails_ShouldFallbackToMockGeneration`
   passes for a completed job with placeholders or a failed job) and code (`Fiszki.Services/Services/GenerationService.cs` `GenerateMockProposals`).
10. **Cancel stops the waiting, not the work.** "Cancel Generation" stops polling and shows
    "Generation canceled". The page does not tell the service to cancel, so the AI call still
    runs to completion in the background. Backed by: code only
    (`Components/Pages/GeneratePage.razor.cs` `HandleCancelGeneration`).

### Review

11. **Proposals appear under "Generated Flashcards (X of Y selected)".** Nothing is selected
    at first. Backed by: Gherkin
    (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`, scenario "Generate flashcards and validate they appear on generation page")
    for the header and count; selection state from
    `Components/Pages/Generate/FlashcardProposalViewModel.cs` `FromDto`.
12. **There is no way to accept a single card directly.** The review controls are:
    **Accept All** (selects and accepts every card that is not rejected), **Reject All** (rejects
    every card that is not already rejected), per-card **Reject** (X) with **Undo** (which restores the card but leaves it unselected), and per-card
    **Edit**. Editing and saving a card also accepts it. To keep only some cards, use Accept All
    and then reject the unwanted ones. Backed by: Gherkin for Accept All
    (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`, scenario "Generate flashcards and validate they appear on generation page");
    code for the rest (`Components/Pages/GeneratePage.razor.cs` `HandleBulkAccept`,
    `Components/Pages/GeneratePage.razor.cs` `HandleBulkReject`,
    `Components/Pages/Generate/ProposalsSection.razor.cs` `OnToggleReject`).
13. **Editing needs a front and a back.** The edit dialog has Front and Back (required) and
    Example and Notes (optional); **Save Changes** is disabled while front or back is empty.
    A rejected card cannot be edited until the rejection is undone. Backed by: code only
    (`Components/Pages/Generate/ProposalReviewDialog.razor` `CanSave`).
14. **Save Selected needs at least one selected card.** The button shows the count, for example
    "Save Selected (5)", and is disabled at zero or while saving. Backed by: Gherkin for enabled
    and count after Accept All
    (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`, scenario "Generate flashcards and validate they appear on generation page").

### Saving

15. **Only accepted, non-rejected proposals are saved.** Backed by: unit test
    (`Fiszki.Tests/Services/GenerationServiceTests.cs` `SaveProposalsAsync_WithValidCommand_ShouldSaveAcceptedProposals`,
    `Fiszki.Tests/Services/GenerationServiceTests.cs` `SaveProposalsAsync_WithNoAcceptedProposals_ShouldReturnEmptyList`).
16. **Saved cards are AI flashcards owned by the current user, with front and back only.**
    Example, notes, the AI model name and tags are not stored, so the example the user saw during
    review is lost. Backed by: unit test for ownership and `Ai` source
    (`Fiszki.Tests/Services/GenerationServiceTests.cs` `SaveProposalsAsync_WithValidCommand_ShouldSaveAcceptedProposals`);
    code for what is dropped (`Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`). See [[flashcard-entity]].
17. **Save validation.** At least one proposal must be accepted; front and back are required and
    at most 500 characters; example and notes are at most 1000 characters. The edit dialog does
    not limit length, so an over-long text is only caught when saving, and the user sees
    "Failed to save flashcards: Validation failed". Backed by: code only
    (`Fiszki.Services/Validation/SaveProposalsCommandValidator.cs` `SaveProposalsCommandValidator`).
18. **After a successful save the page resets.** The proposals are cleared, the page returns to
    its starting state and the message "Selected flashcards have been saved successfully." is
    shown. Backed by: code only
    (`Components/Pages/GeneratePage.razor.cs` `HandleSaveSelected`). The Gherkin scenario stops
    before saving, see Test coverage.
19. **Saving does not change the user's generation counters.** `TotalCardsGenerated` and
    `TotalCardsAccepted` are never updated by generation or saving; they only hold seeded test
    values. Backed by: code only (no code writes them).
20. **Unsaved proposals are lost without a warning.** Leaving the page, or reloading, discards
    all proposals. A "leave without saving" dialog exists but is not wired up. Backed by: code
    only (`Components/Pages/Generate/GenerateFlashcardsViewState.cs` `HasUnsavedEdits` is defined
    but unused; the navigation guard in `Components/Pages/GeneratePage.razor.cs` is commented
    out).
21. **"Try Again" resets the page.** When an error is shown (for example when starting the job throws),
    "Try Again" clears the proposals and returns to the starting state. Backed by: code only
    (`Components/Pages/GeneratePage.razor.cs` `HandleRetry`).

### Behavior worth knowing (code reading, no tests)

- **The "source text" is used as a topic.** The prompt tells the model to create flashcards
  "about the topic" given by the text. It does not tell the model to extract facts from it, even
  though the form says "text to generate flashcards from".
- **The prompt is written for Polish learners.** Fronts are asked to be Polish and backs English,
  whatever language is selected. The selected language is validated but not used when building the
  prompt (`Fiszki.Services/Services/GenerationService.cs` `GenerateFlashcardsWithOpenRouter`).
- **The text you submit is written to the application log** at information level, labeled as the
  topic.
- **Failure is practically invisible.** Because the AI failure is replaced by placeholders (rule
  9), a "Generation failed" outcome is only reachable for problems outside the AI call; the
  user is not told that the cards are placeholders.

## User flow

1. The user opens `/generate`, types or pastes the text, optionally changes language and maximum
   cards, and clicks **Generate Flashcards**.
2. A full-screen overlay shows "Generating Flashcards..." with a **Cancel Generation** button.
3. When done, the page shows "Generation completed" and the list under "Generated Flashcards".
4. The user clicks **Accept All**, rejects or edits cards as needed.
5. The user clicks **Save Selected (N)**; the accepted cards are stored and appear on the
   Flashcards page as "AI Generated".

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Page:** `Components/Pages/GeneratePage.razor` and `Components/Pages/GeneratePage.razor.cs`
  hold the state (`Components/Pages/Generate/GenerateFlashcardsViewState.cs`) and the handlers
  `HandleGenerateSubmit`, `PollGenerationStatus`, `HandleBulkAccept`, `HandleBulkReject`,
  `HandleSaveSelected`, `HandleProposalSave`. The user id for saving comes from the
  `NameIdentifier` claim.
- **Components:** `SourceInputForm`, `ProposalsSection`, `ProposalReviewDialog`, `LoadingOverlay`,
  `ErrorState` and `EmptyState` in `Components/Pages/Generate/`. `SourceInputForm` and
  `ProposalReviewDialog` are plain Bootstrap markup; `ProposalsSection`, `LoadingOverlay`,
  `ErrorState` and `EmptyState` use MudBlazor.
- **View model:** `Components/Pages/Generate/FlashcardProposalViewModel.cs` `FromDto` marks a
  proposal selected when it is accepted; `ToDto` carries accepted and rejected flags to the
  service.
- **Service contract:** `Fiszki.Services/Interfaces/IGenerationService.cs` `StartAsync`,
  `GetStatusAsync`, `CancelAsync`, `SaveProposalsAsync`.
- **Service logic:** `Fiszki.Services/Services/GenerationService.cs` `StartAsync` validates the
  command, creates an in-memory job and starts the work in the background, see
  [[generation-job-flow]]. `SaveProposalsAsync` validates, keeps accepted non-rejected proposals,
  maps them with `Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity` and saves in one
  `SaveChangesAsync`.
- **Validation in the page:** `Components/Pages/Generate/GenerationValidation.cs` defines the same
  limits and message helpers, but the page uses only its default-cards constant; the 50, 5000 and
  50 limits are repeated as literals in `Components/Pages/Generate/SourceInputForm.razor.cs`
  `HandleMaxCardsChange` and the form markup.
- **Validators:** `Fiszki.Services/Validation/StartGenerationCommandValidator.cs`
  `StartGenerationCommandValidator`, `Fiszki.Services/Validation/SaveProposalsCommandValidator.cs`
  `SaveProposalsCommandValidator`.
- **Test selectors:** the form and review list expose `data-testid` values such as
  `source-text-input`, `language-select`, `max-cards-input`, `generate-flashcards-button`,
  `accept-all-button`, `reject-all-button`, `save-selected-button`, `proposals-header`,
  `proposal-item`.

### Test coverage

- **Unit tests** (`Fiszki.Tests/Services/GenerationServiceTests.cs`): starting a job, invalid
  command, status of an existing and an unknown job, cancel (existing and unknown), saving only
  accepted proposals, saving none, invalid save command, and the placeholder fallback. The
  validators are **mocked** there; the input limits are tested in
  `Fiszki.Tests/Services/Validation/ValidatorTests.cs` `StartGenerationCommandValidatorTests`
  (valid, text too short, unsupported language, zero cards). The maximum text length, the maximum
  and minimum card counts above 1, and `SaveProposalsCommandValidator` are not tested.
- **No test covers** the real OpenRouter client, the prompt, the mapping from the AI response to
  proposals, or the page logic.
- **Gherkin** (`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`) drives the page:
  login, open Generate, enter the sample text (`SampleSourceText`, a Polish text about an
  archaeological site), set 5 cards, generate, see "Generation completed", see proposals and the
  "X of Y selected" header, click Accept All, check all are selected and Save Selected is enabled.
  Browser actions are in `Fiszki.FunctionalTests/PageObjects/FlashcardGenerationPage.cs`.
- **Gaps in the scenario:** it never clicks Save Selected, so saving through the UI is untested
  end to end. The step "the generated flashcards should contain expected content"
  (`Fiszki.FunctionalTests/Steps/FlashcardSteps.cs` `ThenTheGeneratedFlashcardsShouldContainExpectedContent`)
  passes if **any one** of five expected words appears **anywhere on the page**, not specifically
  in a generated card. Whether the run produced real or placeholder cards depends on the API key
  of the environment, see [[openrouter-integration]].

## Dependencies and side effects

- Creates flashcards that appear in [[flashcard-deletion]]'s list and can be deleted there.
- Calls an external AI service, see [[openrouter-integration]]; it needs an API key to work.
- Does not change user counters (rule 19).

## Discrepancies

Older notes and some UI texts disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Text length.** The historical product requirements document (prd.md in the old `.ai` folder)
  and API plan say the text must be 1000 to 10000 characters. The code allows 50 to 5000.
- **Behavior on AI errors.** The product requirements say the user sees an error message when
  the AI API fails or does not answer. The code falls back to placeholder cards and reports
  success (rule 9).
- **Counters.** The API plan says generation and acceptance increase the user's generated and
  accepted counters; no code does that (rule 19). The success metric in the requirements
  (75% of AI cards accepted) therefore cannot be computed from these counters.
- **Logging.** The API plan says the source text must be excluded from logs; the code logs it.
- **HTTP API.** The API plan describes generation endpoints; there is no HTTP API, the page calls
  `IGenerationService` directly, see [[application-layers]].
- **"Retry logic".** `Components/Pages/AboutTechnicalities.razor` says the OpenRouter integration
  has retry logic. The code has none, see [[openrouter-integration]].
- **Configuration name.** `README.md` shows `OpenRouter__BaseUrl`, but the option read by the code
  is `BaseUri`.

## Related

- [[openrouter-integration]]
- [[generation-job-flow]]
- [[flashcard-entity]]
- [[flashcard-ownership]]
- [[flashcard-deletion]]
- [[application-layers]]
- [[glossary]]
