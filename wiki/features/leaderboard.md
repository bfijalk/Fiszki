---
title: Leaderboard
type: feature
summary: How the leaderboard ranks active users by points (1 per flashcard plus 3 per archived "learned" flashcard, so an archived card is worth 4): the Flashcards, Archived, Learning progress and Points columns, ordering and tie-break by the latest creation or archiving, masked emails, 30 users per page, the motivation banner, how points change, what the tests cover.
tags: [leaderboard, ranking, points, score, position, rank, rival, competition, motivation, banner, pagination, masked-email, gamification, archived, learned, learning-progress, bonus, tie-break]
sources:
  - openspec/specs/leaderboard/spec.md
  - Components/Pages/Leaderboard.razor
  - Components/Layout/NavMenu.razor
  - Fiszki.Services/Interfaces/ILeaderboardService.cs
  - Fiszki.Services/Services/LeaderboardService.cs
  - Fiszki.Services/Models/LeaderboardDtos.cs
  - Fiszki.Services/Services/EmailMasker.cs
  - Fiszki.Services/Services/LearningProgressCalculator.cs
  - Fiszki.Services/Services/TestDataSeeder.cs
  - Fiszki.Services/DependencyInjection.cs
  - Fiszki.Tests/Services/LeaderboardServiceTests.cs
  - Fiszki.Tests/Services/EmailMaskerTests.cs
  - Fiszki.FunctionalTests/Features/Leaderboard.feature
  - Fiszki.FunctionalTests/PageObjects/LeaderboardPage.cs
  - Fiszki.FunctionalTests/Steps/LeaderboardSteps.cs
  - docs/TEST_SELECTORS.md
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: medium
---

# Leaderboard

## Summary

A logged-in user opens **Leaderboard** from the side menu and sees all active users ranked by
points. One flashcard a user owns is one point and every flashcard the user has archived as learned
("I learned it", see [[flashcard-archiving]]) is **3 more**, so an archived flashcard is worth 4. Each row shows
the number of flashcards, the number of archived flashcards, the learning progress percentage and the points. Above
the table a banner says where the user stands, with the same numbers, and how far the nearest user above is ("X has
only 2 points more than you. Create or learn flashcards to catch up!"), to motivate them to create and to learn more
flashcards. Users appear only as masked emails. The ranking has 30 users per page.

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

2. **Points = flashcards the user currently owns + 3 for each of them that is archived.** AI-generated and
   manual flashcards count the same, so an archived flashcard is worth 4 points. Backed by: unit test
   (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `AiAndManualFlashcardsCountEqually`, `ArchivedFlashcard_IsWorthFourPoints`)
   and OpenSpec (`openspec/specs/leaderboard/spec.md`, requirement "Points for flashcards and archived flashcards").
   The 3-point bonus is the constant `LeaderboardService.ArchivedBonus`.
3. **Points always follow the current state.** Creating a flashcard adds 1 point, archiving it adds 3 more,
   restoring it takes the 3 back, and deleting a card takes all it was worth: 1 point, or 4 if it was archived.
   Points are counted from the flashcards table every time the page loads. Backed by: Gherkin ("My points follow my
   flashcards", "Archiving, restoring and deleting an archived flashcard move my points") and unit test
   (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `DeletingFlashcard_LowersPoints`, `RestoringArchivedFlashcard_RemovesExtraPoints`,
   `DeletingArchivedFlashcard_RemovesFourPoints`). The user counters on the user record are not used, see
   [[user-entity]]. A card created and archived at once gives 4 points; the product accepted this.

### Order and positions

4. **More points rank higher, and positions are unique** (1, 2, 3, ...), so two users never share a
   position. Backed by: Gherkin ("Ranking is sorted with unique consecutive positions") and OpenSpec
   (requirement "Ranking order with unique positions").
5. **On equal points the user who reached the score first ranks higher.** The moment a user reached the score is
   the **later of their newest flashcard's creation time and their newest archiving time**: creating and archiving
   are the two events that change the score. If that is also equal, the older account ranks higher, then the lower
   user id. Backed by: unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Tie_IsBrokenByEarlierLastCard_AndPositionsAreUnique`,
   `ArchivingAfterRival_LosesTheTie`, `CreationOnlyTies_StillUseNewestFlashcard`) and OpenSpec (requirement
   "Ranking order with unique positions"). The dates are taken from the cards the user *still has*, so deleting
   or restoring a card can move the date earlier. Backed by: code only (`Fiszki.Services/Services/LeaderboardService.cs` `GetAsync`).
   Archiving an old card late makes the user the later one in a tie.
6. **Users with 0 points are listed last**, still with unique positions. Backed by: Gherkin ("Ranking is
   sorted with unique consecutive positions") and unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `ZeroPointUsers_AreListedLast_WithUniquePositions`).
7. **Inactive users are not listed and take no position.** Backed by: unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
   `InactiveUsers_AreNotListedAndDoNotTakePositions`) and OpenSpec (requirement "Inactive users are excluded").
   Nothing in the application sets an account to inactive today, see [[user-entity]].

### Columns and learning progress

7a. **Each row shows the flashcards, the archived flashcards, the learning progress and the points** of that user.
    Points are always flashcards plus 3 per archived flashcard. The **learning progress** is the archived share of all
    the user's flashcards in whole percent, rounded down, 0 for a user without flashcards, computed by the same rule as on
    the Flashcards page (`Fiszki.Services/Services/LearningProgressCalculator.cs` `Percent`), so both show the same number.
    Backed by: Gherkin (`Fiszki.FunctionalTests/Features/Leaderboard.feature`, scenario "Rows show flashcards, archived flashcards, learning progress and points", which checks every row on the page against the formula), unit test
    (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `ArchivedFlashcard_IsWorthFourPoints`, `ProgressInLeaderboard_EqualsFlashcardSummary`)
    and OpenSpec (requirement "Paginated list").
7b. **The learning progress is information only.** It is shown in its own muted column and is never used to
    order users or to compute points: a user with more points and a lower percentage ranks higher. Backed by: unit test
    (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `LearningProgress_NeverChangesTheOrder`) and OpenSpec
    (requirement "Ranking order with unique positions").

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

10. **The banner shows the user's position and points, the user's own flashcards, archived flashcards and learning
    progress**, and then one of three messages. Backed by: Gherkin ("Banner matches the ranking table", "Banner for a
    user without flashcards" and "Rows show flashcards, archived flashcards, learning progress and points", which compares
    the banner with the user's own row), unit tests (`Fiszki.Tests/Services/LeaderboardServiceTests.cs`
    `Banner_RivalAhead_ShowsGapAndRivalMaskedEmail`, `Banner_Leader_HasNoRival`, `Banner_ShowsOwnNumbers`) and OpenSpec
    (requirement "Motivation banner"):
    - **Rival ahead with more points:** the nearest user above, by masked email, "has only N points more
      than you. Create or learn flashcards to catch up!".
    - **Rival above with the same points** (they reached the score first): "has the same number of points
      but reached them first. Create or learn one more flashcard to overtake them!".
    - **Leader (position 1):** "You lead the ranking. Keep creating and learning flashcards to stay ahead!".
11. **The banner also counts the other users with exactly the same points** ("You also have N rivals with
    the same number of points") when there are any. Backed by: Gherkin ("Banner for a user without
    flashcards") and unit test (`Fiszki.Tests/Services/LeaderboardServiceTests.cs` `Banner_SamePointsCount_CountsOtherUsersWithEqualPoints`).
12. **The banner does not depend on the page being viewed.** It is computed from the whole ranking, so a
    user on page 2 still sees their own position and rival. A "Generate flashcards" button in the
    banner opens the Generate page (its label was not changed to mention learning). Backed by: Gherkin ("Leaderboard is paginated by 30 users", the
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
   flashcards, marks some as learned ("I learned it"), or moves between pages.
4. After creating, archiving, restoring or deleting flashcards, the user opens the leaderboard again and sees the
   new points, counts, learning progress and position.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **UI:** `Components/Pages/Leaderboard.razor` (`@page "/leaderboard"`, `Authorize`) shows the banner, the table
  and the pagination; `LoadPage` calls the service and handles the loading and error states. The menu
  item is in `Components/Layout/NavMenu.razor` (`AuthorizeView`, test id `nav-leaderboard`).
- **Service:** `Fiszki.Services/Services/LeaderboardService.cs` `GetAsync` (interface
  `Fiszki.Services/Interfaces/ILeaderboardService.cs`, registered as scoped in
  `Fiszki.Services/DependencyInjection.cs` `AddFiszkiServices`). One query returns one row per active user
  with the number of flashcards, the number of archived flashcards, and the newest creation and newest archiving times;
  points (`flashcards + ArchivedBonus * archived`), the learning progress and the moment the score was reached
  (the later of the two times) are then computed in memory, together with the ranking, positions, page slice
  and banner, so the list and the banner always agree. `PageSize` is 30 and `ArchivedBonus` is 3.
  The current user is found by id; if the user is not an active user the service throws
  `DomainNotFoundException`, which cannot happen for a logged-in user because inactive accounts cannot log in.
- **DTOs:** `Fiszki.Services/Models/LeaderboardDtos.cs` (`LeaderboardRowDto`, `LeaderboardBannerDto`,
  `LeaderboardPageDto`) carry only the masked email, never the full address. Rows and the banner carry
  `Flashcards`, `Archived` and `LearningProgress` besides the points.
- **Masking:** `Fiszki.Services/Services/EmailMasker.cs` `Mask`, applied in the service.
- **Data:** read-only over `users` and `flashcards` (including the `archived_at` column, see [[flashcard-entity]]); the
  ranking itself needs no schema of its own, see [[user-entity]]. Cost grows with the number of active users because every call
  reads one aggregate row per active user.
- **Test data:** in test mode `Fiszki.Services/Services/TestDataSeeder.cs` `CreateLeaderboardFillerUsers`
  adds 31 active users without flashcards (`leaderboard.filler01@example.com` and so on) so the leaderboard has
  a second page. They share one password hash and are not meant to log in.
- **Unit tests:** `Fiszki.Tests/Services/LeaderboardServiceTests.cs` (ordering, ties including archiving as a score event,
  points of archived cards, learning progress, paging, banner, masking in the result, inactive users, service registration) and `Fiszki.Tests/Services/EmailMaskerTests.cs`. They run on the
  in-memory database provider.
- **Test selectors:** `leaderboard-page`, `leaderboard-banner` and its parts (including `leaderboard-banner-own`,
  `leaderboard-banner-flashcards`, `leaderboard-banner-archived`, `leaderboard-banner-progress`), `leaderboard-table`,
  `leaderboard-row` (with `data-position` and `data-current-user`) and its cells (`leaderboard-flashcards`,
  `leaderboard-archived`, `leaderboard-progress`, `leaderboard-points`), `leaderboard-prev`, `leaderboard-next`,
  `leaderboard-page-indicator`; listed in `docs/TEST_SELECTORS.md`.

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/Leaderboard.feature` has 11 scenarios: menu item for a logged-in user,
  hidden for visitors, protected page, points following flashcards (create then delete), points through archive,
  restore and deleting an archived card (+1, +4, +1, back to the start), rows and banner following the points formula and
  the progress rule, sorted unique positions, masked emails, pagination, banner matching the table, and the banner for a
  user without flashcards. Page object:
  `Fiszki.FunctionalTests/PageObjects/LeaderboardPage.cs` `GetBannerAsync`; steps:
  `Fiszki.FunctionalTests/Steps/LeaderboardSteps.cs`.
- **Relative assertions only.** All scenarios share one database that other scenarios also change, so they assert on
  the logged-in user's own row, points and banner and on structure (sorted points, unique positions, rows per page),
  never on other users' absolute positions.
- **Left to unit tests** because the UI cannot set up the data: the tie-break by who reached the score first (including
  archiving as a score event), AI and manual flashcards counting equally, the learning progress never changing the order,
  inactive users, and the banner for the leader.
- **Not tested at all:** the loading and error states, and running the ranking against PostgreSQL (the automated tests use the
  in-memory provider). The new query with the archived counts and the two maxima was run once by hand against PostgreSQL 14
  and gave the expected points, order and percentages; that check is not kept in the repository.

## Dependencies and side effects

- Reads the flashcards of all users but exposes only counts, never content, see [[flashcard-ownership]].
- Points change when flashcards are created, archived, restored or deleted: [[manual-flashcard-creation]],
  [[ai-flashcard-generation]], [[flashcard-archiving]], [[flashcard-deletion]].
- Uses `ArchivedAt` and `CreatedAt` of [[flashcard-entity]] and `Email`, `IsActive` and `CreatedAt` of [[user-entity]]; it does not use the `TotalCardsGenerated` and
  `TotalCardsAccepted` counters.
- The login state comes from [[authentication-session]]; the menu entry and its position are in [[site-navigation]].
- The banner's button leads to Generate, which needs an OpenRouter API key, see [[openrouter-integration]].

## Discrepancies

- **Masking of short names.** The archived design (`openspec/changes/archive/2026-11-23-add-leaderboard/design.md`,
  decision D5) says to keep "the first two characters of the local part (fewer if shorter)". The code never shows the
  local part in full: a one-character name keeps none and a two-character name keeps one
  (`Fiszki.Services/Services/EmailMasker.cs` `Mask`). The spec only requires "a masked form". The code wins.
- **Stale purpose in the main spec.** The `## Purpose` of `openspec/specs/leaderboard/spec.md` still says the ranking is by
  "the number of flashcards they own". The requirements below it (and the code) say points are flashcards plus 3 per archived
  flashcard. Syncing a change never rewrites an existing purpose, so the code and the requirements win.
- **Wording for 0 points.** For a user with 0 points the banner says the rival above "reached" the same points first,
  which is odd when nobody has any points (`Components/Pages/Leaderboard.razor`). Behavior as written is described
  above; it was noticed during implementation and not changed.

## Related

- [[flashcard-archiving]]
- [[site-navigation]]
- [[flashcard-entity]]
- [[user-entity]]
- [[flashcard-ownership]]
- [[flashcard-deletion]]
- [[manual-flashcard-creation]]
- [[ai-flashcard-generation]]
- [[application-layers]]
- [[glossary]]
