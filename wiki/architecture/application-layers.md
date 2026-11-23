---
title: Application layers and request flow
type: architecture
summary: How the Fiszki Blazor Server app is layered (UI, services, database), how the UI reaches services without an HTTP API, how the user identity flows, and which database provider is used.
tags: [architecture, layers, blazor, services, dependency-injection, authentication, claims, database, test-mode, no-api]
sources:
  - Program.cs
  - Fiszki.Services/DependencyInjection.cs
  - Fiszki.Services/Services/CustomAuthStateProvider.cs
  - Components/Pages/Flashcards.razor
verified_commit: 5d1568b818c698f75aa953c7422003d1563bec9d
confidence: high
---

# Application layers and request flow

This page covers only what the flashcard features rely on. It does not describe the whole
system.

## Responsibility

Fiszki is a **Blazor Server** application with interactive server components. Pages call
domain services directly. There is no HTTP controller or minimal-API endpoint in the
application: a search of the code for endpoint mapping finds none, and the pages inject
service interfaces.

## Structure

```
 Components/Pages/*.razor          UI (Razor components), inject service interfaces
        |  direct calls (no HTTP API)
        v
 Fiszki.Services                   services, commands, FluentValidation validators
        |  Entity Framework Core
        v
 Fiszki.Database (FiszkiDbContext) entities, configurations, migrations
        |
        v
 PostgreSQL (Npgsql)   or   in-memory provider in test mode
```

- **UI and host:** `Program.cs` registers Razor components with interactive server render mode.
  Styling is mixed: the Flashcards page uses plain Bootstrap markup, while other pages (for
  example the login, home and generation pages) use MudBlazor components.
- **Services:** `Fiszki.Services/DependencyInjection.cs` `AddFiszkiServices` registers
  validators and services as scoped, including `IFlashcardService` mapped to `FlashcardService`.
  `Program.cs` calls it once at startup.
- **Database:** `FiszkiDbContext` is registered in `Program.cs`. Entities are mapped by
  configuration classes found in the database assembly.

## Interactions

- **Who is the user.** `Fiszki.Services/Services/CustomAuthStateProvider.cs`
  `CustomAuthStateProvider` builds the claims of the signed-in user, including
  `NameIdentifier` (the user id), `Name` (email) and `Role`. Pages read the `NameIdentifier`
  claim and pass the user id to services, see [[flashcard-ownership]].
- **Protected pages.** A page is protected with the `Authorize` attribute, for example
  `Components/Pages/Flashcards.razor` `Authorize`.

## Configuration and wiring

- **Database provider.** In normal runs the app uses PostgreSQL through Npgsql with the
  connection string `FiszkiDatabase`. In **test mode** it uses an in-memory database named
  `FiszkiTestDb`. Test mode is on when the environment is `Test`, the `TestMode` setting is
  true, or the app is started with `--test-mode`. (`Program.cs` `isTestMode`)

## Related

- [[flashcard-deletion]]
- [[flashcard-ownership]]
- [[flashcard-entity]]
