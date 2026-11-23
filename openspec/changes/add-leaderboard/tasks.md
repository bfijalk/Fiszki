# Tasks

## 1. Ranking service

- [x] 1.1 Add leaderboard DTOs (row, banner data, page result) in `Fiszki.Services/Models` that carry no full email; verify the project builds with `dotnet build`
- [x] 1.2 Add the email masking function (first two characters of the local part, `***`, domain kept; shorter local parts handled) and verify unit tests for normal, short and one-character local parts pass
- [x] 1.3 Add `ILeaderboardService` and its implementation: aggregated query over active users (points, last card time, user creation time), total ordering from design D3, unique positions, 30-per-page slicing with clamped page number, and banner data from design D4; verify with unit tests on the InMemory provider (ordering, tie broken by earlier last card, zero-point users last, inactive users excluded, deleting a card lowers points, page 1 and last page sizes, leader, rival gap, same-points count, no full email in the result)
- [x] 1.4 Register the service in `Fiszki.Services/DependencyInjection.cs` and verify the application starts and resolves it (`dotnet build` and a service-resolution unit test)

## 2. UI

- [x] 2.1 Add the "Leaderboard" menu item inside `<Authorized>` in `Components/Layout/NavMenu.razor` with `id`/`data-testid` `nav-leaderboard`; verify it is shown when logged in, hidden when logged out, and navigates to `/leaderboard`
- [x] 2.2 Add `Components/Pages/Leaderboard.razor` (`/leaderboard`, `[Authorize]`) showing the table (position, masked email, points, highlighted current user row), pagination controls, and the motivation banner variants (rival ahead, tie-break rival, same-points rivals, leader); add stable `data-testid` attributes for each element; verify manually in the running app (use the `run` skill) against the spec scenarios
- [x] 2.3 Handle loading and error states on the page and verify that a service failure shows a message instead of an unhandled exception

## 3. Functional tests

- [x] 3.1 Add a `LeaderboardPage` page object in `Fiszki.FunctionalTests/PageObjects` (menu item, banner and its parts, rows, current-user row, pagination controls) using the `data-testid` values from 2.1 and 2.2, reaching the page through the menu; verify `dotnet build` succeeds
- [x] 3.2 Seed more than 30 active zero-point users in test mode only (in `TestDataSeeder`, per design D7) and verify the app in test mode shows 30 rows on page 1 of the leaderboard and that existing seeded users and flashcard counts are unchanged
- [x] 3.3 Add `Features/Leaderboard.feature` with scenarios derived from `specs/leaderboard/spec.md`: menu item visible when logged in and hidden when logged out, `/leaderboard` protected, creating a flashcard adds a point and deleting removes one, ordering and unique positions, zero-point users listed last, masked emails with no full address, pagination (30 on page 1, next page, controls disabled at the ends), and the banner variants (leader, rival ahead, same-points rivals); verify each spec scenario is covered by a feature scenario or is deliberately left to unit tests and say which
- [x] 3.4 Add the step definitions for the feature, reusing existing login, navigation and flashcard steps where they fit and asserting only relative to the scenario's own user (design D7); verify the new feature passes locally with `dotnet test` in `Fiszki.FunctionalTests` and commit the regenerated `.feature.cs`
- [x] 3.5 Document the leaderboard test ids in `docs/TEST_SELECTORS.md` and the new coverage in the functional tests README, and verify every `data-testid` used by the page object appears in the documentation

## 4. Integration checks

- [x] 4.1 Run the full existing test suite (`dotnet test`) and verify no regressions
- [x] 4.2 Verify end to end in the running app with seeded users: ranking order, pagination at 30, creating and deleting a flashcard changes points and the banner, and no full email appears anywhere on the page
- [ ] 4.3 Run the whole functional suite the way CI does (`ASPNETCORE_ENVIRONMENT=Test`, `TestMode=true`, `dotnet run --environment Test`) and verify all scenarios pass, including that the `Test` environment serves the static assets (`blazor.web.js` returned 404 in an earlier manual run)

## Workflow follow-up

- Archive the change after review; wiki-ingest runs last in archive mode (see `openspec/config.yaml`).
