---
title: Credential rules
type: business-rule
summary: The rules for email and password at registration and at login, side by side, how they differ, how emails are matched and passwords are stored, and the gaps that follow (accounts that cannot sign in, trimming, a weak password policy).
tags: [credentials, email, password, policy, validation, registration, login, uniqueness, bcrypt, trimming, case-insensitive, weak-password]
sources:
  - Fiszki.Services/Validation/RegisterUserCommandValidator.cs
  - Fiszki.Services/Validation/LoginCommandValidator.cs
  - Fiszki.Services/Services/UserService.cs
  - Components/Pages/Register.razor
  - Components/Pages/Login.razor
  - Fiszki.Database/Configurations/UserConfiguration.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
  - Fiszki.Tests/Services/UserServiceTests.cs
verified_commit: 6a27f79b9a9bb38c9e68a0f1e35a693394ffb792
confidence: high
---

# Credential rules

## Rule

The rules for the email and the password are not the same when an account is created and when a
user signs in.

| | Registration | Login |
|---|---|---|
| Email | required (not blank), at most 255 characters, **no format check** | required, at most 255 characters, **must be a valid email address** |
| Password | required, **3 to 128 characters, at least one digit** | required, at most 128 characters, no other rule |
| Spaces at the ends | email **and password** are trimmed by the page | **nothing is trimmed** |
| Uniqueness | one account per email, ignoring letter case | email matched ignoring letter case |
| Message on failure | list of failed rules; or "User with email ... already exists" | "Invalid credentials" for a wrong email or password or an inactive account; "Unexpected error. Please try again." for a bad email format |

Other rules:

- **Password storage.** Only a BCrypt hash (work factor 12) is stored; the plain password is never
  stored or returned. (`Fiszki.Services/Services/UserService.cs` `RegisterAsync`)
- **Email storage.** The email is kept with the letter case the user typed. The comparison for
  duplicates and for login ignores case, in the code and in the database column, see [[user-entity]].
- **Login does not say what was wrong.** An unknown email and a wrong password give the same message.

## Where it applies

- [[user-registration]]: the registration column.
- [[user-login-and-logout]]: the login column.

## Evidence

- Registration rules: `Fiszki.Services/Validation/RegisterUserCommandValidator.cs`
  `RegisterUserCommandValidator`. Tested: valid, empty or blank email, passwords of two characters,
  a password without a digit
  (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `RegisterUserCommandValidatorTests`).
  **Not tested:** the 255 and 128 maxima.
- Login rules: `Fiszki.Services/Validation/LoginCommandValidator.cs` `LoginCommandValidator`.
  Tested: valid, empty email, empty password
  (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `LoginCommandValidatorTests`).
  **Not tested:** the email format and the maxima.
- Trimming at registration only: `Components/Pages/Register.razor` `HandleRegister` trims; the login
  call in `Components/Pages/Login.razor` `HandleLogin` passes the values as typed.
- Case-insensitive matching: `Fiszki.Services/Services/UserService.cs` `LoginAsync` and
  `RegisterAsync` compare lower-cased emails. The duplicate test uses the exact same address, so
  letter case itself is not tested
  (`Fiszki.Tests/Services/UserServiceTests.cs` `RegisterAsync_WithExistingEmail_ShouldThrowConflictException`).

## Edge cases

- **An account that cannot sign in.** Registration accepts any non-blank text as an email, but
  login requires a valid email format. A user who registers with a value that is not an email
  address is created and logged in once, but can never sign in again, and gets "Unexpected error.
  Please try again.". (Inferred from the two validators; not tested.)
- **A password with spaces at the ends.** The registration page trims the password, the login page
  does not, so a password typed with a leading or trailing space at registration will not match
  when typed the same way at login. (Inferred from the two pages; not tested.)
- **A weak password is accepted.** Three characters with one digit, such as "a12", pass.
- **The password confirmation is not enforced.** Only a message is shown when it differs; it is
  never checked on submit, see [[user-registration]].
- **Existing emails are revealed.** The duplicate-email message names the address, so anyone can
  test whether an address has an account.

## Discrepancies

The historical API plan (api-plan.md in the old `.ai` folder) states a stricter and uniform policy:
a valid email format (RFC 5322) at registration, a password of at least 8 characters with an
upper-case letter, a lower-case letter and a digit, and Argon2id hashing. The code is more relaxed
(3 characters and a digit, no email format at registration) and uses BCrypt. The code comment in the
registration validator calls its rules "relaxed". The code wins.

## Related

- [[user-registration]]
- [[user-login-and-logout]]
- [[user-entity]]
