---
title: Information pages
type: feature
summary: The five open information pages (Home, How to Start, Examples, About Technicalities, About Me): what each contains, that they are static text, and a table of the claims they make that do not match the code, the workflow or the repository.
tags: [information, home, landing, how-to-start, guide, examples, samples, about, about-technicalities, about-me, claims, accuracy, marketing, spaced-repetition, ci-cd, deployment]
sources:
  - Components/Pages/Home.razor
  - Components/Pages/HowToStart.razor
  - Components/Pages/FlashcardsExamples.razor
  - Components/Pages/AboutTechnicalities.razor
  - Components/Pages/About.razor
  - Components/Pages/Flashcards.razor
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Services/OpenRouter/OpenRouterChatService.cs
  - Fiszki.csproj
  - Program.cs
  - .github/workflows/ci-cd.yml
  - Fiszki.FunctionalTests/Features/Navigation.feature
verified_commit: 5367383718e4da80f89f35abf89c2b78f6f682c8
confidence: high
---

# Information pages

## Summary

Five pages explain and promote the application. They are open to everyone, show fixed text and
sample data, and read no data from the database. They are written by hand, so **what they say is
not derived from the code**, and several statements do not match it, see the table under
Discrepancies. Do not treat these pages as a description of what the application does; use the
feature pages of this wiki. How the pages are reached is in [[site-navigation]].

## Business rules

For analysts. Each rule says how it is backed: **Gherkin**, **unit test**, or **code only** (read
from the implementation, no automated test).

1. **Home (`/`)** is the landing page: a pitch ("AI-Powered Flashcards for Smarter Learning"), three
   benefits (AI-generated content, spaced repetition, full control), a four-step "How It Works" and
   two buttons that both open the Login page. It looks the same for logged-in users. Backed by: code
   only (`Components/Pages/Home.razor` `NavigateToLogin`).
2. **How to Start (`/how-to-start`)** is a user guide with screenshots that open in a larger view:
   creating an account and logging in, AI generation, manual creation, and managing flashcards, ending
   with two quick-start checklists. Backed by: code only (`Components/Pages/HowToStart.razor`).
3. **Examples (`/examples`)** shows **ten built-in sample flashcards** in three categories (Language
   Learning, Science & Math, History & Culture), labeled AI or Manual. Visitors can filter by category
   and flip cards. The samples are written into the page; they are **not** other users' flashcards,
   and nothing is stored. Two buttons: "Get Started" (Login page) and "Try AI Generation" (the Generate
   page, which needs a login). Backed by: code only (`Components/Pages/FlashcardsExamples.razor`
   `GetFilteredExamples`).
4. **About Technicalities (`/about-technicalities`)** is the developer's technical description of the
   application: purpose, technology stack, architecture, the CI/CD pipeline, the Azure deployment and
   development practices. Backed by: code only (`Components/Pages/AboutTechnicalities.razor`).
5. **About Me (`/about`)** is a profile of the application's developer: professional background,
   areas of expertise and links to social profiles. It describes a person, not the product. Backed by:
   code only (`Components/Pages/About.razor`).
6. **All pages are in English** and have no data access. Backed by: code only.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Pages:** `Components/Pages/Home.razor`, `Components/Pages/HowToStart.razor`,
  `Components/Pages/FlashcardsExamples.razor`, `Components/Pages/AboutTechnicalities.razor`,
  `Components/Pages/About.razor`. Almost all markup. None calls a service.
- **How to Start:** a small code block opens an enlarged screenshot (`_showImageModal`); the images
  are in `wwwroot/Images/Instruction/`.
- **Examples:** the ten samples are a list in the page's code block; category buttons filter it and a
  per-card flag flips it. The page shows at most ten.
- **Home:** injects the navigation manager only to send both buttons to `/login`.

### Test coverage

- **Gherkin:** only that the home page loads and shows a Login link
  (`Fiszki.FunctionalTests/Features/Navigation.feature`, scenario "Nav shows Login link when not authenticated").
- **Not tested:** the content, the screenshots, the Examples filter and flip, and the buttons.
- **Drift:** any change to these pages marks this wiki page stale, because they are listed as
  sources.

## Dependencies and side effects

- Link to the real features: [[ai-flashcard-generation]], [[manual-flashcard-creation]],
  [[flashcard-list-and-filters]], [[flashcard-deletion]], [[user-registration]],
  [[user-login-and-logout]].
- The deployment and pipeline claims are checked against [[ci-cd-and-startup]].

## Discrepancies

The in-app texts disagree with the code in many places. **The code wins** (see SCHEMA, trust
hierarchy). Verdicts: **True** (matches the code), **False** (the code does not do this),
**Partly**, or **Not checkable** (cannot be seen in the repository).

### Home

| Claim | Verdict | What the code does |
|---|---|---|
| "Built-in spaced repetition algorithm", "Study with spaced repetition" | **False** | There is no study or review screen and no scheduling code. The review-state table exists but nothing writes it, see [[learning-progress-entity]]. |
| "Create, edit, and manage your flashcards manually" | **Partly** | Create and delete work; the Edit button does nothing, see [[manual-flashcard-creation]]. |
| "Paste any text and let our AI create perfect flashcards" | **Partly** | The text is used as a topic in the prompt, not as material to extract from, see [[ai-flashcard-generation]]. |
| "Accept, edit, or reject generated cards" | **True** (accept only in bulk) | See [[ai-flashcard-generation]]. |
| "10x faster", "thousands of students" | **Not checkable** | Marketing statements. |

### How to Start

| Claim | Verdict | What the code does |
|---|---|---|
| "Click on Register in the navigation" | **False** | The menu has no Register link; registration is reached from the Login page, see [[site-navigation]]. |
| AI generation: minimum 50 characters, language, 1 to 50 cards | **True** | See [[ai-flashcard-generation]]. |
| "Longer content produces better results" | **Not checkable** | The text is capped at 5000 characters and used as a topic. |
| "Add Manual Card" to create a card | **Partly** | That button exists only when the user already has cards; with none, it is "Create Manually". |
| "Smart Filtering" by AI, Manual or all | **True** | See [[flashcard-list-and-filters]]. |
| "Click to flip between question and answer" | **True** (card view only) | See [[flashcard-list-and-filters]]. |
| "Edit Cards: modify any flashcard content" | **False** | The Edit button does nothing. |
| "Delete Cards" | **True** | See [[flashcard-deletion]]. |
| "Tagging System: organize cards with custom tags" | **Partly** | Tags can be typed and are stored, but are never shown, searched or filtered, see [[manual-flashcard-creation]]. |
| Quick start "Register, then Login" | **Partly** | Registering already logs the user in, see [[user-registration]]. |

### Examples

| Claim | Verdict | What the code does |
|---|---|---|
| "See how other users create effective flashcards" | **False** | Ten samples built into the page; no user data. |
| "Join thousands of learners" | **Not checkable** | Marketing statement. |

### About Technicalities

| Claim | Verdict | What the code or the repository shows |
|---|---|---|
| Core features "Interactive learning sessions" and "Progress tracking" | **False** | Not implemented, see [[learning-progress-entity]]. |
| Stack lists "ASP.NET Core 8" next to ".NET 10" | **False** | Every project targets .NET 10 (`Fiszki.csproj`). |
| "Repository Pattern" | **False** | Services use the database context directly. |
| "CQRS (Command Query)" | **Partly** | Commands are plain records passed to services; there is no query side and no handlers. |
| "Async Processing: background jobs" | **Partly** | An in-memory background task per service instance, see [[generation-job-flow]]. |
| "Resilient AI calls", "retry logic" | **False** | No retries; a failed AI call is replaced by placeholder cards, see [[openrouter-integration]]. |
| "Token Management: cost optimization" | **False** | The cost is always zero (a TODO) and token use is only logged, see [[openrouter-integration]]. |
| "Integration Tests: database interactions" | **Partly** | There is no integration test project; unit tests use an in-memory database. |
| "Code coverage analysis", "TRX reports" | **Partly** | Coverage is collected and TRX files are produced; no report or quality gate step exists, see [[ci-cd-and-startup]]. |
| "Database migration execution", "automated rollback", "health check validation" in deployment | **False** | None of these is in the workflow or in the startup code, see [[ci-cd-and-startup]]. |
| "Manual Dispatch: on-demand deployment" | **False** | A manual run exists but the deploy job runs only for a push to the main branch, see [[ci-cd-and-startup]]. |
| Azure Web App deployment to the Production slot | **True** | See [[ci-cd-and-startup]]. |
| "Application Insights monitoring" | **False** | No such package or configuration in the code. |
| "Security scanning", "code quality gates", "performance monitoring" | **False** | No such step in the workflow. |
| Auto-scale, managed certificates, private endpoint, daily backups, custom domain, zero-downtime | **Not checkable** | Cloud settings that are not in the repository. |
| Purpose "learn Polish vocabulary" | **True** | The generation prompt is written for Polish learners, see [[ai-flashcard-generation]]. |

## Related

- [[site-navigation]]
- [[ci-cd-and-startup]]
- [[ai-flashcard-generation]]
- [[manual-flashcard-creation]]
- [[flashcard-list-and-filters]]
- [[application-layers]]
- [[glossary]]
