# Fiszki (10x-cards)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-alpha-orange)](#project-status)
[![Build](https://img.shields.io/badge/build-GitHub_Actions-lightgrey?logo=github)](#project-status)
[![License](https://img.shields.io/badge/license-TBD-lightgrey)](#license)

> Fast AI‑assisted creation and spaced-repetition study of flashcards (Blazor Server + PostgreSQL + OpenRouter).

A lightweight Blazor Server application (working name: “Fiszki”, product codename: “10x-cards”) focused on dramatically reducing the time needed to produce high‑quality study flashcards by leveraging LLM generation plus a classic SM‑2 spaced repetition loop.

---

## Table of Contents

1. [Project Name](#fiszki-10x-cards)
2. [Project Description](#project-description)
3. [Tech Stack](#tech-stack)
4. [Getting Started Locally](#getting-started-locally)
5. [Testing Strategy](#testing-strategy)
6. [Available Scripts](#available-scripts)
7. [Project Scope](#project-scope)
8. [Project Status](#project-status)
9. [License](#license)
10. [Environment Variables](#environment-variables)
11. [Architecture (Conceptual)](#architecture-conceptual)
12. [Security & Privacy](#security--privacy)
13. [Roadmap / Future Enhancements](#roadmap--future-enhancements)
14. [Contributing](#contributing)
15. [Metrics & Success Criteria](#metrics--success-criteria)
16. [References](#references)

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
- Simple per-user isolation (service layer).
- GDPR-friendly data handling (user-controlled deletion).

---

## Tech Stack (summary)
- .NET 8 / Blazor Server
- PostgreSQL + EF Core (Npgsql)
- OpenRouter (LLM gateway)
- Tailwind CSS (planned)
- **Testing**: xUnit + bUnit + Playwright
- **Quality**: FluentAssertions, Moq, Coverlet

---

## Getting Started Locally

```bash
git clone https://github.com/bfijalk/Fiszki.git
cd Fiszki
dotnet restore
dotnet run
```

Navigate to: https://localhost:5001 (or shown in console).

### Database
Configured connection string key: `FiszkiDatabase`.

Sample local PostgreSQL (macOS/Homebrew):
```bash
brew install postgresql@16
brew services start postgresql@16
createdb fiszki_dev || true
```

### Environment Variables (example)
```bash
export OPENROUTER_API_KEY="sk-or-..."
export AI_MODEL="meta-llama/Meta-Llama-3-70B-Instruct"
```

User Secrets (dev):
```bash
dotnet user-secrets init
dotnet user-secrets set "OpenRouter:ApiKey" "sk-or-..."
```

---

## Scope (MVP)
In:
- AI flashcard generation
- Manual CRUD
- Accept / edit / reject suggestions
- SM-2 study session
- Basic stats (generated vs accepted)
- Account & data deletion

Out:
- Advanced repetition variants
- Gamification / streaks
- Mobile apps
- Multi-format ingestion
- Public API
- Sharing / collaboration
- Advanced notifications
- Full-text / semantic search

---

## Status
Alpha scaffold.
Implemented:
- Blazor setup
- EF Core infrastructure (empty DbContext)

---

## Security & Privacy
- Password hashing & auth logic pending.
- Account deletion will cascade user data.
- No third-party data store dependencies besides PostgreSQL.

---

## Roadmap (condensed)
- Auth & user management
- Generation workflow
- Study loop (SM-2)
- Basic stats
- Styling & components
- Docker / CI

---

## Metrics
- ≥75% acceptance of AI suggestions
- ≥75% of new cards from AI
- Weekly active study sessions (TBD)

---

## Testing Strategy

### Unit Tests (xUnit)
```bash
# Run all unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Core technologies:**
- **xUnit**: Primary testing framework for .NET
- **FluentAssertions**: Readable assertions (`result.Should().BeEquivalentTo(expected)`)
- **AutoFixture**: Automatic test data generation
- **Moq**: Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory**: Fast in-memory database for tests

**Test coverage:**
- Domain services (UserService, FlashcardService, GenerationService)
- FluentValidation validators
- SM-2 algorithm implementation
- Entity mapping and business logic

### Integration Tests
```bash
# Run integration tests with real PostgreSQL
dotnet test --filter "Category=Integration"
```

**Technologies:**
- **Testcontainers**: Real PostgreSQL instances in Docker
- **Microsoft.AspNetCore.Mvc.Testing**: TestServer for full app testing
- **WebApplicationFactory**: End-to-end HTTP testing

### UI Tests (Blazor Components)
```bash
# Run component tests
dotnet test --filter "Category=UI"
```

**Technologies:**
- **bUnit**: Testing Blazor Server components
- **AngleSharp**: HTML parsing and DOM assertions
- **Playwright**: Browser automation for E2E tests (planned)

**Test coverage:**
- Authentication flow (Login/Register components)
- Flashcard generation workflow
- Study session components
- Form validation and user interactions

### Performance & Load Testing
```bash
# Run performance benchmarks
dotnet run --project Fiszki.Benchmarks -c Release
```

**Technologies:**
- **NBomber**: Load testing framework for .NET
- **BenchmarkDotNet**: Micro-benchmarks for algorithms
- **MiniProfiler**: Database query profiling

### Quality Metrics
- **Target coverage**: 85% for business logic, 70% overall
- **Performance**: <2s AI generation, <100ms DB queries
- **Security**: OWASP Top 10 compliance testing

See [Test Plan](plan-testow.md) for detailed testing strategy.

---

## Available Scripts
