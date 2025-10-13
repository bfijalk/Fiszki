# Tech Stack (skrót)

Minimalny, aktualny i docelowy zestaw technologii dla projektu.

## Core
- Runtime / Framework: .NET 8 (ASP.NET Core Blazor Server)
- Język: C# 12
- Baza danych: PostgreSQL (EF Core / Npgsql)
- ORM: Entity Framework Core 8
- AI Gateway: OpenRouter
- Algorytm nauki: SM-2 (plan: możliwe rozszerzenie do FSRS)

## Frontend / UI
- Stylowanie: (plan) Tailwind CSS
- Komponenty: (plan) port shadcn/ui do Razor (fallback: MudBlazor)
- Format danych z AI: JSON (walidowany)

## Testy / Jakość
- Jednostkowe: xUnit (plan)
- UI: bUnit / Playwright (plan)

## DevOps / Infra
- Kontenery: Docker (plan)
- CI/CD: GitHub Actions (plan)
- Hosting: DigitalOcean (plan)
- Sekrety: zmienne środowiskowe / user-secrets
- Observability (MVP): logi stdout

## Bezpieczeństwo
- Autentykacja / Autoryzacja: własna implementacja (users + hasła hashowane)
- Izolacja danych: logika usług

## Decyzje skrót (ADR)
- Pełna kontrola backend + baza (rezygnacja z BaaS).
- Blazor Server na start (szybka iteracja).
- OpenRouter dla elastyczności modeli.
- Własna implementacja SM-2.

---
Plik ograniczony do esencji technologicznej. Szczegóły funkcjonalne w `prd.md`.
