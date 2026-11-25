# Design

## Context

- The Flashcards page (`Components/Pages/Flashcards.razor`) loads the 100 newest flashcards of the user
  through `IFlashcardService.ListAsync` and computes the Total / AI / Manual counters from that list.
  Edit is a stub, Delete opens a Bootstrap confirmation dialog rendered while `_cardToDelete` is set.
- The leaderboard (`LeaderboardService.GetAsync`) reads one aggregate row per active user
  (`Flashcards.Count()` and the newest `CreatedAt`), ranks in memory and builds the banner from the same
  list. It has no schema of its own.
- `Flashcard` has no state flag. The `LearningProgress` table is spaced-repetition data that no flow
  writes; it is unrelated to "learned" and is left alone.
- Functional tests share one database. Scenarios assert on the logged-in user's own row and on structure,
  never on other users' absolute positions. The test user is chosen from the scenario tags and **title**
  in `SeededTestUserHooks` (a title containing "empty", "admin" or "demo" switches the user), so new
  scenario titles must avoid those words unless that user is intended.

See proposal.md for the motivation and the specs for the required behavior.

## Goals / Non-Goals

**Goals:**
- One source of truth for "learned": a nullable timestamp on the flashcard.
- Points, learning progress and counters always computed from the current rows, as the leaderboard does
  today, so archive, restore and delete need no extra bookkeeping.
- The counters and the progress stay correct even when the active list is capped at 100.

**Non-Goals:**
- No change to Edit, to the 100-card window of the active list, or to the spaced-repetition tables.
- No pagination or search in the archived view.
- No history of archive events (only the current archive time is kept).
- No change to the user counters `TotalCardsGenerated` / `TotalCardsAccepted`.

## Decisions

### D1: A nullable `ArchivedAt` on `Flashcard`

Add `DateTime? ArchivedAt`, column `archived_at` (timestamp with time zone, nullable), with a new
migration. A flashcard is archived when the value is not null. Archive sets it to now (UTC), restore sets
it back to null.

Alternatives: a boolean `IsArchived` (cannot serve the tie-break, which needs the moment the score was
reached); a separate table of archive events (more structure for no requirement); reusing
`LearningProgress` (different meaning, one row per card, would need rows for cards that never had one).

Existing rows get null, so every existing flashcard stays active and nobody's points change except for
the archived ones created later.

### D2: Service contract in `IFlashcardService`

- `ArchiveAsync(userId, flashcardId)` and `RestoreAsync(userId, flashcardId)`: look the card up by id
  **and** user id, do nothing when it is missing, belongs to someone else or is already in the target
  state (same quiet behavior as `DeleteAsync`). Archive only sets `ArchivedAt` if it is null, so a
  repeated call does not move the date. Both update `UpdatedAt`.
- `ListAsync` gets a state parameter (active by default, so existing callers keep their behavior):
  active cards (`ArchivedAt` is null) or archived cards (`ArchivedAt` is not null; newest archived first).
- `GetSummaryAsync(userId)` returns total, AI, manual and archived counts and the learning progress,
  computed in the database. The page uses it for the counters and the progress instead of counting the
  loaded list, because the active list no longer contains archived cards and is capped at 100.
- `DeleteAsync` is unchanged: it already works for any flashcard the user owns, so deleting an archived
  card needs no new code, only the button in the archived view.

`FlashcardDto` gets `ArchivedAt` so the archived view can show it.

### D3: Learning progress formula

`progress = floor(archived * 100 / total)` as an integer, 0 when total is 0. The rule is in one place
(a small static helper used by the page summary and the leaderboard) so both show the same number.
Rounding down is chosen over normal rounding so that 100% appears only when every card is archived (for
example 199 of 200 does not show 100%). The spec states this.

### D4: Counters stay "all cards"

`Total` = all cards including archived, `AI` + `Manual` = `Total` (all cards by source). This keeps the
existing invariant that the functional test checks. The filter on the AI and Manual counters filters the
visible list (active or archived view) by source; the numbers on the counters do not change with the
filter or with the view, as today. The page also shows the archived count and the progress bar.

### D5: Archived view on the same page

A switch (Active / Archived) on `/flashcards` with test ids, kept in page state. Switching loads the
archived cards through `ListAsync`. In the archived view each card has "Restore" (no dialog) and Delete
(the existing dialog); "I learned it" is shown only in the active view. Empty archived view shows its own
message and does not use the "No flashcards yet" state, so the stuck-filter behavior described in the
wiki is not extended to it.

After archive, restore or delete the page updates the lists and calls `GetSummaryAsync` again, so the
counters and the progress change without a reload.

### D6: Confirmation dialog like the delete dialog

A second conditional Bootstrap dialog driven by `_cardToArchive`, showing question and answer, with
Cancel / close and a confirm button. Reuses the delete dialog's structure and the project's `data-testid`
convention (`archive-confirmation-modal`, `confirm-archive-button`, `cancel-archive-button`,
`cancel-archive-modal-button`). The buttons on cards get `data-testid`s too, which the delete buttons
lack today, so tests need not locate them by title. They are added to `docs/TEST_SELECTORS.md`.

### D7: Leaderboard points and tie-break

Points = `flashcards + 3 * archived` with the bonus as a named constant `ArchivedBonus = 3`. The
aggregate query per active user returns: flashcard count, archived count, and the latest *score event*
time, which is the later of the newest `CreatedAt` and the newest `ArchivedAt` among the user's cards.
The ordering stays: points descending, then earlier latest score event, then older account, then user id.
Because the date is taken from the cards that remain, restoring or deleting moves it back, the same way
deleting the newest card does today.

The row and banner DTOs gain `Flashcards`, `Archived` and `LearningProgress`; `Points` stays. The banner
text mentions creating or learning flashcards. The percentage is displayed but never used in ordering.
The page shows a column for each number and the percentage, with the percentage styled apart from the
points (for example a small progress bar) so it does not read as part of the score.

### D8: Test data and tests

- No change to the seeded users is required: functional scenarios create their own flashcard through the
  UI, archive it and assert relative changes on the user's own data (points +4 relative to before
  creating, archived +1, progress strictly higher than before). This also fits the shared-database rule.
- Unit tests (in-memory provider) cover the data rules the UI cannot set up: points with archived cards,
  the tie-break with archive dates, the progress rounding, owner-only archive and restore, idempotence,
  delete of an archived card and the summary counts.
- Existing Gherkin that assumes "1 flashcard = 1 point" is reviewed: the Leaderboard scenario "My points
  follow my flashcards" stays valid for a card that is not archived; the banner and table scenarios get
  steps for the new columns; the Flashcards UI scenarios get the progress and the archived count.

## Risks / Trade-offs

- **Cheap points** (a card created and archived at once gives +4) → accepted by the product decision; no
  guard is added. The only cost is a few clicks.
- **The ranking query reads more** (two counts and two maxima per user) → still one aggregate query over
  active users, as the existing design accepted; the cost grows with the number of users, not cards.
- **Archived cards and the window of 100** → the active list shows only active cards, so the window is no
  longer used up by learned cards; the archived view has its own cap (the same 100 newest) which is an
  accepted limit.
- **PostgreSQL vs in-memory provider** → the new aggregate (conditional counts, max of two columns) is
  tested only on the in-memory provider, so translation problems on PostgreSQL would not show in tests.
  Mitigation: keep the query to simple `Count(predicate)` and `Max` expressions and run the migration
  and the page once against PostgreSQL before archiving the change.
- **Rounding down hides the last card** (99% for 199 of 200) → intended, see D3.
- **Scenario titles select the test user** → avoid "empty", "admin" and "demo" in new titles (see Context).
- **Wiki becomes stale** (flashcard list, deletion, leaderboard, flashcard entity) → covered by the
  wiki-ingest step required when archiving the change.

## Migration Plan

1. Add the `archived_at` column through an EF Core migration (nullable, no default, no data backfill).
2. Apply the migration to the production database **by hand before deploying** the new code: the
   application does not run migrations at startup (it only checks connectivity, and in test mode it uses
   `EnsureCreated`) and the deployment workflow has no migration step. New code against a database
   without the column fails on every flashcard query. The column is nullable, so old rows stay active and
   the previous version keeps working after the migration.
3. Rollback: deploy the previous version (it ignores the column). The column can be dropped by the
   migration's `Down` if needed; archived cards would then become active cards again and the leaderboard
   points would fall by 3 per archived card.
