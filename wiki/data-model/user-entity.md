---
title: User entity
type: data-model
summary: The users table and User entity: fields, the case-insensitive unique email, role and active flag, card counters, relations and delete cascade, and which fields the application actually uses or ever changes.
tags: [user, users, entity, table, database, email, unique, citext, role, admin, active, counters, password-hash, cascade]
sources:
  - Fiszki.Database/Entities/User.cs
  - Fiszki.Database/Configurations/UserConfiguration.cs
  - Fiszki.Database/Migrations/20251014085238_InitialCreate.cs
  - Fiszki.Services/Models/UserDto.cs
  - Fiszki.Services/Services/UserService.cs
  - Fiszki.Services/Services/CustomAuthStateProvider.cs
  - Fiszki.Services/Services/TestDataSeeder.cs
verified_commit: 5367383718e4da80f89f35abf89c2b78f6f682c8
confidence: high
---

# User entity

## Entity

A user is one person's account. It is stored in the `users` table and mapped from
`Fiszki.Database/Entities/User.cs` `User`. It owns that person's flashcards and learning-progress
records.

## Fields

| Field | Type | Constraints | Meaning |
|---|---|---|---|
| `Id` | GUID (`uuid`) | primary key | User identifier; becomes the user id on every flashcard |
| `Email` | `citext` | required, **unique** | Login name; compared ignoring letter case by the database |
| `PasswordHash` | `text` | required | BCrypt hash of the password; never returned by the service |
| `Role` | enum stored as `text` (`Basic`, `Admin`) | required | Account role; default `Basic` |
| `IsActive` | `boolean` | required | Whether the account may sign in; default true |
| `TotalCardsGenerated` | `integer` | required | Intended count of AI cards generated |
| `TotalCardsAccepted` | `integer` | required | Intended count of AI cards accepted |
| `CreatedAt` | `timestamp with time zone` | column `created_at` | Creation time (UTC) |
| `UpdatedAt` | `timestamp with time zone` | column `updated_at` | Last change time (UTC) |

The email column is unique through an index
(`Fiszki.Database/Configurations/UserConfiguration.cs` `Configure`) and its type `citext` is a
PostgreSQL case-insensitive text type, so "A@x.com" and "a@x.com" are the same address in the
database. The service passes the data to pages as a DTO without the hash
(`Fiszki.Services/Models/UserDto.cs` `UserDto`).

## Relations

- **Flashcards:** one user, many flashcards. Deleting the user deletes their flashcards, see
  [[flashcard-entity]].
- **Learning progress:** one user, many records. Deleting the user deletes them, see
  [[learning-progress-entity]].

## Lifecycle

- **Created** by registration, see [[user-registration]]: role `Basic`, active, counters 0, both
  times set to now, email kept as typed.
- **Never changed by the application afterwards.** No code updates a user: not the role, the active
  flag, the password or `UpdatedAt`. There is no profile page, password change or account deletion.
- **`Role` has no effect.** The role is put in the login claims but nothing checks it, so an `Admin`
  can do exactly what a `Basic` user can. Only seeded test data contains an `Admin`. See
  [[authentication-session]].
- **`IsActive` is only read** when signing in, see [[user-login-and-logout]]. Nothing sets it to
  false.
- **The counters are never updated.** Generating or saving flashcards does not change them; they hold
  only the numbers set for seeded test users, see [[ai-flashcard-generation]].
- **Lookup by id exists but the application does not use it.** `UserService.GetByIdAsync` is only
  called from tests.

## Discrepancies

The historical database plan (db-plan.md in the old `.ai` folder) describes the same fields but
differs in form. The code wins.

- **Role type.** The plan uses a PostgreSQL enum with lower-case values `basic` and `admin`; the
  code stores the role as text with the values `Basic` and `Admin`.
- **Column names and defaults.** The plan uses snake_case names and database defaults (generated id,
  current time, default role, active, zero counters); the migration uses names such as `Email` and
  `PasswordHash` (only the two timestamps are snake_case) and sets no database defaults, so the
  application supplies all values.
- **Credentials and counters.** See [[credential-rules]] and [[ai-flashcard-generation]].

## Related

- [[user-registration]]
- [[user-login-and-logout]]
- [[credential-rules]]
- [[flashcard-entity]]
- [[learning-progress-entity]]
- [[authentication-session]]
