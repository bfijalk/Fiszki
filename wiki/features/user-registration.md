---
title: User registration
type: feature
summary: How a visitor creates an account on the Register page: the fields, the password mismatch message that does not block registration, the rules for email and password, duplicate emails, automatic login and what is stored.
tags: [registration, register, sign-up, account, email, password, confirm-password, mismatch, duplicate, bcrypt, auto-login, create-account]
sources:
  - Fiszki.FunctionalTests/Features/Register.feature
  - Components/Pages/Register.razor
  - Components/Pages/Login.razor
  - Fiszki.Services/Interfaces/IUserService.cs
  - Fiszki.Services/Services/UserService.cs
  - Fiszki.Services/Commands/RegisterUserCommand.cs
  - Fiszki.Services/Validation/RegisterUserCommandValidator.cs
  - Fiszki.Tests/Services/UserServiceTests.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
  - Fiszki.FunctionalTests/Steps/RegisterSteps.cs
verified_commit: 6a27f79b9a9bb38c9e68a0f1e35a693394ffb792
confidence: high
---

# User registration

## Summary

A visitor creates an account on the Register page (`/register`) with an email, a password and a
password confirmation. When registration succeeds the user is **logged in automatically** and sent
to the home page. The rules for email and password are collected in [[credential-rules]]; what is
stored is in [[user-entity]]; how the logged-in state is kept is in [[authentication-session]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

1. **Anyone can register.** The page needs no login. It is reached from the "Create Account"
   button on the Login page; the main menu has no Register link. Backed by: code only
   (`Components/Pages/Login.razor` `create-account-button` points to `/register`; the page has no
   `Authorize` attribute).
2. **The form has three fields:** Email, Password and Confirm Password. Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/Register.feature`, scenario "Password mismatch shows message").
3. **A mismatch message appears while typing.** When both password fields are filled and differ, the
   text "Passwords don't match." is shown under Confirm Password. Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/Register.feature`, scenario "Password mismatch shows message").
4. **The mismatch does not stop registration.** The Register button is disabled only while a
   request is in progress. The confirmation field is never sent to the service, so an account is
   created with the first password even when the confirmation differs. Backed by: code only
   (`Components/Pages/Register.razor` `HandleRegister`). The Gherkin scenario only checks that the
   message appears, not that registering is blocked.
5. **Email and password are trimmed** of leading and trailing spaces before they are sent. Backed
   by: code only (`Components/Pages/Register.razor` `HandleRegister`). Login does not trim the
   password, see [[credential-rules]].
6. **Rules the service enforces.** The email must not be empty and at most 255 characters; there
   is **no check that it looks like an email address**. The password must be 3 to 128 characters and
   contain at least one digit. Backed by: unit test for a valid account, an empty or blank email, a
   password shorter than 3 characters and a password without a digit
   (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `RegisterUserCommandValidatorTests`); the
   maximum lengths come from code only
   (`Fiszki.Services/Validation/RegisterUserCommandValidator.cs` `RegisterUserCommandValidator`).
7. **An email can be registered only once, ignoring letter case.** A second registration with the
   same address fails with "User with email {address} already exists", which the page shows as it is;
   this tells anyone that the address already has an account. Backed by: unit test for the exact
   same address (`Fiszki.Tests/Services/UserServiceTests.cs` `RegisterAsync_WithExistingEmail_ShouldThrowConflictException`);
   the case-insensitive comparison comes from code
   (`Fiszki.Services/Services/UserService.cs` `RegisterAsync`) and from the database column, see
   [[user-entity]].
8. **What the new account looks like.** Role Basic, active, both card counters 0, the email kept
   with the letter case the user typed, and the password stored only as a hash (never the plain
   password, and the hash is not returned to the page). Backed by: unit test
   (`Fiszki.Tests/Services/UserServiceTests.cs` `RegisterAsync_WithValidCommand_ShouldCreateUserSuccessfully`).
   The hash strength (BCrypt, work factor 12) is code only
   (`Fiszki.Services/Services/UserService.cs` `RegisterAsync`).
9. **After success the user is logged in and sent to the home page.** There is no separate login
   step and no redirect to the flashcards. The page sets "Account created" and navigates to `/`.
   Backed by: code only (`Components/Pages/Register.razor` `HandleRegister`).
10. **Errors are shown in a red box.** Failed rules are listed, separated by "; "; a duplicate email
    shows its message; anything else shows "Unexpected error: " and the technical message. Backed by:
    code only (`Components/Pages/Register.razor` `HandleRegister`).
11. **There is no email confirmation, no CAPTCHA and no terms step.** A search of the code finds
    none. Backed by: code only (absence).

## User flow

1. On the Login page the user clicks **Create Account**, or opens `/register`.
2. The user types an email, a password and the confirmation.
3. The user clicks **Register**.
4. On success the user is signed in and lands on the home page; on failure a red message appears.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Page:** `Components/Pages/Register.razor` is plain Bootstrap markup rendered interactively. The
  fields are plain state variables updated on every keystroke; `HandleRegister` trims email and
  password, calls `IUserService.RegisterAsync`, and on success calls
  `CustomAuthStateProvider.MarkUserAsAuthenticated` and navigates to `/`. The page writes the typed
  email and the password length to the server console while typing.
- **Service:** `Fiszki.Services/Services/UserService.cs` `RegisterAsync` validates the command, looks
  for an existing user by lower-cased email, throws `ConflictException` if found, hashes the password
  with BCrypt (work factor 12), creates the user and returns a DTO without the hash.
- **Validator:** `Fiszki.Services/Validation/RegisterUserCommandValidator.cs`
  `RegisterUserCommandValidator`; the code comment calls the rules "relaxed".
- **Test selectors:** `email-input`, `password-input`, `password-confirm-input`,
  `password-mismatch-message`, `register-button`, `back-to-login-button`, `register-error-alert`,
  `register-success-alert`.

### Test coverage

- **Gherkin:** `Fiszki.FunctionalTests/Features/Register.feature` has one scenario: type an email, a
  password and a different confirmation, and see the mismatch message. Steps are in
  `Fiszki.FunctionalTests/Steps/RegisterSteps.cs`.
- **Unit tests:** valid registration, existing email, invalid command, in
  `Fiszki.Tests/Services/UserServiceTests.cs` (the validator is mocked there), and the validator
  rules listed in rule 6.
- **Not tested:** a successful registration through the screen, that the mismatch does not block
  registration, the automatic login and redirect, trimming, the maximum lengths, letter case in the
  duplicate check, and the email format (which is not checked at all).

## Dependencies and side effects

- Creates the account that [[user-login-and-logout]] later signs in.
- Marks the user as logged in for the current connection, see [[authentication-session]].

## Discrepancies

Older notes disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Password policy.** The historical API plan (api-plan.md in the old `.ai` folder) requires at
  least 8 characters with an upper-case letter, a lower-case letter and a digit. The code requires 3
  characters and a digit.
- **Email format.** The API plan requires a valid email format (RFC 5322). The registration code
  only requires a non-empty value.
- **Hashing.** The API plan says Argon2id. The code uses BCrypt.
- **Result of registering.** The API plan says registration returns an access token. The code has no
  tokens and no HTTP endpoint: the page calls the service directly and keeps the user logged in
  in memory, see [[authentication-session]].

## Related

- [[user-login-and-logout]]
- [[credential-rules]]
- [[user-entity]]
- [[authentication-session]]
- [[application-layers]]
- [[glossary]]
