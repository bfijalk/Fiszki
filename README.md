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
- Simple per-user isolation (service layer).
- GDPR-friendly data handling (user-controlled deletion).

---

## Tech Stack (summary)
- .NET 8 / Blazor Server
- PostgreSQL + EF Core (Npgsql)
- OpenRouter (LLM gateway)
- Tailwind CSS (planned)

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

## References
- PRD: `.ai/prd.md`
- Tech stack: `.ai/tech-stack.md`
- OpenRouter Docs
- EF Core Docs
- PostgreSQL Docs

---

