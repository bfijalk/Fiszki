# Tasks

## 1. Menu

- [x] 1.1 Reorder `Components/Layout/NavMenu.razor` as in design D1 and add the two `<hr class="nav-separator">` separators with the test ids `nav-separator-account` and `nav-separator-logout`, plus the `.nav-separator` rule in `NavMenu.razor.css` (design D2); verify `dotnet build` succeeds and, in the running app, that a logged-in user sees Home, Flashcards, Generate, Leaderboard, separator, How to Start, Examples, About Technicalities, About Me, separator, Logout, that a visitor sees Home, How to Start, Examples, About Technicalities, About Me, Login without separators, that the active-link highlight and the top and bottom padding of the menu are unchanged, and that the menu is correct again after Logout

## 2. Functional tests

- [x] 2.1 Add a `NavBar` method that returns the ordered entries of `nav.flex-column` in one browser call (the link text for items and a marker for each separator); verify `dotnet build` of `Fiszki.FunctionalTests` succeeds
- [x] 2.2 Add two scenarios to `Fiszki.FunctionalTests/Features/Navigation.feature`, one for the visitor menu order (no login) and one for the logged-in menu order (default test user), with step definitions in `NavigationSteps.cs` comparing the whole sequence; no other scenarios are added (design D3); verify both pass locally, commit the regenerated `Navigation.feature.cs`, and report their durations to confirm they add roughly 10 seconds to the suite
- [x] 2.3 Update the menu groups and order in `docs/TEST_SELECTORS.md` and document `nav-separator-account` and `nav-separator-logout`; verify every `data-testid` added in 1.1 appears in the document

## 3. Integration checks

- [x] 3.1 Run `dotnet test` for `Fiszki.Tests` and the whole functional suite the way CI does (`TestMode=true`, launch profile) and verify no regressions in scenarios that click menu items; the two AI-generation scenarios need an OpenRouter API key and may fail locally without it, so state which scenarios failed and why

## Workflow follow-up

- Archive the change after review; wiki-ingest runs last in archive mode (see `openspec/config.yaml`),
  which also refreshes the wiki page `site-navigation`.
