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
verified_commit: c20f71dc4bf41ad1693b6ec4a52ab177fb5fe501
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

- **Who is the user.** (The login state itself is an in-memory object per browser connection, see
  [[authentication-session]].) `Fiszki.Services/Services/CustomAuthStateProvider.cs`
  `CustomAuthStateProvider` builds the claims of the signed-in user, including
  `NameIdentifier` (the user id), `Name` (email) and `Role`. Pages read the `NameIdentifier`
  claim and pass the user id to services, see [[flashcard-ownership]].
- **Outbound AI call.** The application has no HTTP API of its own, but it makes one outbound
  HTTP call: the generation service calls OpenRouter, see [[openrouter-integration]]. Generation
  runs as a background job inside the service, see [[generation-job-flow]].
- **Protected pages.** A page is protected with the `Authorize` attribute, for example
  `Components/Pages/Flashcards.razor` `Authorize`.

## Configuration and wiring

- **What happens at startup** (database check, test-mode seeding, no migrations) and how the code is
  built and deployed are in [[ci-cd-and-startup]].
- **Database provider.** In normal runs the app uses PostgreSQL through Npgsql with the
  connection string `FiszkiDatabase`. In **test mode** it uses an in-memory database named
  `FiszkiTestDb`. Test mode is on when the environment is `Test`, the `TestMode` setting is
  true, or the app is started with `--test-mode`. (`Program.cs` `isTestMode`)

## Related

- [[flashcard-deletion]]
- [[authentication-session]]
- [[ci-cd-and-startup]]
- [[site-navigation]]
- [[ai-flashcard-generation]]
- [[openrouter-integration]]
- [[generation-job-flow]]
- [[flashcard-ownership]]
- [[flashcard-entity]]
