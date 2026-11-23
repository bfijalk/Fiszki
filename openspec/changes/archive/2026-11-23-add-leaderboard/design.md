# Design

## Context

Requirements are in `specs/leaderboard/spec.md`; motivation in `proposal.md`.

- Blazor Server; pages call scoped services directly, with no HTTP API. Protected pages use
  `@attribute [Authorize]` and read the current user from `AuthenticationStateProvider`
  (see `Components/Pages/Flashcards.razor`).
- `IFlashcardService` and `IUserService` work for a single user; there is no cross-user query today.
- `User` has only `Email` as identifier. `TotalCardsGenerated`/`TotalCardsAccepted` are never updated
  and must not be used as points.
- `Flashcard` has `CreatedAt`, `UserId` (indexed) and cascade delete; there is no soft-delete.
- Unit tests use the EF Core InMemory provider; production uses PostgreSQL.

## Goals / Non-Goals

**Goals:**
- One service that returns everything the page needs (page of rows plus the current user's banner
  data) from a single ranking computation, so list and banner always agree.
- A query that works on both PostgreSQL and the InMemory provider, so ranking logic is unit-testable.

**Non-Goals:**
- Caching, materialized views or real-time push updates; the page shows the state at load and when
  the user changes page.
- Any schema change or backfill.

## Decisions

### D1. Points are derived, not stored
Count `flashcards` rows per user at query time. No new column, no migration, and deletions are
reflected automatically.
Alternatives: reuse `TotalCardsGenerated` (never updated, would need wiring into create, save and
delete paths, can drift); a dedicated counter column (migration plus drift risk). Rejected.

### D2. Rank in application memory over a small aggregate projection
The database returns one row per active user: `UserId`, `Email`, `Points` (count),
`LastCardAt` (max `CreatedAt`, null if none), `UserCreatedAt`. The service orders, assigns
positions, slices the page and derives the banner in memory.
Alternatives: SQL window functions (`ROW_NUMBER`) are not translatable on InMemory and would split
tests from production behavior; counting "users above me" with extra queries doubles round trips.
Cost: transfers one small row per active user; fine at the expected scale, and revisitable if user
count grows into the tens of thousands.

### D3. Total order gives unique positions
Order by: `Points` descending, then `LastCardAt` ascending (earlier means reached the score first),
then `UserCreatedAt` ascending, then `UserId`. The last two keys make the order total and stable,
including for 0-point users (whose `LastCardAt` is null and sorts after everyone with points
through the points key). Position = index + 1, so positions are never shared.
Note: `LastCardAt` is the newest remaining flashcard, so deleting the newest card moves it earlier;
accepted as part of "reached the score first".

### D4. Banner is computed from the full ranking
The current user's entry gives position and points; the rival is the entry directly above;
gap = rival points minus own points (can be 0 when the rival won the tie-break); "same points"
count = other active users with equal points. The banner never depends on the requested page.
An inactive current user cannot log in, so the current user is always in the ranking.

### D5. Masking
Mask on the server in the service DTO so the full email never reaches the component: keep the first
two characters of the local part (fewer if shorter), replace the rest with `***`, keep the domain
(`jan.kowalski@example.com` becomes `ja***@example.com`). Pure static function, unit-tested.
The service never exposes `Email` in the DTO; the current user is identified by `UserId` internally.

### D6. Service and page shape
New `ILeaderboardService` in `Fiszki.Services/Interfaces`, scoped, registered in
`AddFiszkiServices`. One method taking the current user id, page number and page size 30 returning
a DTO with: rows (position, masked email, points, is-current-user), total user count, page,
total pages, and banner data. Page `Components/Pages/Leaderboard.razor` at `/leaderboard` with
`[Authorize]`; page number is clamped to the valid range. Menu item added inside `<Authorized>` in
`NavMenu.razor` with an `id` and `data-testid` following the existing pattern
(`nav-leaderboard`), and stable test ids on the page for the functional tests.

### D7. Functional tests assert relative to the scenario's own user
All scenarios share one in-memory database that grows during a run, so tests never assert absolute
positions of other users. They assert the current user's row, points and banner, plus structural
properties: points sorted descending, positions unique and consecutive, rows per page, masked
emails. A `LeaderboardPage` page object uses the page's `data-testid` values and reaches the page
through the menu, because a full page reload drops the in-memory login. Scenarios are derived from
the scenarios in `specs/leaderboard/spec.md` and use the existing per-scenario test-user
assignment.

Pagination needs more than 30 active users. The test-mode seeder (`TestDataSeeder`) creates extra
zero-point users so the scenario is fast and deterministic. Alternative: register users through the
UI in a Given step, which keeps production code untouched but costs about 1 to 2 seconds per user.
These users rank above newly registered zero-point users, which relative assertions tolerate.

## Risks / Trade-offs

- [Ranking read cost grows with users] → one aggregated query without loading flashcards; revisit
  with caching or SQL-side paging if user count grows.
- [Masked emails can collide] → accepted; position and points are always shown, rows stay distinct.
- [Cross-user data exposure] → only masked email and totals leave the service; covered by a unit
  test asserting no full email in the DTO.
- [Deleting cards lowers rank, which may discourage cleanup] → accepted product decision.
- [Gaming by creating junk cards] → out of scope; no quality or duplicate checks.
- [Tie-break by `LastCardAt` uses card timestamps, and batch saves share close timestamps] →
  deterministic fallback keys in D3.
- [Functional tests that depend on other users' counts or positions would be flaky on the shared
  database] → relative assertions only (D7).
- [Extra seeded users change what existing scenarios see in test mode] → they own no flashcards and
  no existing scenario logs in as them or reads the user count; verified by running the full
  functional suite.
