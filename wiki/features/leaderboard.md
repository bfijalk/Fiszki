---
title: Leaderboard
type: feature
summary: How the leaderboard ranks active users by the number of flashcards they own (1 flashcard = 1 point): ordering and tie-break, masked emails, 30 users per page, the motivation banner, how points change, what the tests cover and what they leave to unit tests.
tags: [leaderboard, ranking, points, score, position, rank, rival, competition, motivation, banner, pagination, masked-email, gamification]
sources:
  - openspec/specs/leaderboard/spec.md
  - Components/Pages/Leaderboard.razor
  - Components/Layout/NavMenu.razor
  - Fiszki.Services/Interfaces/ILeaderboardService.cs
  - Fiszki.Services/Services/LeaderboardService.cs
  - Fiszki.Services/Models/LeaderboardDtos.cs
  - Fiszki.Services/Services/EmailMasker.cs
  - Fiszki.Services/Services/TestDataSeeder.cs
  - Fiszki.Services/DependencyInjection.cs
  - Fiszki.Tests/Services/LeaderboardServiceTests.cs
  - Fiszki.Tests/Services/EmailMaskerTests.cs
  - Fiszki.FunctionalTests/Features/Leaderboard.feature
  - Fiszki.FunctionalTests/PageObjects/LeaderboardPage.cs
  - Fiszki.FunctionalTests/Steps/LeaderboardSteps.cs
  - docs/TEST_SELECTORS.md
verified_commit: cdf3374897d4b0f5eab4affaaba6a6874064698e
confidence: medium
---

# Leaderboard

## Summary

A logged-in user opens **Leaderboard** from the side menu and sees all active users ranked by
points, where one flashcard they own is one point. Above the table a banner says where the user
stands and how far the nearest user above them is ("X has only 2 points more than you, generate
flashcards to catch up!"), to motivate them to create more flashcards. Users appear only as masked
emails. The ranking has 30 users per page.

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit test**,
**OpenSpec** (a requirement in `openspec/specs/leaderboard/spec.md`) or **code only**.

### Access

1. **Login is required.** The menu item is shown only to logged-in users and the page is protected
   like the other logged-in pages. Backed by: Gherkin (`Fiszki.FunctionalTests/Features/Leaderboard.feature`,
   scenarios "Leaderboard menu item is visible after login and opens the leaderboard", "Leaderboard
   menu item is hidden for visitors" and "Leaderboard page requires login") and OpenSpec
   (`openspec/specs/leaderboard/spec.md`, requirement "Leaderboard access and menu entry"). Where the
   item sits in the menu is in [[site-navigation]].

### Points

2. **A point is a flashcard the user currently owns.** AI-generated and manual flashcards count the
   same. Backed by: unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `AiAndManualFlashcardsCountEqually`) and OpenSpec (requirement "Points equal the user's flashcards").
3. **Creating a flashcard adds a point and deleting one removes it.** Points are counted from the
   flashcards table every time the page loads, so they always match the flashcards the user has.
   Backed by: Gherkin ("My points follow my flashcards") and unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `DeletingFlashcard_LowersPoints`). The user counters on the user record are not used, see
   [[user-entity]].

### Order and positions

4. **More points rank higher, and positions are unique** (1, 2, 3, ...), so two users never share a
   position. Backed by: Gherkin ("Ranking is sorted with unique consecutive positions") and OpenSpec
   (requirement "Ranking order with unique positions").
5. **On equal points the user who reached the score first ranks higher**, meaning the user whose newest
   remaining flashcard is older. If that is also equal, the older account ranks higher, then the lower
   user id. Backed by: unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Tie_IsBrokenByEarlierLastCard_AndPositionsAreUnique`).
   Because the date is that of the newest *remaining* flashcard, deleting a user's newest flashcard
   moves that date earlier. Backed by: code only (`Fiszki.Services/Services/LeaderboardService.cs` `GetAsync`).
6. **Users with 0 points are listed last**, still with unique positions. Backed by: Gherkin ("Ranking is
   sorted with unique consecutive positions") and unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `ZeroPointUsers_AreListedLast_WithUniquePositions`).
7. **Inactive users are not listed and take no position.** Backed by: unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `InactiveUsers_AreNotListedAndDoNotTakePositions`) and OpenSpec (requirement "Inactive users are excluded").
   Nothing in the application sets an account to inactive today, see [[user-entity]].

### Identity

8. **Users are shown only by a masked email.** The first two characters of the part before the "@",
   then `***`, then the domain: `jan.kowalski@example.com` is shown as `ja***@example.com`. The part
   before the "@" is never shown in full, so a two-character name keeps one character and a
   one-character name keeps none (`a@x.pl` becomes `***@x.pl`). Backed by: unit test
   (`Fiszki.Tests/Services/EmailMaskerTests.cs`), Gherkin ("Users are shown with masked emails") and
   OpenSpec (requirement "Masked user identity"). Two different users can produce the same mask; the
   position and points still tell the rows apart. The current user's own row is also masked and is
   marked "(you)".

### Pagination

9. **30 users per page**, with Previous and Next buttons and a "Page N of M" indicator. A page number
   outside the range is moved to the nearest valid page. Backed by: Gherkin ("Leaderboard is paginated
   by 30 users"), unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Paging_FirstAndLastPageSizes_AndClamping`)
   and OpenSpec (requirement "Paginated list").

### Motivation banner

10. **The banner shows the user's position and points**, and then one of three messages. Backed by:
    Gherkin ("Banner matches the ranking table" and "Banner for a user without flashcards"), unit tests
    (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Banner_RivalAhead_ShowsGapAndRivalMaskedEmail`, `Banner_Leader_HasNoRival`)
    and OpenSpec (requirement "Motivation banner"):
    - **Rival ahead with more points:** the nearest user above, by masked email, "has only N points more
      than you. Generate flashcards to catch up!".
    - **Rival above with the same points** (they reached the score first): "has the same number of points
      but reached them first. Create one more flashcard to overtake them!".
    - **Leader (position 1):** "You lead the ranking. Keep creating flashcards to stay ahead!".
11. **The banner also counts the other users with exactly the same points** ("You also have N rivals with
    the same number of points") when there are any. Backed by: Gherkin ("Banner for a user without
    flashcards") and unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Banner_SamePointsCount_CountsOtherUsersWithEqualPoints`).
12. **The banner does not depend on the page being viewed.** It is computed from the whole ranking, so a
    user on page 2 still sees their own position and rival. A "Generate flashcards" button in the
    banner opens the Generate page. Backed by: Gherkin ("Leaderboard is paginated by 30 users", the
    step "the banner should still be shown") and code only (`Components/Pages/Leaderboard.razor`
    `GoToGenerate`).

### Loading and errors

13. **While loading, a spinner is shown; if loading fails, the message "The leaderboard could not be
    loaded. Please try again later." is shown** instead of an error page. Backed by: code only
    (`Components/Pages/Leaderboard.razor` `LoadPage`); no test makes the service fail.
14. **The ranking is a snapshot.** It is read when the page opens and when the user changes page; it
    does not update by itself. Backed by: code only (`Components/Pages/Leaderboard.razor` `LoadPage`).

## User flow

1. A logged-in user clicks Leaderboard in the side menu.
2. The page loads the ranking and shows the banner and the first 30 users.
3. The user reads the gap to the user above and either goes to Generate or Flashcards to create more
   flashcards, or moves between pages.
4. After creating or deleting flashcards, the user opens the leaderboard again and sees the new points
   and position.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **UI:** `Components/Pages/Leaderboard.razor` (`@page "/leaderboard"`, `Authorize`) shows the banner, the table
  and the pagination; `LoadPage` calls the service and handles the loading and error states. The menu
  item is in `Components/Layout/NavMenu.razor` (`AuthorizeView`, test id `nav-leaderboard`).
- **Service:** `Fiszki.Services/Services/LeaderboardService.cs` `GetAsync` (interface
  `Fiszki.Services/Interfaces/ILeaderboardService.cs`, registered as scoped in
  `Fiszki.Services/DependencyInjection.cs` `AddFiszkiServices`). One query returns one row per active user
  with the number of flashcards and the newest flashcard's creation time; the ranking, positions, page slice
  and banner are then computed in memory, so the list and the banner always agree. `PageSize` is 30.
  The current user is found by id; if the user is not an active user the service throws
  `DomainNotFoundException`, which cannot happen for a logged-in user because inactive accounts cannot log in.
- **DTOs:** `Fiszki.Services/Models/LeaderboardDtos.cs` (`LeaderboardRowDto`, `LeaderboardBannerDto`,
  `LeaderboardPageDto`) carry only the masked email, never the full address.
- **Masking:** `Fiszki.Services/Services/EmailMasker.cs` `Mask`, applied in the service.
- **Data:** read-only over `users` and `flashcards`; no schema change and no migration, see
  [[flashcard-entity]] and [[user-entity]]. Cost grows with the number of active users because every call
  reads one aggregate row per active user.
- **Test data:** in test mode `Fiszki.Services/Services/TestDataSeeder.cs` `CreateLeaderboardFillerUsers`
  adds 31 active users without flashcards (`leaderboard.filler01@example.com` and so on) so the leaderboard has
  a second page. They share one password hash and are not meant to log in.
- **Unit tests:** `Fiszki.Tests/Services/LeaderboardServiceTests.cs` (ordering, ties, paging, banner, masking in the
  result, inactive users, service registration) and `Fiszki.Tests/Services/EmailMaskerTests.cs`. They run on the
  in-memory database provider.
- **Test selectors:** `leaderboard-page`, `leaderboard-banner` and its parts, `leaderboard-table`,
  `leaderboard-row` (with `data-position` and `data-current-user`), `leaderboard-prev`, `leaderboard-next`,
  `leaderboard-page-indicator`; listed in `docs/TEST_SELECTORS.md`.

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/Leaderboard.feature` has 9 scenarios: menu item for a logged-in user,
  hidden for visitors, protected page, points following flashcards (create then delete), sorted unique positions,
  masked emails, pagination, banner matching the table, and the banner for a user without flashcards. Page object:
  `Fiszki.FunctionalTests/PageObjects/LeaderboardPage.cs` `GetBannerAsync`; steps:
  `Fiszki.FunctionalTests/Steps/LeaderboardSteps.cs`.
- **Relative assertions only.** All scenarios share one database that other scenarios also change, so they assert on
  the logged-in user's own row, points and banner and on structure (sorted points, unique positions, rows per page),
  never on other users' absolute positions.
- **Left to unit tests** because the UI cannot set up the data: the tie-break by who reached the score first, AI and
  manual flashcards counting equally, inactive users, and the banner for the leader.
- **Not tested at all:** the loading and error states, and running the ranking against PostgreSQL (the tests use the
  in-memory provider), so the query's behavior on the real database is not verified.

## Dependencies and side effects

- Reads the flashcards of all users but exposes only counts, never content, see [[flashcard-ownership]].
- Points change when flashcards are created or deleted: [[manual-flashcard-creation]], [[ai-flashcard-generation]],
  [[flashcard-deletion]].
- Uses `Email`, `IsActive` and `CreatedAt` of [[user-entity]]; it does not use the `TotalCardsGenerated` and
  `TotalCardsAccepted` counters.
- The login state comes from [[authentication-session]]; the menu entry and its position are in [[site-navigation]].
- The banner's button leads to Generate, which needs an OpenRouter API key, see [[openrouter-integration]].

## Discrepancies

- **Masking of short names.** The archived design (`openspec/changes/archive/2026-11-23-add-leaderboard/design.md`,
  decision D5) says to keep "the first two characters of the local part (fewer if shorter)". The code never shows the
  local part in full: a one-character name keeps none and a two-character name keeps one
  (`Fiszki.Services/Services/EmailMasker.cs` `Mask`). The spec only requires "a masked form". The code wins.
- **Wording for 0 points.** For a user with 0 points the banner says the rival above "reached" the same points first,
  which is odd when nobody has any points (`Components/Pages/Leaderboard.razor`). Behavior as written is described
  above; it was noticed during implementation and not changed.

## Related

- [[site-navigation]]
- [[flashcard-entity]]
- [[user-entity]]
- [[flashcard-ownership]]
- [[flashcard-deletion]]
- [[manual-flashcard-creation]]
- [[ai-flashcard-generation]]
- [[application-layers]]
- [[glossary]]
