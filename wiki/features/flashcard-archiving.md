---
title: Flashcard archiving and learning progress
type: feature
summary: How a user marks a flashcard as learned with "I learned it" (archive), the archived view with restore and delete, the learning progress percentage and archived count on the Flashcards page, what counts toward 100%, and what the tests cover.
tags: [archive, archiving, archived, learned, i-learned-it, restore, learning-progress, progress, percentage, progress-bar, flashcards-page, archived-view, counters, confirmation, dialog, points, gamification]
sources:
  - openspec/specs/flashcard-archiving/spec.md
  - Components/Pages/Flashcards.razor
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Services/LearningProgressCalculator.cs
  - Fiszki.Services/Models/FlashcardDto.cs
  - Fiszki.Services/Models/FlashcardSummaryDto.cs
  - Fiszki.Database/Entities/Flashcard.cs
  - Fiszki.Database/Configurations/FlashcardConfiguration.cs
  - Fiszki.Database/Migrations/20261124100000_AddFlashcardArchivedAt.cs
  - docs/migrations/20261124100000_AddFlashcardArchivedAt.sql
  - Fiszki.Tests/Services/FlashcardArchivingTests.cs
  - Fiszki.FunctionalTests/Features/FlashcardArchiving.feature
  - Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs
  - Fiszki.FunctionalTests/Steps/FlashcardArchivingSteps.cs
  - docs/TEST_SELECTORS.md
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: medium
---

# Flashcard archiving and learning progress

## Summary

On the Flashcards page (`/flashcards`) every active flashcard has a third button, **"I learned it"**,
next to Edit and Delete. It opens a confirmation dialog; after the user confirms, the flashcard is
**archived**: it leaves the main list and moves to an **Archived** view of the same page, where the user can
**restore** it or delete it. The page shows a **learning progress** percentage (how much of the user's
collection is archived) and the archived count. Every archived flashcard is also worth 3 extra points on the
[[leaderboard]], which shows the same percentage next to the points. In this wiki "learning progress" means this
percentage; the table described in [[learning-progress-entity]] is spaced-repetition data that nothing writes
and is unrelated.

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit test**,
**OpenSpec** (a requirement in `openspec/specs/flashcard-archiving/spec.md`) or **code only**.

### Archiving

1. **Every active flashcard has an "I learned it" button**, in card view and in list view. The Edit and Delete
   buttons stay. Backed by: Gherkin (`Fiszki.FunctionalTests/Features/FlashcardArchiving.feature`, scenario
   "Every active flashcard has an I learned it button in both views") and OpenSpec
   (`openspec/specs/flashcard-archiving/spec.md`, the requirement for the "I learned it" button).
2. **Archiving needs confirmation.** Clicking the button only opens a dialog that shows the question and the
   answer and asks whether the user is sure; nothing changes until the user clicks the green "I learned it" button in
   the dialog. Cancel and the close (X) button leave the flashcard active and the progress unchanged. Backed by:
   Gherkin ("Cancelling the confirmation keeps the flashcard active", "Confirming moves the flashcard to the
   archived view") and OpenSpec (requirement "Archiving needs confirmation").
3. **After confirming, the flashcard leaves the active list at once and appears in the Archived view.** Its
   content is not changed. Backed by: Gherkin ("Confirming moves the flashcard to the archived view") and OpenSpec
   (requirement "Archived flashcards leave the active list").
4. **Archiving works only on the user's own flashcards, and repeating it changes nothing.** Archiving a missing
   card, another user's card or a card that is already archived does nothing and raises no error; the first
   archive time is kept. Backed by: unit test (`Fiszki.Tests/Services/FlashcardArchivingTests.cs`
   `ArchiveAsync_OtherUsersCard_ChangesNothing`, `ArchiveAsync_MissingCard_DoesNotThrow`,
   `ArchiveAsync_Repeated_KeepsFirstArchivedAt`) and OpenSpec (requirement "Users manage only their own archive").
   See [[flashcard-ownership]].

### Archived view, restore and delete

5. **The Archived view is a switch on the same page**, "Active" and "Archived (N)", where N is the archived
   count. The Archived view lists the archived cards, most recently archived first, at most 100. Backed by: Gherkin
   ("Confirming moves the flashcard to the archived view") and unit test
   (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `ListAsync_Archived_ReturnsOnlyArchivedNewestArchivedFirst`);
   the limit of 100 is code only (`Components/Pages/Flashcards.razor` `LoadFlashcards`).
6. **In the Archived view a card has "Restore" and Delete, and no "I learned it" and no Edit button.** Backed by:
   Gherkin ("Confirming moves the flashcard to the archived view", steps "should offer Restore and Delete" and "should
   not have the I learned it button") and code (`Components/Pages/Flashcards.razor` `CardActions`).
7. **Restore needs no confirmation.** The card goes back to the active list, loses its 3 extra points and the
   progress goes down. Backed by: Gherkin ("Restoring brings the flashcard back and lowers the progress") and unit test
   (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `RestoreAsync_ClearsArchivedAt`,
   `ArchiveThenRestore_MovesProgressUpAndDown`). Restoring a card that is not archived does nothing.
8. **An archived flashcard can be deleted** with the same "Confirm Deletion" dialog and the same permanent deletion
   as an active one, see [[flashcard-deletion]]. The user loses all 4 points of that card, the total and archived
   counts go down by one and the progress is recalculated from the smaller collection (2 of 4 archived is 50%;
   after deleting one archived card it is 1 of 3, 33%). Backed by: Gherkin ("An archived flashcard can be deleted")
   and unit test (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `DeleteAsync_ArchivedCard_Removes_AndChangesTheBase`).

### Learning progress and counters

9. **Learning progress is the archived share of all the user's flashcards**, in whole percent, **rounded down**,
   so 100% appears only when every flashcard is archived (199 of 200 shows 99%). All flashcards count, active and
   archived, AI and manual. With no flashcards the value is 0. Backed by: unit test
   (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `LearningProgressCalculator_RoundsDown_AndReaches100OnlyWhenAllArchived`,
   `GetSummaryAsync_NoFlashcards_IsZero`) and OpenSpec (requirement "Learning progress percentage").
10. **Archiving raises the progress and keeps the counters.** "Total Cards", "AI Generated" and "Manual" count
    archived flashcards too, so archiving does not change them; AI plus Manual always equals Total. The page also
    shows the archived count ("N of M learned"). Backed by: Gherkin ("Archiving raises the learning progress and keeps
    the counters") and OpenSpec (requirement "Counters include archived flashcards").
11. **The counters and the progress come from the database, not from the loaded list.** They are counted over all of
    the user's flashcards, so they stay right although the page loads at most 100 cards and the active list no longer
    contains archived ones. They are refreshed after every archive, restore, delete and create. Backed by: code only
    (`Components/Pages/Flashcards.razor` `RefreshSummaryAsync`,
    `Fiszki.Services/Services/FlashcardService.cs` `GetSummaryAsync`) and unit test
    (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `GetSummaryAsync_CountsArchivedInEveryTotal`).
12. **"No flashcards yet" is shown only to a user with no flashcards at all.** A user whose flashcards are all
    archived still sees the counters, the progress at 100% and the switch, and the active list says "No active
    flashcards to show."; the Archived view of a user with nothing archived says no flashcards are archived yet.
    A filter that matches nothing now shows the same short message instead of hiding the counters. Backed by: Gherkin
    ("Archiving the only flashcard keeps the page usable", "Archived view of a user who archived nothing") and OpenSpec
    (the requirement that "No flashcards yet" appears only without any flashcards). The progress is not shown on the empty page.
13. **Failures replace the page with an error.** If archiving or restoring throws, the whole page content is
    replaced by one red alert ("Failed to archive flashcard: ..." or "Failed to restore flashcard: ..."), as for
    deletion, with no retry button. Backed by: code only (`Components/Pages/Flashcards.razor` `ConfirmArchive`,
    `Components/Pages/Flashcards.razor` `RestoreCard`).

### Points

14. **An archived flashcard is worth 4 points** (1 for owning it plus 3 for learning it), restoring takes the 3
    back, and the learning progress itself never changes the ranking. See [[leaderboard]]. Backed by: Gherkin
    (`Fiszki.FunctionalTests/Features/Leaderboard.feature`, scenario "Archiving, restoring and deleting an archived flashcard move my points") and OpenSpec (`openspec/specs/leaderboard/spec.md`, requirement
    "Points for flashcards and archived flashcards").

## User flow

1. On `/flashcards` the user clicks "I learned it" on a card and reads the dialog.
2. The user confirms. The card disappears, the learning progress and the archived count go up and the Total
   counter stays the same.
3. The user opens "Archived (N)" to see learned cards, and can restore one (progress goes down) or delete it.
4. On the [[leaderboard]] the user sees more points, the new archived count and the new percentage.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Data:** `Fiszki.Database/Entities/Flashcard.cs` `Flashcard` has a nullable `ArchivedAt` (column `archived_at`,
  `Fiszki.Database/Configurations/FlashcardConfiguration.cs` `Configure`). A flashcard is archived when it is not
  null; restoring sets it back to null. There is no history of archive events, only the current value. See
  [[flashcard-entity]]. Migration: `Fiszki.Database/Migrations/20261124100000_AddFlashcardArchivedAt.cs`
  (hand-written, because `dotnet ef` is not usable in this repository) and the equivalent idempotent script
  `docs/migrations/20261124100000_AddFlashcardArchivedAt.sql`. The application does **not** apply migrations, so the
  column must exist before the new code runs, see [[ci-cd-and-startup]].
- **Service:** `Fiszki.Services/Interfaces/IFlashcardService.cs` and `Fiszki.Services/Services/FlashcardService.cs`:
  `ArchiveAsync` and `RestoreAsync` look the card up by id **and** user id and quietly do nothing when it is
  missing, foreign or already in the target state (archive keeps the first `ArchivedAt` and both set `UpdatedAt`).
  `ListAsync` takes a `FlashcardListState` (`Active` by default, or `Archived`, newest archived first).
  `GetSummaryAsync` returns a `FlashcardSummaryDto` (total, AI, manual, archived, learning progress) from
  three count queries. `DeleteAsync` is unchanged and works for archived cards.
- **Progress rule:** `Fiszki.Services/Services/LearningProgressCalculator.cs` `Percent` (integer division, so rounded
  down; 0 when there are no flashcards). The Flashcards page and the leaderboard both use it, so they show the same number.
- **UI:** `Components/Pages/Flashcards.razor`. State: `_cardToArchive` drives a second conditional Bootstrap dialog
  (`ShowArchiveConfirmation`, `CancelArchive`, `ConfirmArchive`), `_showArchived` the view, `_summary` the counters
  and progress. `SwitchViewAsync` reloads the list for the chosen view (the selected source filter is kept).
  `CardActions` is one render fragment with the buttons for both card and list view. `RestoreCard` restores without
  a dialog. `ConfirmDelete` and `CreateCard` also call `RefreshSummaryAsync`; a card created while the Archived view
  is open is not added to that list. The empty state is chosen from `_summary.Total`, not from the filtered list.
- **Test selectors:** `archive-card-button`, `edit-card-button`, `restore-card-button`, `delete-card-button`,
  `archive-confirmation-modal`, `confirm-archive-button`, `cancel-archive-button`, `cancel-archive-modal-button`,
  `learning-progress`, `learning-progress-value`, `learning-progress-bar`, `archived-count`, `view-active-button`,
  `view-archived-button`, `flashcards-view-empty`; listed in `docs/TEST_SELECTORS.md`.

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/FlashcardArchiving.feature` has 8 scenarios: the button in both views,
  cancelling, confirming, progress and counters, restoring, deleting an archived card, the Archived view of a user who
  archived nothing, and archiving the only flashcard. Page object: `Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs`;
  steps: `Fiszki.FunctionalTests/Steps/FlashcardArchivingSteps.cs`. The scenarios create their own flashcards with unique
  texts and assert on differences from what they remembered, never on absolute numbers, because all scenarios share one
  database. A scenario title must not contain "empty", "admin" or "demo", because those words choose a different test
  user (`Fiszki.FunctionalTests/Hooks/SeededTestUserHooks.cs`).
- **Unit tests** `Fiszki.Tests/Services/FlashcardArchivingTests.cs`: archive, restore, owner check, repetition, the two
  lists, the summary, the rounding and the effect of deleting an archived card. They run on the in-memory provider.
- **Not tested:** the exact percentages 25, 66 and 33 through the UI (the scenarios compare with what they remembered; the
  rounding itself is a unit test), the error states of archive and restore, and the 100-card cap of the Archived view.
  No automated test runs the queries on PostgreSQL; the migration and the archive, restore, delete and ranking queries
  were run once by hand against PostgreSQL 14 with a throwaway program that is not kept in the repository.

## Dependencies and side effects

- Changes the user's points and position on the [[leaderboard]] (3 extra points per archived flashcard).
- Uses the same ownership rule as every flashcard operation, see [[flashcard-ownership]].
- Archived flashcards are still part of the collection: deletion works for them ([[flashcard-deletion]]) and the Total
  counter on [[flashcard-list-and-filters]] includes them.
- The learning-progress table ([[learning-progress-entity]]) is not read or written by this feature.

## Discrepancies

- **Name collision.** The product says "learning progress" for the percentage, while the code and the older wiki pages
  use `LearningProgress` for the spaced-repetition entity. They are separate things; this page and the glossary keep
  them apart.
- **Unstated filter behavior.** The spec does not say what happens to the selected AI or Manual filter when the user
  switches between Active and Archived; the code keeps it (`Components/Pages/Flashcards.razor` `SwitchViewAsync`).
  The code wins.

## Related

- [[leaderboard]]
- [[flashcard-list-and-filters]]
- [[flashcard-deletion]]
- [[flashcard-entity]]
- [[learning-progress-entity]]
- [[flashcard-ownership]]
- [[glossary]]
