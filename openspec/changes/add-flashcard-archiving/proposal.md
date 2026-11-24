# Proposal

## Why

A user has no way to say "I already know this flashcard", so the Flashcards page keeps showing cards
that are done and gives no sense of progress. The leaderboard rewards only creating flashcards, not
learning them. Letting the user mark a flashcard as learned (archive it), showing a learning progress
percentage and rewarding learned flashcards in the ranking encourages the user to actually study
what they create.

## What Changes

- Each flashcard on the Flashcards page gets a third button, "I learned it", next to Edit and Delete,
  in both the card view and the list view. It opens a confirmation dialog; after the user confirms,
  the flashcard is archived.
- An archived flashcard disappears from the main list and appears in a separate archived view on the
  Flashcards page, where the user can restore it (undo the archiving, no confirmation) or delete it.
- The Flashcards page shows a learning progress percentage: archived flashcards divided by all the
  user's flashcards (active and archived, AI and manual). It goes up when a flashcard is archived and
  down when one is restored or when an archived one is deleted.
- The Total, AI Generated and Manual counters count archived flashcards too.
- The leaderboard score changes from "1 point per flashcard" to "1 point per flashcard plus 3 extra
  points per archived flashcard" (an archived flashcard is worth 4). The ranking shows, for every user,
  the number of flashcards, the number of archived flashcards, the points and the learning progress
  percentage. The percentage is information only and does not affect the order.
- The tie-break on equal points follows the moment the score was reached, so archiving counts as
  reaching it.
- The banner on the leaderboard talks about catching up by creating or learning flashcards.
- Functional (Gherkin) tests are updated to match the new points and extended with scenarios for
  archiving, the archived view, restoring, deleting an archived flashcard and the new leaderboard
  columns.
- A new nullable column on the flashcards table and a database migration.

Not changing: the Edit button stays a stub, learning-progress (spaced repetition) records are not used,
and the 100-card window of the main list stays.

## Capabilities

### New Capabilities
- `flashcard-archiving`: marking a flashcard as learned (archive) with confirmation, the archived view,
  restoring, deleting an archived flashcard, and the learning progress percentage shown on the
  Flashcards page.

### Modified Capabilities
- `leaderboard`: points formula (1 per flashcard plus 3 per archived flashcard), new columns
  (flashcards, archived, points, learning progress), tie-break by the moment the score was reached,
  and the banner wording.

## Impact

- **Database:** `Fiszki.Database` `Flashcard` entity and `FlashcardConfiguration` (new nullable
  `archived_at`), a new EF Core migration.
- **Services:** `IFlashcardService` / `FlashcardService` (archive, restore, list by state, counts for
  the page), `LeaderboardService` and `LeaderboardDtos`, `TestDataSeeder` if seed data is needed.
- **UI:** `Components/Pages/Flashcards.razor` (button, confirmation dialog, archived view, progress,
  counters), `Components/Pages/Leaderboard.razor` (columns, banner), `docs/TEST_SELECTORS.md`.
- **Tests:** `Fiszki.Tests` unit tests for the service and the ranking; `Fiszki.FunctionalTests`
  features `Leaderboard.feature`, `FlashcardsUI.feature` and a new archiving feature, with page
  objects and steps.
- **Wiki:** pages for the Flashcards page, deletion, leaderboard and the flashcard entity become stale
  and are refreshed by `wiki-ingest` after archiving this change.
