---
title: Site navigation and layout
type: feature
summary: How users move around the application: the side menu for logged-out and logged-in users and its order and separators, the list of open and protected pages, the common layout with its leftover "About" link, what happens on errors and unknown addresses, and how it is tested.
tags: [navigation, menu, navbar, sidebar, routes, layout, pages, public, protected, login-link, register, error-page, not-found, about-link, home, leaderboard, order, separator, menu-order]
sources:
  - Fiszki.FunctionalTests/Features/Navigation.feature
  - Components/Layout/NavMenu.razor
  - Components/Layout/MainLayout.razor
  - Components/Layout/MainLayout.razor.css
  - Components/Routes.razor
  - Components/App.razor
  - Components/Pages/Home.razor
  - Components/Pages/Error.razor
  - Program.cs
  - Fiszki.FunctionalTests/Steps/NavigationSteps.cs
  - openspec/specs/site-navigation/spec.md
  - Components/Layout/NavMenu.razor.css
  - Fiszki.FunctionalTests/PageObjects/NavBar.cs
  - docs/TEST_SELECTORS.md
verified_commit: cdf3374897d4b0f5eab4affaaba6a6874064698e
confidence: high
---

# Site navigation and layout

## Summary

Every page shares one layout: a menu on the side and the page content next to it. The menu shows
Home as the first item for everyone. Logged-in users then see **Flashcards**, **Generate** and
**Leaderboard** above the open links, separated from them by a line, and **Logout** at the very bottom;
logged-out visitors see the open links and then **Login**. Three pages need a login; the rest are open. The content of the information pages is in [[information-pages]]; the login mechanics are in
[[user-login-and-logout]] and [[authentication-session]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

### Pages and the menu

1. **Pages and who can open them.** Open to everyone: Home (`/`), How to Start (`/how-to-start`),
   Examples (`/examples`), About Technicalities (`/about-technicalities`), About Me (`/about`),
   Login (`/login`), Logout (`/logout`), Register (`/register`) and the error page (`/Error`).
   Login required: Flashcards (`/flashcards`), Generate (`/generate`) and Leaderboard (`/leaderboard`).
   Backed by: Gherkin (`Fiszki.FunctionalTests/Features/Leaderboard.feature`, scenario "Leaderboard page requires login") for the leaderboard and code only (`@page` and `Authorize` in `Components/Pages/`; see
   [[user-login-and-logout]]) for the rest. The leaderboard itself is in [[leaderboard]].
2. **The menu always shows** the brand "Fiszki" (a link to the home page), Home, How to Start,
   Examples, About Technicalities and About Me; Home is always the first item. Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/Navigation.feature`, scenarios "Menu order for a visitor" and "Menu order for a logged-in user") and OpenSpec (`openspec/specs/site-navigation/spec.md`, requirement "Home is the first menu item").
3. **A logged-out visitor also sees "Login"**, as the last item. Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/Navigation.feature`, scenarios "Nav shows Login link when not authenticated"
   and "Menu order for a visitor").
4. **A logged-in user also sees Flashcards, Generate, Leaderboard and Logout** and no longer sees Login.
   Backed by: Gherkin (`Fiszki.FunctionalTests/Features/Navigation.feature`, scenario "Menu order for a logged-in user").
5. **There is no Register link in the menu.** An account is created from the Register page, which
   the user reaches with the "Create Account" button on the Login page. The buttons on the home page
   and on the Examples page lead to the Login page, not to Register. Backed by: code only
   (`Components/Layout/NavMenu.razor`, `Components/Pages/Home.razor` `NavigateToLogin`).
6. **The home page does not change after logging in.** Its buttons ("Get Started - Sign In" and
   "Start Learning Today") always open the Login page, even for a logged-in user. Backed by: code
   only (`Components/Pages/Home.razor` `NavigateToLogin`).
7. **The menu label for the author's page is "About Me"** and the page is `/about`. The label
   "About Technicalities" opens `/about-technicalities`. Backed by: code only.

### Order and groups

8. **For a logged-in user the order is** Home, Flashcards, Generate, Leaderboard, separator, How to Start,
   Examples, About Technicalities, About Me, separator, Logout. The items available only after login sit
   above the open ones and Logout is last. Backed by: Gherkin (`Fiszki.FunctionalTests/Features/Navigation.feature`,
   scenario "Menu order for a logged-in user") and OpenSpec (`openspec/specs/site-navigation/spec.md`,
   requirements "Logged-in items above public items" and "Logout is the last menu item").
9. **The two separators are thin horizontal lines shown only to logged-in users:** one between the logged-in
   items and the open ones, one before Logout. Backed by: Gherkin (same scenario) and OpenSpec (requirement
   "Separators between menu groups").
10. **The visitor menu is Home, How to Start, Examples, About Technicalities, About Me, Login**, with no
    separators, and the menu returns to this after Logout. Backed by: Gherkin (scenario "Menu order for a visitor") and OpenSpec (requirement "Visitor menu order is unchanged"); the return after Logout is code
    only (`Components/Layout/NavMenu.razor` `Logout`).

### Layout, errors and unknown addresses

11. **A leftover "About" link sits in the top bar of every page.** It opens Microsoft's ASP.NET Core
   documentation in a new tab. It is not the About Me page. Backed by: code only
   (`Components/Layout/MainLayout.razor`).
12. **An unknown address shows "Sorry, there's nothing at this address."** inside the normal layout.
   Backed by: code only (`Components/Routes.razor` `NotFound`).
13. **A protected page opened without a login** shows the framework's default message, not the Login
    page. Backed by: code only; see [[authentication-session]].
14. **An unhandled error in the browser session shows a light-yellow bar at the bottom** "An unhandled
    error has occurred." with Reload and dismiss. The bar is hidden until the framework shows it.
    Backed by: code only (`Components/Layout/MainLayout.razor`, `Components/Layout/MainLayout.razor.css`).
15. **An unhandled server error opens the `/Error` page** (outside the Development environment):
    "Error. An error occurred while processing your request.", with a request id when there is one.
    The page then **also shows developer text** about switching to the Development environment and
    a warning not to enable it in production; that template text is visible to end users. Backed by:
    code only (`Components/Pages/Error.razor`, `Program.cs` `UseExceptionHandler`).

## User flow

1. A visitor opens the application and sees Home with the menu.
2. The visitor can read the open pages. To use flashcards the visitor opens Login (and from there
   Create Account).
3. After login the menu gains Flashcards, Generate and Leaderboard at the top and Logout at the bottom; Logout
   returns to the home page.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Layout:** `Components/Layout/MainLayout.razor` holds the side `NavMenu`, a top bar with the
  leftover link, the page body and the framework's error banner.
- **Menu:** `Components/Layout/NavMenu.razor` is plain Bootstrap links with Bootstrap Icons. The open
  links are always rendered; Home is rendered first, then an `AuthorizeView` whose logged-in
  part holds Flashcards, Generate, Leaderboard and the first separator, then the open links, then a second
  `AuthorizeView` that shows the second separator and Logout when logged in and Login when logged out. The
  separators are `<hr>` elements, not `div`s, so the `:first-of-type` and `:last-of-type` padding rules in
  `Components/Layout/NavMenu.razor.css` still match Home and the last item. `Logout` clears the login and goes
  to the home page.
- **Routing:** `Components/Routes.razor` wraps the router in the authentication state, uses the
  authorization-aware route view and defines the unknown-address message. There is no custom
  "not authorized" content and no redirect.
- **Shell:** `Components/App.razor` loads the stylesheets (MudBlazor, Bootstrap, Bootstrap Icons, the
  application's own) and the MudBlazor dialog and snackbar hosts.
- **Errors:** `Program.cs` uses the exception handler with the `/Error` page and HSTS when the
  environment is not Development.
- **Test selectors:** `navbar-brand`, `nav-home`, `nav-how-to-start`, `nav-examples`,
  `nav-about-technicalities`, `nav-about`, `nav-flashcards`, `nav-generate`, `nav-leaderboard`,
  `nav-logout`, `nav-login`, `nav-separator-account`, `nav-separator-logout` (listed in `docs/TEST_SELECTORS.md`).

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/Navigation.feature` has **three** scenarios: on the
  home page a "Login" link is visible, and the whole menu order with separators for a visitor and for a
  logged-in user. The last two read the ordered menu entries in one browser call
  (`Fiszki.FunctionalTests/PageObjects/NavBar.cs` `GetMenuEntriesAsync`) and compare the sequence, so adding a
  menu item means updating the expected sequence. Steps are in `Fiszki.FunctionalTests/Steps/NavigationSteps.cs`.
- **Used indirectly:** other scenarios click menu items (for example "Generate") through generic
  steps in the same file.
- **Not tested:** where the menu links lead (only their order is tested), the hidden Register link, the leftover top link,
  the unknown-address message, the error banner and the `/Error` page.

## Dependencies and side effects

- Depends on the login state, see [[authentication-session]].
- Leads to every feature page: [[flashcard-list-and-filters]], [[ai-flashcard-generation]],
  [[leaderboard]], [[user-registration]], [[user-login-and-logout]] and the [[information-pages]].

## Discrepancies

Older notes and in-app texts disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Redirect to login.** The historical UI plan (ui-plan-basic.md in the old `.ai` folder) says
  unauthenticated users are redirected to login. The code shows a default message instead.
- **Component library.** The same plan says MudBlazor is used for all forms, tables, dialogs and
  navigation components. The menu, the Register page, the Flashcards page and the AI edit dialog use
  plain Bootstrap markup; MudBlazor is used on some pages.
- **Error handling.** The historical UI plans (ui-plan-basic.md and ui-plan-prompted.md in the old
  `.ai` folder) describe a global error boundary and toast notifications. The code has neither (no error boundary
  and no snackbar call anywhere); it uses the framework's error bar and the `/Error` page.
- **"Click Register in the navigation".** The How to Start page tells users to do this; there is no
  such link, see [[information-pages]].

## Related

- [[information-pages]]
- [[user-login-and-logout]]
- [[authentication-session]]
- [[application-layers]]
- [[glossary]]
- [[leaderboard]]
