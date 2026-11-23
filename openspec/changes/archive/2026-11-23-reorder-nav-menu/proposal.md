# Proposal

## Why

The side menu appends everything available after login (Flashcards, Generate, and now Leaderboard)
to the end of the list, below five links that every visitor sees. The features a logged-in user
actually works with are the furthest from the top, and every new feature pushes them further down.

## What Changes

- **Home is always the first item** of the side menu, for visitors and logged-in users.
- For a **logged-in user**, the items available only after login (Flashcards, Generate, Leaderboard)
  come directly after Home, above the items that every visitor sees (How to Start, Examples,
  About Technicalities, About Me).
- A **separator** (a thin horizontal line) divides the logged-in-only group from the public group.
- **Logout** is the last item of the menu, below the public items and separated from them by a
  second separator.
- A **visitor who is not logged in** sees the same menu as today: Home, the public items, then Login.
  No separator is shown to visitors.
- Menu labels, targets, icons and test ids do not change; only the order and the separators do.

Out of scope: changing which items are shown to whom, renaming or adding menu items, moving the
Login item, section headings or collapsible groups, and the layout and styling of the menu beyond
the separators.

## Capabilities

### New Capabilities
- `site-navigation`: order and grouping of the side menu items for visitors and logged-in users:
  Home first, logged-in items above public items, separators, and Logout last.

### Modified Capabilities

None. No existing spec describes the menu. The leaderboard menu item itself is specified in the
`add-leaderboard` change, which does not define its position, so the two do not conflict.

## Impact

- **UI:** `Components/Layout/NavMenu.razor` (reorder items, add separators; possibly a rule in
  `NavMenu.razor.css`).
- **Tests:** a functional scenario for the menu order in `Fiszki.FunctionalTests` (the existing
  `Navigation.feature` has one scenario and its steps find links by text, so they do not depend on
  order).
- **Docs and wiki:** the menu groups in `docs/TEST_SELECTORS.md` and the wiki page
  `site-navigation` (rules 2 to 4) describe the old order; the wiki is refreshed by wiki-ingest
  after archive.
