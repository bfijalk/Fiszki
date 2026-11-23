---
title: Site navigation and layout
type: feature
summary: How users move around the application: the side menu for logged-out and logged-in users, the list of open and protected pages, the common layout with its leftover "About" link, what happens on errors and unknown addresses, and how little of this is tested.
tags: [navigation, menu, navbar, sidebar, routes, layout, pages, public, protected, login-link, register, error-page, not-found, about-link, home]
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
verified_commit: 5367383718e4da80f89f35abf89c2b78f6f682c8
confidence: high
---

# Site navigation and layout

## Summary

Every page shares one layout: a menu on the side and the page content next to it. The menu shows
the same open links to everyone and changes only for the login: **Login** when logged out;
**Flashcards**, **Generate** and **Logout** when logged in. Two pages need a login; the rest are
open. The content of the information pages is in [[information-pages]]; the login mechanics are in
[[user-login-and-logout]] and [[authentication-session]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

### Pages and the menu

1. **Pages and who can open them.** Open to everyone: Home (`/`), How to Start (`/how-to-start`),
   Examples (`/examples`), About Technicalities (`/about-technicalities`), About Me (`/about`),
   Login (`/login`), Logout (`/logout`), Register (`/register`) and the error page (`/Error`).
   Login required: Flashcards (`/flashcards`) and Generate (`/generate`). Backed by: code only
   (`@page` and `Authorize` in `Components/Pages/`; see [[user-login-and-logout]]).
2. **The menu always shows** the brand "Fiszki" (a link to the home page), Home, How to Start,
   Examples, About Technicalities and About Me. Backed by: code only
   (`Components/Layout/NavMenu.razor` `NavLink`).
3. **A logged-out visitor also sees "Login".** Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/Navigation.feature`, scenario "Nav shows Login link when not authenticated").
4. **A logged-in user also sees Flashcards, Generate and Logout** and no longer sees Login. Backed
   by: code only (`Components/Layout/NavMenu.razor` `AuthorizeView`).
5. **There is no Register link in the menu.** An account is created from the Register page, which
   the user reaches with the "Create Account" button on the Login page. The buttons on the home page
   and on the Examples page lead to the Login page, not to Register. Backed by: code only
   (`Components/Layout/NavMenu.razor`, `Components/Pages/Home.razor` `NavigateToLogin`).
6. **The home page does not change after logging in.** Its buttons ("Get Started - Sign In" and
   "Start Learning Today") always open the Login page, even for a logged-in user. Backed by: code
   only (`Components/Pages/Home.razor` `NavigateToLogin`).
7. **The menu label for the author's page is "About Me"** and the page is `/about`. The label
   "About Technicalities" opens `/about-technicalities`. Backed by: code only.

### Layout, errors and unknown addresses

8. **A leftover "About" link sits in the top bar of every page.** It opens Microsoft's ASP.NET Core
   documentation in a new tab. It is not the About Me page. Backed by: code only
   (`Components/Layout/MainLayout.razor`).
9. **An unknown address shows "Sorry, there's nothing at this address."** inside the normal layout.
   Backed by: code only (`Components/Routes.razor` `NotFound`).
10. **A protected page opened without a login** shows the framework's default message, not the Login
    page. Backed by: code only; see [[authentication-session]].
11. **An unhandled error in the browser session shows a light-yellow bar at the bottom** "An unhandled
    error has occurred." with Reload and dismiss. The bar is hidden until the framework shows it.
    Backed by: code only (`Components/Layout/MainLayout.razor`, `Components/Layout/MainLayout.razor.css`).
12. **An unhandled server error opens the `/Error` page** (outside the Development environment):
    "Error. An error occurred while processing your request.", with a request id when there is one.
    The page then **also shows developer text** about switching to the Development environment and
    a warning not to enable it in production; that template text is visible to end users. Backed by:
    code only (`Components/Pages/Error.razor`, `Program.cs` `UseExceptionHandler`).

## User flow

1. A visitor opens the application and sees Home with the menu.
2. The visitor can read the open pages. To use flashcards the visitor opens Login (and from there
   Create Account).
3. After login the menu gains Flashcards and Generate; Logout returns to the home page.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Layout:** `Components/Layout/MainLayout.razor` holds the side `NavMenu`, a top bar with the
  leftover link, the page body and the framework's error banner.
- **Menu:** `Components/Layout/NavMenu.razor` is plain Bootstrap links with Bootstrap Icons. The open
  links are always rendered; `AuthorizeView` switches between the logged-in and logged-out groups.
  `Logout` clears the login and goes to the home page.
- **Routing:** `Components/Routes.razor` wraps the router in the authentication state, uses the
  authorization-aware route view and defines the unknown-address message. There is no custom
  "not authorized" content and no redirect.
- **Shell:** `Components/App.razor` loads the stylesheets (MudBlazor, Bootstrap, Bootstrap Icons, the
  application's own) and the MudBlazor dialog and snackbar hosts.
- **Errors:** `Program.cs` uses the exception handler with the `/Error` page and HSTS when the
  environment is not Development.
- **Test selectors:** `navbar-brand`, `nav-home`, `nav-how-to-start`, `nav-examples`,
  `nav-about-technicalities`, `nav-about`, `nav-flashcards`, `nav-generate`, `nav-logout`,
  `nav-login`.

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/Navigation.feature` has **one** scenario: on the
  home page a "Login" link is visible. Steps are in `Fiszki.FunctionalTests/Steps/NavigationSteps.cs`.
- **Used indirectly:** other scenarios click menu items (for example "Generate") through generic
  steps in the same file.
- **Not tested:** the links of the logged-in menu, the hidden Register link, the leftover top link,
  the unknown-address message, the error banner and the `/Error` page.

## Dependencies and side effects

- Depends on the login state, see [[authentication-session]].
- Leads to every feature page: [[flashcard-list-and-filters]], [[ai-flashcard-generation]],
  [[user-registration]], [[user-login-and-logout]] and the [[information-pages]].

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
