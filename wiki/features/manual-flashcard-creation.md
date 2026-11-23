---
title: Manual flashcard creation
type: feature
summary: How a logged-in user creates a flashcard by hand in the Create Manual Flashcard dialog on the Flashcards page: required fields, tags, limits, what the user sees, and the service-only update that has no screen.
tags: [flashcards, manual, create, creation, dialog, modal, question, answer, tags, validation, add-manual-card, create-manually, update, edit, concurrency]
sources:
  - Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature
  - Fiszki.FunctionalTests/Features/FlashcardCreation.feature
  - Components/Pages/Flashcards.razor
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Commands/CreateFlashcardCommand.cs
  - Fiszki.Services/Commands/UpdateFlashcardCommand.cs
  - Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs
  - Fiszki.Services/Validation/UpdateFlashcardCommandValidator.cs
  - Fiszki.Tests/Services/FlashcardServiceTests.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
  - Fiszki.FunctionalTests/Steps/FlashcardSteps.cs
  - Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs
verified_commit: b495105fc83203eab08cd4f7daa346d228e4d01e
confidence: high
---

# Manual flashcard creation

## Summary

A logged-in user can add a flashcard by hand from the Flashcards page (`/flashcards`). A dialog
asks for a question (front), an answer (back) and optional tags. The new card is stored with
creation source `Manual` and appears in the list at once. The other way to create flashcards is
[[ai-flashcard-generation]]. The limits on text and tags are collected in
[[flashcard-content-limits]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

1. **Login is required.** The Flashcards page is only for authenticated users. Backed by: code
   only (`Components/Pages/Flashcards.razor` `Authorize`).
2. **Two buttons open the dialog.** "Create Manually" (when the user has no flashcards yet) and
   "Add Manual Card" (when there are some). Backed by: Gherkin for "Add Manual Card"
   (`Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature`, scenario "Create a manual flashcard successfully"),
   code for the other (`Components/Pages/Flashcards.razor` `CreateManualCard`).
3. **Question and answer are required.** The dialog refuses to create a card when either is
   empty or only spaces and shows "Both question and answer are required." in the dialog, which
   stays open. Backed by: Gherkin for an empty question
   (`Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature`, scenario "Attempt to create flashcard with missing required fields"),
   code (`Components/Pages/Flashcards.razor` `CreateCard`). The Gherkin step only checks that
   some error text appears, not which text.
4. **Tags are optional, typed as a comma-separated list.** Spaces around each tag are removed and
   empty tags are dropped. Backed by: code
   (`Components/Pages/Flashcards.razor` `CreateCard`); the Gherkin success scenario types a tag
   but never checks it.
5. **Text and tags are cleaned before saving.** Question and answer are trimmed; tags are trimmed
   and duplicates are removed ignoring letter case, keeping the first spelling ("Tag1" and
   " tag1 " become one tag "Tag1"). Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `CreateAsync_ShouldPersistAndReturnDto`).
6. **Limits.** Question at most 1000 characters, answer at most 2000, at most 10 tags, each tag
   at most 50 characters. Backed by: unit test for an empty question and for more than 10 tags
   (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `CreateFlashcardCommandValidatorTests`);
   code for the lengths
   (`Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs` `CreateFlashcardCommandValidator`).
   Details and the comparison with other paths are in [[flashcard-content-limits]].
7. **The dialog does not limit length.** The text boxes accept any length, so a too-long text is
   only refused after the user clicks Create Card, and the dialog shows only "Validation failed",
   not which field or limit. Backed by: code only
   (`Components/Pages/Flashcards.razor` `CreateCard`).
8. **A manual card belongs to the current user and is marked Manual.** Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `CreateAsync_ShouldPersistAndReturnDto`).
   See [[flashcard-ownership]].
9. **The list and counters update immediately.** The dialog closes, the card appears and the
   "Total Cards" and "Manual" counters go up. The new card is added at the **end** of the visible
   list; after a reload the list is newest first, so it moves to the top. Backed by: Gherkin for
   "the dialog closes and the card is visible"
   (`Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature`, scenario "Create a manual flashcard successfully");
   the position comes from code only (`Components/Pages/Flashcards.razor` `CreateCard`).
10. **Cancel creates nothing.** The Cancel button and the close (X) button close the dialog and
    clear what was typed. Backed by: Gherkin
    (`Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature`, scenario "Cancel manual flashcard creation").
11. **While saving, the button is disabled and shows a spinner**, which discourages submitting
    twice. The code has no other guard against a double submit. Backed by: code only
    (`Components/Pages/Flashcards.razor` `CreateCard`).
12. **Failures stay in the dialog.** If the current user cannot be determined the dialog shows
    "Unable to determine current user."; any error from the service is shown as its message. The
    dialog stays open with what the user typed. Backed by: code only
    (`Components/Pages/Flashcards.razor` `CreateCard`).
13. **Tags are stored but not shown.** Tags are saved with the card, but the Flashcards page never
    displays them and the counter cards filter only by creation source, so tags cannot currently
    be seen, searched or filtered in the application. Backed by: code only (no markup renders
    tags).

### Editing existing flashcards (service only)

- **There is no edit screen.** Every card shows a pencil button with the tooltip "Edit", but it
  does nothing: the handler is a stub with a TODO
  (`Components/Pages/Flashcards.razor` `EditCard`). Backed by: code only.
- **The service can update.** `Fiszki.Services/Services/FlashcardService.cs` `UpdateAsync`
  accepts the same limits as creation, changes question, answer and tags (cleaned the same way)
  and sets the update time. A flashcard of another user, or one that does not exist, is reported
  as "Flashcard not found". If the caller passes the update time it last saw and it no longer
  matches, the update is refused with "Flashcard has been modified by another operation"
  (protection against overwriting a newer change). Backed by: unit test for the stale-time and
  not-found cases (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `UpdateAsync_ConcurrencyConflict_ShouldThrow`,
  `Fiszki.Tests/Services/FlashcardServiceTests.cs` `UpdateAsync_NotFound_ShouldThrow`); a successful
  update and the update validator have no test.

## User flow

1. On `/flashcards` the user clicks "Add Manual Card" (or "Create Manually" when the list is
   empty).
2. The "Create Manual Flashcard" dialog opens with Question (Front)\*, Answer (Back)\* and Tags.
3. The user types, then clicks **Create Card** (or Cancel).
4. On success the dialog closes and the card appears in the list.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **UI:** `Components/Pages/Flashcards.razor`. State fields for the dialog and its fields; the
  dialog markup is outside the list conditions, so it can show in every state. `CreateManualCard`
  opens it and clears the fields, `CancelCreate` closes it, `CreateCard` does the work: checks
  the two required fields, reads the user id from the `NameIdentifier` claim, splits the tags,
  builds a `CreateFlashcardCommand` and calls the service.
- **Service:** `Fiszki.Services/Services/FlashcardService.cs` `CreateAsync` validates the command
  (a `ValidationException` with the message "Validation failed" when invalid), builds the entity as
  `Manual` with a new id and creation and update times in UTC, and saves it. The returned DTO is
  appended to the page's lists.
- **Commands and validators:** `Fiszki.Services/Commands/CreateFlashcardCommand.cs`,
  `Fiszki.Services/Commands/UpdateFlashcardCommand.cs`,
  `Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs`,
  `Fiszki.Services/Validation/UpdateFlashcardCommandValidator.cs`.
- **Data:** [[flashcard-entity]].
- **Test selectors:** the buttons and dialog have `data-testid` values `create-manually-button`,
  `add-manual-card-button`, `create-manual-card-modal`, `question-front-input`,
  `answer-back-input`, `tags-input`, `create-card-button`, `cancel-create-button`,
  `close-create-modal-button`, `create-error-alert`.

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/ManualFlashcardCreation.feature` has three
  scenarios: create successfully (dialog opens, question, answer and a tag are typed, dialog
  closes, the card is visible), missing question (an error text is shown), and cancel (dialog
  closes). Steps are in `Fiszki.FunctionalTests/Steps/FlashcardSteps.cs`, browser actions in
  `Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs`.
- **Unit tests** in `Fiszki.Tests/Services/FlashcardServiceTests.cs`: `CreateAsync_ShouldPersistAndReturnDto`
  (saves, trims, removes duplicate tags, `Manual`, owner) and `CreateAsync_Invalid_ShouldThrow`
  (with the validator mocked). Validator tests cover a valid card, an empty question and too many tags
  (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `CreateFlashcardCommandValidatorTests`).
- **Not tested:** an empty answer alone, the length limits (question, answer, tag), the tags
  (typed in the success scenario but never checked), the "no current user" path, the position of
  the new card, double-submit behavior, and the update validator.
- **Weak scenario:** `Fiszki.FunctionalTests/Features/FlashcardCreation.feature` is named like a
  creation test but exercises AI generation and saving, and its checks are weak, see
  [[ai-flashcard-generation]].

## Dependencies and side effects

- Creates a flashcard that [[flashcard-deletion]] can later remove.
- Does not change the user's generation counters (they are not updated by any flow).

## Discrepancies

Older notes disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Limits.** The historical API plan (api-plan.md in the old `.ai` folder) says question and
  answer are at most 2000 characters and each tag at most 40 characters. The code allows a
  question of 1000, an answer of 2000 and tags of 50 characters.
- **Learning progress.** The API plan says manual creation also creates a learning-progress
  record. The code creates none, see [[learning-progress-entity]].
- **Editing.** The API plan and the historical UI plan describe editing a flashcard (PUT or PATCH
  with concurrency protection, an edit dialog). The code has the protection in the service but no
  HTTP endpoint and no working edit screen.
- **Client-side validation.** The UI plan says the screens mirror the server limits with form
  validation. The create dialog checks only that question and answer are filled in.

## Related

- [[flashcard-content-limits]]
- [[flashcard-entity]]
- [[flashcard-ownership]]
- [[ai-flashcard-generation]]
- [[flashcard-deletion]]
- [[application-layers]]
- [[glossary]]
