# Design

## Context

Requirements are in `specs/site-navigation/spec.md`; motivation in `proposal.md`.

- The whole menu is `Components/Layout/NavMenu.razor`: a `<nav class="flex-column">` whose children
  are `div.nav-item` blocks. Today the five public items come first and one `AuthorizeView` at the end
  holds Flashcards, Generate, Leaderboard and Logout (`Authorized`) or Login (`NotAuthorized`).
- `NavMenu.razor.css` (scoped) styles `.nav-item` and uses `.nav-item:first-of-type` (extra top
  padding) and `.nav-item:last-of-type` (extra bottom padding). Both are evaluated per element type
  among siblings, so any new sibling that is also a `div` would change which item gets that padding.
- Functional tests find menu links by text (`NavBar.GetLink`) or by `data-testid`; nothing depends on
  the order.

## Goals / Non-Goals

**Goals:**
- Order and separators exactly as in the spec, with no change to labels, targets, icons or ids.
- Keep the visitor menu rendering identical to today.

**Non-Goals:**
- A data-driven menu (a list of items rendered in a loop). Eight static links do not justify it.
- Section headings, collapsible groups or restyling beyond the separators.

## Decisions

### D1. Two `AuthorizeView` blocks around the public items
Markup order inside `<nav>`: Home; `AuthorizeView` with `Authorized` holding Flashcards, Generate,
Leaderboard and the first separator; the public items (How to Start, Examples, About Technicalities,
About Me); a second `AuthorizeView` with `Authorized` holding the second separator and Logout, and
`NotAuthorized` holding Login. Visitors get Home, the public items and Login, exactly as today.
Alternatives: one `AuthorizeView` and CSS `order` to reorder items (hides the real order from the
DOM, screen readers and tests, so rejected); duplicating the public items per state (more markup to
keep in sync, rejected). The authentication state is cascaded, so the second `AuthorizeView` costs
nothing extra.

### D2. Separators are `<hr>` elements with a scoped CSS class
Use `<hr class="nav-separator" data-testid="...">` as a direct child of `<nav>`. Because it is not a
`div`, `.nav-item:first-of-type` and `:last-of-type` keep matching Home and the last item
(Logout for a logged-in user, About Me or Login for a visitor) and the existing padding is unchanged.
A rule for `.nav-separator` in `NavMenu.razor.css` gives it a faint light line (low-opacity white, as
the hover colour already used there) and small horizontal and vertical margins. Test ids:
`nav-separator-account` (after Leaderboard) and `nav-separator-logout` (before Logout).

### D3. Two functional scenarios, reading the menu order from the DOM once
To keep the suite fast, add only two scenarios to the existing `Navigation.feature`: the visitor menu
(no login, about 2 s) and the logged-in menu (one login, about 6 s). Each reads the ordered children
of `nav.flex-column` in a single browser call (link text, or a marker for each `<hr>`) and compares
the whole sequence with the expected one, which covers Home first, group order, separators and Logout
last at once. A new `NavBar` method returns that sequence. The scenarios use the default test user
and do not change any data.

## Risks / Trade-offs

- [CSS `:first-of-type` and `:last-of-type` could pick the wrong item] → separators are `<hr>`, not
  `div` (D2); checked manually in the running app for both menu states.
- [Menu order is asserted as a whole sequence, so adding a menu item later breaks the test] →
  intended: the order is a specified behavior, and the failure message shows the full sequence.
- [Two `AuthorizeView` blocks briefly render in the same state during login or logout] → both read
  the same cascaded authentication state, so they switch together.
- [Wiki page `site-navigation` and `docs/TEST_SELECTORS.md` describe the old order] → the docs are
  updated with the change; the wiki is refreshed by wiki-ingest after archive.
