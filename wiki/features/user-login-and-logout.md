---
title: User login and logout
type: feature
summary: How a user signs in on the Login page and signs out, what the screens and messages do, which pages need a login, how long the login lasts, and what the code does not have (reset, lockout, remember me).
tags: [login, logout, sign-in, sign-out, authentication, credentials, invalid-credentials, inactive, navigation-menu, protected-pages, not-authorized, session]
sources:
  - Fiszki.FunctionalTests/Features/Login.feature
  - Components/Pages/Login.razor
  - Components/Pages/Logout.razor
  - Components/Layout/NavMenu.razor
  - Components/Routes.razor
  - Components/Pages/Flashcards.razor
  - Components/Pages/GeneratePage.razor
  - Fiszki.Services/Interfaces/IUserService.cs
  - Fiszki.Services/Services/UserService.cs
  - Fiszki.Services/Commands/LoginCommand.cs
  - Fiszki.Services/Validation/LoginCommandValidator.cs
  - Fiszki.Services/Exceptions/UnauthorizedDomainException.cs
  - Fiszki.Tests/Services/UserServiceTests.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
  - Fiszki.FunctionalTests/Steps/LoginSteps.cs
  - Fiszki.FunctionalTests/Steps/DynamicLoginSteps.cs
verified_commit: 6a27f79b9a9bb38c9e68a0f1e35a693394ffb792
confidence: high
---

# User login and logout

## Summary

A visitor signs in on the Login page (`/login`) with an email and a password and lands on the
Flashcards page. The user signs out with the Logout button in the menu or by opening `/logout`.
The login lasts only as long as the browser connection to the application. The rules for email and
password are in [[credential-rules]]; how the logged-in state is kept and how pages are protected is
in [[authentication-session]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

### Login

1. **The Login button needs both fields.** It is disabled while the email or the password is empty
   or only spaces, and while a sign-in is running (the label then reads "Signing in..."). Backed by:
   Gherkin
   (`Fiszki.FunctionalTests/Features/Login.feature`, scenario "Login button disabled when fields empty" and
   `Fiszki.FunctionalTests/Features/Login.feature`, scenario "Login button enabled when both fields filled").
2. **Wrong email and wrong password look the same.** Both give the message "Invalid credentials", so
   the screen does not reveal which one was wrong. The email is matched ignoring letter case. Backed
   by: unit test
   (`Fiszki.Tests/Services/UserServiceTests.cs` `LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedException`,
   `Fiszki.Tests/Services/UserServiceTests.cs` `LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException`);
   the letter-case matching is from code
   (`Fiszki.Services/Services/UserService.cs` `LoginAsync`).
3. **An inactive account cannot sign in**, even with the correct password, and the user sees
   "Invalid credentials". The service's own message "Account inactive" is never shown. The check
   happens after the password is verified. Nothing in the application deactivates an account. Backed
   by: unit test for refusing an inactive user
   (`Fiszki.Tests/Services/UserServiceTests.cs` `LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException`);
   the displayed text is from code (`Components/Pages/Login.razor` `HandleLogin`).
4. **Success signs the user in and opens the Flashcards page.** The user is marked as logged in for
   this connection and sent to `/flashcards`. Backed by: Gherkin, indirectly: almost every other
   scenario starts by logging in with a test user
   (`Fiszki.FunctionalTests/Steps/DynamicLoginSteps.cs` `WhenILoginWithMyTestUser`); unit test for the
   service result
   (`Fiszki.Tests/Services/UserServiceTests.cs` `LoginAsync_WithValidCredentials_ShouldReturnUserDto`).
5. **A text that is not an email address gets a vague error.** The service requires an email format
   when logging in, but the page treats that failure as unexpected and shows "Unexpected error. Please
   try again.", not "Invalid credentials". Because registering does not check the email format, an
   account can exist that can never sign in, see [[credential-rules]]. Backed by: code only
   (`Components/Pages/Login.razor` `HandleLogin`).
6. **Other errors show "Unexpected error. Please try again."** and the technical error is written to
   the server console. Backed by: code only (`Components/Pages/Login.razor` `HandleLogin`).

### Logout

7. **Logout clears the login and goes to the home page.** The menu button (shown only when logged in)
   and the `/logout` page do the same thing, with no confirmation. Backed by: code only
   (`Components/Layout/NavMenu.razor` `Logout`, `Components/Pages/Logout.razor` `OnInitialized`). No
   scenario covers logout.

### Access and the menu

8. **Two pages need a login:** Flashcards (`/flashcards`) and Generate (`/generate`). The other pages
   (home, how to start, examples, about, about technicalities, login, logout, register) are open.
   Backed by: code only (`Components/Pages/Flashcards.razor` `Authorize`,
   `Components/Pages/GeneratePage.razor` `Authorize`).
9. **Opening a protected page without a login shows the framework's default "not authorized"
   message**, not the Login page. The routing component defines no custom message and no redirect to
   login. The exact default text comes from the framework and was not checked by running the
   application. Backed by: code only (`Components/Routes.razor` `AuthorizeRouteView`).
10. **The menu changes with the login.** Logged out, it shows "Login". Logged in, it shows Flashcards,
    Generate and Logout. Home, How to start, Examples, About technicalities and About are always
    shown. Backed by: code only (`Components/Layout/NavMenu.razor` `AuthorizeView`).
11. **The login lasts only as long as the connection (circuit).** Reloading the page or opening a new
    tab starts a new connection and signs the user out, and so does a connection that is lost for good.
    There is no "remember me" and no cookie login. See [[authentication-session]]. Backed by: code only
    (the state lives in a per-connection object); how long a dropped connection is kept is framework
    behavior and was not checked.
12. **There is no password reset, no email confirmation, no limit on failed attempts and no account
    lock.** A search of the code finds none. Backed by: code only (absence).

## User flow

1. The user opens `/login` (or is told "Not authorized" on a protected page and opens it by hand).
2. The user types email and password; the Login button becomes enabled.
3. The user clicks **Login**. On success the Flashcards page opens; on failure a red message shows.
4. To leave, the user clicks **Logout**; the home page opens.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Login page:** `Components/Pages/Login.razor` (MudBlazor form). `HandleLogin` builds a
  `LoginCommand` from the raw email and password (no trimming), calls `IUserService.LoginAsync`, and
  on success calls `CustomAuthStateProvider.MarkUserAsAuthenticated` and navigates to `/flashcards`.
  It catches `UnauthorizedDomainException` (message "Invalid credentials") separately from every
  other exception (message "Unexpected error. Please try again.").
- **Service:** `Fiszki.Services/Services/UserService.cs` `LoginAsync` validates the command, finds
  the user by lower-cased email, verifies the BCrypt hash, then checks the active flag, and returns a
  DTO. Unknown user and wrong password both throw `UnauthorizedDomainException` with the message
  "Invalid credentials".
- **Validator:** `Fiszki.Services/Validation/LoginCommandValidator.cs` `LoginCommandValidator`
  (email required, valid format, at most 255 characters; password required, at most 128).
- **Logout:** `Components/Layout/NavMenu.razor` `Logout` and `Components/Pages/Logout.razor`
  `OnInitialized` call `CustomAuthStateProvider.MarkUserAsLoggedOut` and navigate to `/`.
- **Test selectors:** `email-input`, `password-input`, `login-button`, `create-account-button`,
  `login-error-alert`, `nav-login`, `nav-logout`.

### Test coverage

- **Gherkin:** `Fiszki.FunctionalTests/Features/Login.feature` checks only that the Login button is
  disabled with empty fields and enabled with both filled. Signing in itself is exercised as a setup
  step of other scenarios, which log in with a dynamically created test user.
- **A misleading step name:** the step "I should be redirected to the Flashcard Generation page"
  (`Fiszki.FunctionalTests/Steps/LoginSteps.cs` `ThenIShouldBeRedirectedToFlashcardGenerationPage`)
  passes when the address is any of `/flashcards`, `/generate`, `/home` or `/dashboard`. After a real
  login the application opens `/flashcards`.
- **Unit tests** (`Fiszki.Tests/Services/UserServiceTests.cs`): valid login, unknown email, wrong
  password, inactive user; validator tests for a valid login, an empty email and an empty password.
- **Not tested:** the error texts shown by the page, the email-format failure, logout, the menu
  changes, the "Not authorized" behavior, the session ending on reload, and the authentication state
  provider.

## Dependencies and side effects

- Needs an account made by [[user-registration]].
- Signing in lets the user reach [[ai-flashcard-generation]], [[manual-flashcard-creation]] and
  [[flashcard-deletion]], which read the user id from the login, see [[authentication-session]].

## Discrepancies

Older notes disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Tokens and endpoints.** The historical API plan (api-plan.md in the old `.ai` folder) describes
  `POST /users/login` returning an access token that expires after an hour, sent as a bearer token
  on every request. The code has no tokens and no HTTP endpoint: the page calls the service directly
  and the login is kept in memory for the connection.
- **Credential rules.** See [[credential-rules]] for the different password and email policy.

## Related

- [[user-registration]]
- [[credential-rules]]
- [[user-entity]]
- [[authentication-session]]
- [[application-layers]]
- [[glossary]]
