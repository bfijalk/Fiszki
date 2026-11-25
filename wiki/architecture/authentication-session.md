---
title: Authentication state and sessions
type: architecture
summary: How the application remembers who is logged in: an in-memory object per browser connection with claims, unused cookie configuration, how pages are protected, where roles are (not) used, and why a reload logs the user out.
tags: [authentication, session, state, claims, circuit, blazor, cookie, authorize, roles, custom-auth-state-provider, protected-pages, logout]
sources:
  - Fiszki.Services/Services/CustomAuthStateProvider.cs
  - Program.cs
  - Components/Routes.razor
  - Components/Layout/NavMenu.razor
  - Components/Pages/Login.razor
  - Components/Pages/Register.razor
  - Components/Pages/Logout.razor
  - Components/Pages/Flashcards.razor
  - Components/Pages/GeneratePage.razor
  - Components/Pages/Leaderboard.razor
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: high
---

# Authentication state and sessions

## Responsibility

Decide who the current user is, make that visible to pages and services, and guard the pages that
need a login. There are no tokens and no authentication cookies: the "session" is an object in the
server's memory, one per browser connection. The user-facing flows are in
[[user-registration]] and [[user-login-and-logout]].

## Structure

```
 Login / Register page
        |  on success: MarkUserAsAuthenticated(user)
        v
 CustomAuthStateProvider (scoped = one per connection)
   holds one ClaimsPrincipal in a field
        |  GetAuthenticationStateAsync
        v
 Routes (AuthorizeRouteView) + NavMenu (AuthorizeView) + pages reading the user id claim
        ^
        |  on logout: MarkUserAsLoggedOut()   (menu button or /logout page)
```

- **Provider:** `Fiszki.Services/Services/CustomAuthStateProvider.cs` `CustomAuthStateProvider`
  keeps the current user in a field, starting as an empty (anonymous) identity.
  `MarkUserAsAuthenticated` builds an identity with three claims, user id (`NameIdentifier`), email
  (`Name`) and role (`Role`), and notifies the framework; `MarkUserAsLoggedOut` resets it.
- **Registration in the container:** `Program.cs` registers the provider as **scoped**, together
  with the authorization services and the cascading authentication state.
- **Callers:** the Login and Register pages mark the user as logged in; the menu button and the
  Logout page mark the user as logged out.

## How it behaves

- **Lifetime.** A scoped service in an interactive Blazor Server application lives as long as the
  user's connection (circuit). A page reload or a new browser tab creates a new connection and a new
  provider, so the user is logged out. There is no "remember me" and nothing is stored in the browser.
  How long a dropped connection is kept is framework behavior and was not checked.
- **Protected pages.** A page is guarded with the `Authorize` attribute. Only
  `Components/Pages/Flashcards.razor`, `Components/Pages/GeneratePage.razor` and
  `Components/Pages/Leaderboard.razor` have it.
- **Not logged in.** The routing component uses the framework's authorization-aware route view and
  defines no custom "not authorized" content and no redirect to the login page, so the framework's
  default message is shown. (`Components/Routes.razor` `AuthorizeRouteView`; the default text was not
  checked by running the application.)
- **Menu.** `Components/Layout/NavMenu.razor` shows Login when logged out and Flashcards, Generate,
  Leaderboard and Logout when logged in; the order is in [[site-navigation]].
- **Who is the user for the services.** Pages read the user id from the `NameIdentifier` claim and
  pass it to the services, see [[flashcard-ownership]]. If the claim is missing, pages show
  "Unable to determine current user." (the Flashcards page) or ask the user to log in again (the
  Generate page).
- **Roles do nothing.** The role is a claim, but no code checks it: there is no role-based
  authorization, so an `Admin` and a `Basic` user can do the same. See [[user-entity]].

## Configuration and wiring

- **Cookie authentication is configured but never used.** `Program.cs` adds cookie authentication
  with login path `/login`, logout path `/logout` and a 7-day expiry, and the pipeline calls the
  authentication and authorization middleware, but no code signs a user in or out through it. A
  comment in the code says the cookie setup is left "for future" and is not used by the custom
  provider. Treat the 7-day expiry as inactive.
- **Rendering.** The routes and the Register page use interactive server rendering, which is what
  makes the per-connection state work.

## Behavior worth knowing (code reading, no tests)

- **No test covers the provider**, the protection of pages or the session ending on reload.
- **No list of sessions.** The logged-in users exist only inside their own connections' objects, so
  nothing can list or end another user's login from the server side.

## Discrepancies

The historical API plan (api-plan.md in the old `.ai` folder) describes bearer tokens (JWT) with an
expiry, sent with every request, and endpoints for registering, logging in and deleting the account.
The code has none of them. The code wins.

## Related

- [[user-registration]]
- [[user-login-and-logout]]
- [[user-entity]]
- [[flashcard-ownership]]
- [[application-layers]]
