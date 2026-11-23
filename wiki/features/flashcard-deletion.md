---
title: Flashcard deletion
type: feature
summary: How a logged-in user permanently deletes one of their own flashcards from the Flashcards page, with a confirmation dialog, an ownership check and cascade to learning progress.
tags: [flashcards, deletion, delete, remove, confirmation, dialog, modal, trash, ownership, permanent, cascade]
sources:
  - Fiszki.FunctionalTests/Features/FlashcardDeletion.feature
  - Components/Pages/Flashcards.razor
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Database/Configurations/FlashcardConfiguration.cs
  - Fiszki.Database/Configurations/LearningProgressConfiguration.cs
  - Fiszki.Tests/Services/FlashcardServiceTests.cs
  - Fiszki.FunctionalTests/Steps/FlashcardSteps.cs
  - Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs
  - docs/TEST_SELECTORS.md
verified_commit: 1b5badb4dd95f2e2fd2a8d40d9daf5adfca8041b
confidence: high
---

# Flashcard deletion

## Summary

A logged-in user can delete a flashcard they own from the Flashcards page (`/flashcards`).
Deletion is **permanent** and always goes through a **confirmation dialog**. The same
action is available in the card view and in the list view.

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario),
**unit test**, or **code only** (read from the implementation, no automated test).

1. **Login is required.** The Flashcards page is reachable only by authenticated users.
   Backed by: code only (`Components/Pages/Flashcards.razor` `Authorize`).
2. **Deletion needs confirmation.** Clicking the trash button on a flashcard only opens a
   "Confirm Deletion" dialog. Nothing is deleted until the user clicks **Delete** in that
   dialog. Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/FlashcardDeletion.feature`, scenario "Delete a flashcard with confirmation")
   and code (`Components/Pages/Flashcards.razor` `ShowDeleteConfirmation`).
3. **Cancel keeps the flashcard.** The **Cancel** button and the close (X) button close the
   dialog without deleting anything. Backed by: code only
   (`Components/Pages/Flashcards.razor` `CancelDelete`). No scenario covers this path.
4. **The dialog shows what will be deleted.** It displays the question (front) and the
   answer (back) of the selected flashcard and asks "Are you sure you want to delete this
   flashcard?". Backed by: code only.
5. **Users delete only their own flashcards.** See [[flashcard-ownership]]. Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `DeleteAsync_ShouldNotDeleteOtherUsersCard`).
6. **Deleting something that is not there is harmless.** Deleting a flashcard that does not
   exist, or that belongs to another user, does nothing and raises **no error**. The user is
   not told which of the two it was. Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `DeleteAsync_Idempotent`).
7. **Deletion is permanent.** The flashcard is removed from the database. There is no
   soft-delete flag, trash or undo: the `Flashcard` entity has no deleted marker and the
   service removes the row. Backed by: code only
   (`Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync`).
8. **Only the selected flashcard is deleted.** Other flashcards of the same user are not
   affected. Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `DeleteAsync_ShouldOnlyAffectSpecifiedCard`).
9. **Learning progress is deleted with the flashcard.** The database cascades the delete to
   the flashcard's learning-progress record, see [[learning-progress-entity]]. Backed by: code only
   (`Fiszki.Database/Configurations/LearningProgressConfiguration.cs` `OnDelete`).
10. **The page updates immediately.** After a successful deletion the dialog closes, the
    flashcard disappears from the list, and the three counters ("Total Cards", "AI
    Generated", "Manual") are recalculated. Backed by: code only
    (`Components/Pages/Flashcards.razor` `ConfirmDelete`). The Gherkin scenario does not
    assert this, see Test coverage.
11. **A failure replaces the whole list with an error.** If the current user cannot be
    determined, or the deletion throws, the page shows a single error alert in place of the
    flashcard list ("Unable to determine current user." or "Failed to delete flashcard:
    <message>"). There is no retry button; the user must reload the page. Backed by: code
    only (`Components/Pages/Flashcards.razor` `ConfirmDelete`).
12. **Deletion does not change the user's generation counters.** `TotalCardsGenerated` and
    `TotalCardsAccepted` on the user are not decremented, so AI acceptance statistics are not
    affected by deleting a card. Backed by: code only
    (`Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync` touches only the flashcard).

### Behavior worth knowing (code reading, no tests)

- **Filtered empty state.** The "No flashcards yet" state is chosen from the *filtered* list.
  If a filter (AI Generated or Manual) is selected and its list becomes empty, for example
  after deleting the last card of that kind while other cards remain, the page shows
  "No flashcards yet" and the counter cards that clear the filter are not shown. The user has
  to reload to see the remaining cards. The same happens when a filter with no cards is
  selected, regardless of deletion. See `Components/Pages/Flashcards.razor` `ApplyFilter` and
  [[flashcard-list-and-filters]].
- **100-card window.** The page loads only the 100 newest flashcards
  (`Components/Pages/Flashcards.razor` `LoadFlashcards`). Deleting a card does not reload the
  list, so an older card is not pulled in until the page is reloaded.

## User flow

1. The user opens `/flashcards` and sees their flashcards (card view by default).
2. The user clicks the trash icon (tooltip "Delete") on a flashcard.
3. The "Confirm Deletion" dialog opens with the question and answer of that flashcard.
4. The user clicks **Delete** to confirm, or **Cancel** / X to abort.
5. On confirmation the flashcard is removed and the page updates (rules 7 and 10).

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **UI:** `Components/Pages/Flashcards.razor`. The state field `_cardToDelete` controls the
  dialog: it is set by `ShowDeleteConfirmation`, cleared by `CancelDelete` and by
  `ConfirmDelete`, and the dialog is rendered while it is not null. The dialog is plain
  Bootstrap markup rendered conditionally, not a MudBlazor dialog.
- **Who is the user:** `ConfirmDelete` reads the user id from the `NameIdentifier` claim of
  the authentication state and passes it to the service. If the claim is missing or not a
  GUID it sets the page error instead of calling the service.
- **Service contract:** `Fiszki.Services/Interfaces/IFlashcardService.cs` `DeleteAsync`
  takes `(userId, flashcardId, ct)` and returns no value.
- **Service logic:** `Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync` looks the
  flashcard up by id **and** user id. If it is not found it returns without error. Otherwise
  it removes the entity and calls `SaveChangesAsync` once.
- **In-memory update:** after the service call, `ConfirmDelete` removes the card from the
  in-memory lists `_allFlashcards` and `_flashcards` and from the flipped-cards set. It does
  not call `ApplyFilter` or reload.
- **Data:** [[flashcard-entity]] and [[learning-progress-entity]]; delete behavior is
  configured in `Fiszki.Database/Configurations/FlashcardConfiguration.cs` and
  `Fiszki.Database/Configurations/LearningProgressConfiguration.cs`.
- **Test selectors:** the dialog has `data-testid` values `delete-confirmation-modal`,
  `confirm-delete-button`, `cancel-delete-button` (X) and `cancel-delete-modal-button`
  (Cancel), listed in `docs/TEST_SELECTORS.md`. The trash buttons have **no** `data-testid`;
  they are located by `title="Delete"` or the trash icon.

### Test coverage

- **Unit tests** in `Fiszki.Tests/Services/FlashcardServiceTests.cs`: `DeleteAsync_ShouldRemove`,
  `DeleteAsync_Idempotent`, `DeleteAsync_ShouldNotDeleteOtherUsersCard`,
  `DeleteAsync_ShouldOnlyAffectSpecifiedCard`. They cover the service only.
- **Gherkin** `Fiszki.FunctionalTests/Features/FlashcardDeletion.feature` drives the flow
  through the UI: login with the test user, open the Flashcards page, click delete on the first
  flashcard, see the dialog, click confirm. Steps are in
  `Fiszki.FunctionalTests/Steps/FlashcardSteps.cs` and the browser actions in
  `Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs`.
- **Gap:** the last step, "the flashcard should be deleted"
  (`Fiszki.FunctionalTests/Steps/FlashcardSteps.cs` `ThenTheFlashcardShouldBeDeleted`), only
  asserts that the page is not in the loading state and prints whether flashcards remain. It
  does **not** assert that the flashcard disappeared. That the flashcard is really gone is
  verified only by the service-level unit test. Cancel, error handling and the counters are not
  tested at all.

## Dependencies and side effects

- Removes the flashcard's learning-progress record (rule 9).
- Changes the page counters but not the user's generation counters (rule 12).
- No HTTP API is involved: the page calls the service directly, see [[application-layers]].

## Discrepancies

Older planning notes disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **REST endpoint.** The historical API plan (api-plan.md in the old `.ai` folder) describes
  `DELETE /flashcards/{id}` answering `200 {"message": "Flashcard deleted"}` and a batch
  endpoint `POST /flashcards/batch/delete`. The code has no HTTP endpoint and no batch
  deletion. Deletion is a direct call from `Components/Pages/Flashcards.razor` to
  `IFlashcardService.DeleteAsync`.
- **UI technology and paging.** The historical UI plan (ui-plan-prompted.md in the old `.ai`
  folder) says deletion uses MudBlazor dialogs and the list uses server-side pagination. The
  page uses a Bootstrap-styled dialog (no MudBlazor on this page, although other pages use it)
  and loads up to 100 flashcards with no paging controls.

## Related

- [[flashcard-list-and-filters]]
- [[flashcard-ownership]]
- [[flashcard-entity]]
- [[learning-progress-entity]]
- [[application-layers]]
- [[glossary]]
