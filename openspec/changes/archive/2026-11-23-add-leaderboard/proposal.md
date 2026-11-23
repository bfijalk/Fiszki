# Proposal

## Why

Fiszki gives users no reason to keep creating flashcards after the first session. A leaderboard
that ranks users by the number of flashcards they own, and tells each user how far they are
from the person directly above them, turns that into a visible goal ("X has only 2 points more
than you, generate flashcards to catch up!").

## What Changes

- Add a **Leaderboard** item to the side menu, visible only to logged-in users, that opens a new
  `/leaderboard` page protected like the other logged-in pages.
- The page ranks all **active** users by points: **1 flashcard = 1 point**, AI-generated and
  manually created flashcards count the same. Points are the live count of the user's flashcards,
  so deleting a flashcard removes a point.
- Positions are **unique** (1, 2, 3, ...). Users with equal points are ordered by who reached that
  score first (the one whose most recent flashcard is older ranks higher). Users with 0 points are
  listed last.
- Users are shown by a **masked email**, never the full address.
- The list is paginated, **30 users per page**.
- A motivation banner above the list shows the current user's position and points, the nearest user
  above them with the point gap, a call to generate flashcards to catch up, and how many other users
  have exactly the same number of points. The leader and users with no rival above get a different
  message.
- Inactive users (`IsActive = false`) are not listed and are not counted.
- Introduce a service that computes the ranking; no database migration (points are derived from
  the existing `flashcards` table).
- Add functional tests for the leaderboard (Gherkin scenarios with SpecFlow and Playwright, a page
  object and step definitions), shipped in the same PR as the feature. Test mode gets extra seeded
  users so the pagination scenario has more than 30 active users.

Out of scope: nicknames or display names, caching or materialized rankings, time-boxed
(weekly/monthly) rankings, and filtering by creation source.

## Capabilities

### New Capabilities
- `leaderboard`: ranking of active users by flashcard count: points rule, ordering and tie-break,
  masked identity, pagination, the motivation banner, access for logged-in users, and the menu entry.

### Modified Capabilities

None. Existing specs cover only the wiki; the application's behavior is documented in `wiki/`,
which is refreshed by wiki-ingest after archive.

## Impact

- **UI:** `Components/Layout/NavMenu.razor` (new menu item), new page `Components/Pages/Leaderboard.razor`
  and supporting components.
- **Services:** new `ILeaderboardService` and implementation in `Fiszki.Services`, registered in
  `DependencyInjection.cs`; new DTOs.
- **Database:** read-only query over `flashcards` and `users` (grouping by `UserId`); no schema
  change.
- **Tests:** unit tests for ranking, ties, pagination and masking; functional tests in
  `Fiszki.FunctionalTests` (new `Leaderboard.feature` with its generated code-behind, a
  `LeaderboardPage` page object, step definitions); extra users in the test-mode seeder
  (`TestDataSeeder`); updated test-selector and functional-test documentation.
- **Privacy:** first feature that shows other users' data. Only masked emails and point totals are
  exposed. Two different users can produce the same mask; accepted.
- **Wiki:** pages for the new feature and the updated navigation will be ingested after archive.
