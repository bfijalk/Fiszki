# Tech Stack (skrót)

Minimalny, aktualny i docelowy zestaw technologii dla projektu.

## Core
- Runtime / Framework: .NET 8 (ASP.NET Core Blazor Server)
- Język: C# 12
- BaaS / Baza: Supabase (PostgreSQL, Auth, Row Level Security, Edge Functions)
- AI Gateway: OpenRouter (wielomodelowy dostęp; JSON jako strict output)
- Algorytm nauki: Custom SM-2 (plan: możliwość rozszerzenia do FSRS)

## Frontend / UI
- Stylowanie: Tailwind CSS (docelowo v4, fallback 3.x)
- Komponenty: Port wybranych shadcn/ui do Razor (fallback: MudBlazor)
- Format danych z AI: JSON (ściśle walidowany schema)

## Testy / Jakość
- Jednostkowe: xUnit
- Komponentowe / UI (plan): bUnit / Playwright

## DevOps / Infra
- Kontenery: Docker (multi-stage build)
- CI/CD: GitHub Actions
- Hosting: DigitalOcean (App Platform lub Droplet)
- Sekrety / Konfiguracja: Zmienne środowiskowe + GitHub Secrets
- Observability (MVP): Logi stdout (plan: OpenTelemetry w późniejszym etapie)

## Bezpieczeństwo
- Izolacja danych: Supabase RLS
- Autentykacja: Supabase Auth (JWT)
- Ograniczenie kluczy: OpenRouter API key per środowisko

## Decyzje skrót (ADR)
- Supabase zamiast własnego CRUD backendu.
- Blazor Server na start (szybsze iteracje); opcja migracji do WASM później.
- OpenRouter dla elastyczności modeli i kontroli kosztów.
- Tailwind + port shadcn.ui dla spójnego designu.
- Własna implementacja SM-2 (kontrola, prostota, testowalność).

---
Plik ograniczony do esencji technologicznej. Rozszerzony kontekst (wymagania, funkcje) w `prd.md` (jeśli/dopóki istnieje).
