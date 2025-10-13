# Fiszki (10x-cards)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-alpha-orange)](#project-status)
[![Build](https://img.shields.io/badge/build-GitHub_Actions-lightgrey?logo=github)](#project-status)
[![License](https://img.shields.io/badge/license-TBD-lightgrey)](#license)

> Fast AI‑assisted creation and spaced-repetition study of flashcards (Blazor Server + Supabase + OpenRouter).

A lightweight Blazor Server application (working name: “Fiszki”, product codename: “10x-cards”) focused on dramatically reducing the time needed to produce high‑quality study flashcards by leveraging LLM generation plus a classic SM‑2 spaced repetition loop.

---

## Table of Contents

1. [Project Name](#fiszki-10x-cards)
2. [Project Description](#project-description)
3. [Tech Stack](#tech-stack)
4. [Getting Started Locally](#getting-started-locally)
5. [Available Scripts](#available-scripts)
6. [Project Scope](#project-scope)
7. [Project Status](#project-status)
8. [License](#license)
9. [Environment Variables](#environment-variables)
10. [Architecture (Conceptual)](#architecture-conceptual)
11. [Security & Privacy](#security--privacy)
12. [Roadmap / Future Enhancements](#roadmap--future-enhancements)
13. [Contributing](#contributing)
14. [Metrics & Success Criteria](#metrics--success-criteria)
15. [References](#references)

---

## Project Description

Manual creation of flashcards is time‑consuming and often discourages learners from consistently applying spaced repetition. This project streamlines that workflow:

Core value:
- Paste source text (1,000–10,000 chars) → receive AI‑proposed question/answer pairs.
- Accept, edit, or reject suggestions before persisting.
- Practice cards via a spaced repetition session (SM‑2 baseline, future FSRS extension).

MVP Goals:
- >75% acceptance rate of AI-suggested cards.
- ≥75% of newly added cards originate from AI generation.
- Simple, secure per-user isolation (Supabase Auth + RLS).
- GDPR-friendly data handling (user-controlled deletion).

Current repository state: baseline Blazor Server scaffold (startup pipeline only) – business features pending implementation.

---

## Tech Stack

Core:
- Runtime / Framework: .NET 8 (ASP.NET Core Blazor Server)
- Language: C# 12
- Data & Auth BaaS: Supabase (PostgreSQL, Row Level Security, Auth, Edge Functions)
- AI Gateway: OpenRouter (multi-model, strict JSON output enforcement)
- Learning Algorithm: Custom SM-2 implementation (future FSRS support)

Frontend / UI:
- Styling: Tailwind CSS (target v4; fallback 3.x) (not yet wired)
- Component Strategy: Port of shadcn/ui to Razor (fallback MudBlazor)
- AI Data Format: JSON validated against schema

Testing / Quality:
- Unit: xUnit (planned)
- Component/UI: bUnit / Playwright (planned)
- Linting / Formatting: dotnet format (can be added)

DevOps / Infra:
- Containers: Docker multi-stage build (future)
- CI/CD: GitHub Actions (workflow TBD)
- Hosting Target: DigitalOcean App Platform / Droplet
- Observability MVP: stdout logs (future: OpenTelemetry traces & metrics)

Security:
- Auth: Supabase Auth (JWT)
- Data Isolation: PostgreSQL RLS
- Secrets: Environment variables + GitHub Secrets
- API Keys: Per-environment OpenRouter key

Architectural Decisions (ADR Snapshot):
- Use Supabase instead of building custom CRUD backend early.
- Start with Blazor Server for rapid iteration (option to migrate to WASM later).
- OpenRouter for model flexibility and cost control.
- Tailwind + adapted shadcn/ui for consistent design system.
- Own SM-2 implementation for control & testability.

---

## Getting Started Locally

### Prerequisites
- .NET 8 SDK: https://dotnet.microsoft.com/
- (Planned) Node.js (LTS 20.x) & npm if/when Tailwind build is added.
- Supabase project (https://supabase.com/) with:
  - Auth enabled
  - RLS policies prepared (future step)
- OpenRouter API key: https://openrouter.ai/
- Git (for cloning)

### Clone and Run

```bash
git clone https://github.com/bfijalk/Fiszki.git
cd Fiszki
dotnet restore
dotnet run
```

Navigate to: https://localhost:5001 (or as indicated in console).

### Database (Local PostgreSQL + EF Core Stub)

The solution now includes a `Fiszki.Database` class library with Entity Framework Core configured for PostgreSQL (Npgsql). At this stage there are **no entities or migrations** – the context exists purely to validate connectivity and prepare for upcoming data model work.

Connection strings:
* `appsettings.json` → `FiszkiDatabase` (generic / production placeholder)
* `appsettings.Development.json` → `FiszkiDatabase` (points to `fiszki_dev` database)

Sample local PostgreSQL setup (macOS with Homebrew):
```bash
brew install postgresql@16
brew services start postgresql@16
createdb fiszki_dev
psql fiszki_dev -c "CREATE USER postgres WITH PASSWORD 'postgres'" 2>/dev/null || true
```

The application will attempt a lightweight connectivity check on startup (`Database.CanConnectAsync()`). Failures are logged but do not currently prevent the app from running (policy can be tightened later).

Next data steps (not yet implemented):
1. Introduce initial entities (Flashcard, Deck, Tag, ReviewSession, ReviewLog).
2. Add first migration: `dotnet ef migrations add InitialCreate -p Fiszki.Database -s Fiszki`.
3. Apply migrations automatically or via CLI: `dotnet ef database update -p Fiszki.Database -s Fiszki`.
4. Add repository / service abstractions for querying & persistence.

Until entities are added, running EF Core migration commands will produce an empty model snapshot.

### Configuration & Secrets

During MVP development, prefer environment variables or user secrets (avoid committing secrets).

Example (macOS/Linux):
```bash
export SUPABASE_URL="https://xyzcompany.supabase.co"
export SUPABASE_ANON_KEY="your_anon_key"
export OPENROUTER_API_KEY="sk-or-..."
export AI_MODEL="meta-llama/Meta-Llama-3-70B-Instruct" # example
```

For User Secrets (development):
```bash
dotnet user-secrets init
dotnet user-secrets set "Supabase:Url" "https://xyz.supabase.co"
dotnet user-secrets set "Supabase:AnonKey" "..."
dotnet user-secrets set "OpenRouter:ApiKey" "sk-or-..."
```

Update `Program.cs` later to read & register required services.

### (Planned) Tailwind Integration

When adding Tailwind:
1. Initialize Node project and install Tailwind:
   ```bash
   npm init -y
   npm install -D tailwindcss postcss autoprefixer
   npx tailwindcss init -p
   ```
2. Configure `tailwind.config.js` to scan `.razor`, `.cshtml`, `.html`.
3. Add input `./Styles/tailwind.css` with `@tailwind base; @tailwind components; @tailwind utilities;`
4. Build CSS:
   ```bash
   npx tailwindcss -i ./Styles/tailwind.css -o wwwroot/app.css --watch
   ```
5. Reference `app.css` (already present) or replace.

### (Planned) Docker

A future multi-stage Dockerfile (sample sketch):

```Dockerfile
# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Fiszki.dll"]
```

Build & run (future):
```bash
docker build -t fiszki .
docker run -p 8080:8080 --env-file .env fiszki
```

---

## Available Scripts

Dotnet CLI:
- `dotnet restore` – Restore project dependencies (currently none beyond base).
- `dotnet build` – Compile the project.
- `dotnet run` – Run development server.
- `dotnet watch run` – Live-reload dev loop (optional).
- `dotnet publish -c Release` – Produce optimized build for deployment.
- `dotnet test` – (Future) Run unit & component tests.
- `dotnet format` – (Optional) Code style/format enforcement if added.

(Planned) NPM Scripts (after Tailwind setup):
- `npm run dev:css` – Tailwind watch mode.
- `npm run build:css` – Production CSS build.

Docker (planned):
- `docker build -t fiszki .`
- `docker run -p 8080:8080 fiszki`

---

## Project Scope

In Scope (MVP):
- AI flashcard generation (paste text → suggestions list)
- Manual create/edit/delete
- Accept / edit / reject AI suggestions
- Per-user auth & isolation
- SM-2 learning session view
- Basic stats (generated vs accepted)
- GDPR-compliant deletion (account + cards)

Out of Scope (MVP):
- Advanced / custom spaced repetition beyond baseline
- Gamification & streaks
- Mobile apps
- Multi-format document ingestion (PDF/DOCX)
- Public API
- Sharing / collaborative decks
- Advanced notifications
- Advanced search / full-text indexing

User Stories: US-001 .. US-009 (Registration, Login, AI Generation, Review, Edit, Delete, Manual Create, Study Session, Secure Access).

---

## Project Status

Status: Alpha / Scaffold

Implemented:
- Blazor Server baseline (`Program.cs`, routing, static files)
- EF Core PostgreSQL infrastructure (empty DbContext, connection string, DI registration)

Not Yet Implemented:
- Supabase integration (Auth, DB, RLS)
- EF Core entities & migrations
- AI integration (OpenRouter client, prompt/response schema)
- Flashcard domain model + persistence
- SM-2 scheduling logic & session UI
- Stats & metrics collection
- Tailwind & component library port
- Tests (xUnit, bUnit/Playwright)
- Dockerfile & CI workflow
- Observability/Telemetry
- GDPR data export/delete endpoints

### Roadmap (High-Level)

| Milestone | Description | Status |
|-----------|-------------|--------|
| M1 | Core data model + Supabase auth | Planned |
| M2 | AI generation pipeline & validation | Planned |
| M3 | Manual CRUD + review workflow | Planned |
| M4 | SM-2 study session | Planned |
| M5 | Stats & metrics | Planned |
| M6 | Tailwind + component library styling | Planned |
| M7 | Testing (xUnit + bUnit) | Planned |
| M8 | Docker + CI/CD | Planned |
| M9 | Telemetry & performance baseline | Planned |

---

## License

License: TBD (Recommended: MIT or Apache-2.0 for open source, or a closed-source LICENSE file if proprietary).

Add a `LICENSE` file before public release.

---

## Environment Variables

| Variable | Purpose | Required | Notes |
|----------|---------|----------|-------|
| SUPABASE_URL | Supabase project URL | Yes | Provided by Supabase |
| SUPABASE_ANON_KEY | Public anon key (client ops) | Yes | Treat with care; still public |
| SUPABASE_SERVICE_ROLE_KEY | Elevated operations (server only) | No (dev) | Never expose to browser |
| OPENROUTER_API_KEY | AI gateway key | Yes (AI features) | Different per environment |
| AI_MODEL | Default model identifier | No | e.g., meta-llama/... |
| ASPNETCORE_ENVIRONMENT | Runtime environment | No (defaults) | Development / Production |
| LOG_LEVEL | Logging override | No | Future enhancement |

---

## Architecture (Conceptual)

Blazor Server (SignalR) ←→ Supabase (Auth + Postgres + RLS)
                           |
                           → AI Gateway (OpenRouter → selected LLM)
                           |
                           → Spaced Repetition (SM-2 logic in app)

Data Flow (Generation):
1. User pastes text
2. Backend calls OpenRouter with structured prompt
3. JSON response validated
4. Suggestions presented for accept/edit/reject
5. Accepted cards stored (tagged origin=AI/manual)
6. Scheduler assigns next review dates

Study Session:
- Query due cards (SM-2 fields: interval, easiness, repetitions, due date)
- User rates recall → Update scheduling fields.

---

## Security & Privacy

- RLS ensures per-user row isolation (to be configured in Supabase).
- Supabase Auth JWT associated with each request; server validates claims before data access.
- GDPR Considerations:
  - Right to delete: planned endpoint to cascade delete user + cards.
  - Right to access: potential export feature (future).
- Secrets stored in environment / GitHub Secrets (never in repo).
- API key scoping: distinct keys per environment (dev/staging/prod).

---

## Roadmap / Future Enhancements

- FSRS algorithm upgrade
- Deck sharing + collaboration
- Full-text search across cards
- Import pipelines (PDF, web clipping)
- Gamification (streaks, badges)
- Offline-capable WASM client
- OpenTelemetry tracing & structured metrics
- Adaptive model selection (cost vs quality)

---

## Contributing

Early stage – contributions welcome after core scaffold lands.
Suggested next steps for contributors:
1. Implement Supabase service registration & repository layer.
2. Define Flashcard entity + SM-2 scheduling fields.
3. Add AI integration service with response schema validation.
4. Introduce xUnit tests for scheduling logic.

Please open an issue before large changes.

---

## Metrics & Success Criteria

Target KPIs:
- ≥75% acceptance of AI-suggested cards (quality indicator)
- ≥75% of new cards sourced via AI (efficiency indicator)
- Track ratio: generated vs accepted vs manually created
- (Future) Daily active study sessions & retention curves

Instrumentation Plan:
- Event logging: GenerationRequested, SuggestionsReceived(count), CardAccepted/Rejected, StudyRated(difficulty)
- Aggregation: Basic SQL views / materialized reports

---

## References

- PRD: `.ai/prd.md`
- Tech Stack summary: `.ai/tech-stack.md`
- Supabase Docs: https://supabase.com/docs
- OpenRouter: https://openrouter.ai/
- SM-2 Algorithm Reference: https://www.supermemo.com/en/archives1990-2015/english/ol/sm2
- Tailwind CSS: https://tailwindcss.com/

---

> NOTE: This README reflects planned architecture and features not yet implemented in this commit state. Update sections as milestones are delivered.

---

Let us know via Issues if any section needs clarification or if you begin implementing a milestone and discover architectural adjustments are needed.

### Next Steps (Database Layer)

Planned persistence tasks:
1. Add core entities (Flashcard, Deck, Tag, ReviewSession, ReviewLog) into `Fiszki.Database`.
2. Create initial EF Core migration and apply to dev database.
3. Introduce repository/services abstraction layer (query interfaces) to decouple UI/business logic from EF Core specifics.
4. Add integration tests using a disposable PostgreSQL instance (e.g., Testcontainers) and unit tests for scheduling logic.
5. Evaluate enabling sensitive data logging only in Development for troubleshooting.
