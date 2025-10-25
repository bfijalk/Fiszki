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

### Testy jednostkowe
- **xUnit**: Framework testowy (domyślny dla .NET)
- **FluentAssertions**: Czytelne asercje i porównania
- **AutoFixture**: Automatyczne generowanie danych testowych
- **Moq**: Mockowanie zależności i interfejsów
- **Microsoft.EntityFrameworkCore.InMemory**: Szybkie testy z bazą w pamięci
- **Testcontainers**: Integracja z rzeczywistą PostgreSQL w kontenerze

### Testy end-to-end / UI
- **bUnit**: Testowanie komponentów Blazor Server
- **Playwright**: Testy e2e w przeglądarce (cross-browser)
- **AngleSharp**: Parsowanie HTML w testach komponentów
- **Microsoft.AspNetCore.Mvc.Testing**: Testy integracyjne z TestServer

### Narzędzia jakości
- **Coverlet**: Analiza pokrycia kodu
- **ReportGenerator**: Raporty pokrycia w formacie HTML
- **NBomber**: Testy wydajnościowe i obciążeniowe
- **BenchmarkDotNet**: Micro-benchmarki algorytmów (SM-2)

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
- xUnit + bUnit + Playwright dla pełnej piramidy testowej.

---
Plik ograniczony do esencji technologicznej. Szczegóły funkcjonalne w `prd.md`.
