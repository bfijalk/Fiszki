# Proposal

## Why

Wszystkie projekty w repozytorium celują w `net8.0`. .NET 10 to wersja LTS, a lokalnie
(SDK 10.0.105) i tak pracujemy już na nowszym SDK niż docelowy framework. Podbicie
wersji teraz utrzymuje projekt w wspieranym cyklu i zapobiega rozjazdowi między
środowiskiem lokalnym, CI i produkcją.

## What Changes

- Zmiana `TargetFramework` z `net8.0` na `net10.0` w pięciu projektach:
  `Fiszki`, `Fiszki.Database`, `Fiszki.Services`, `Fiszki.Tests`, `Fiszki.FunctionalTests`.
- Podbicie pakietów NuGet związanych z frameworkiem do linii 10.x:
  `Microsoft.EntityFrameworkCore*`, `Npgsql.EntityFrameworkCore.PostgreSQL`,
  `Microsoft.Extensions.*`.
- Weryfikacja zgodności pakietów zewnętrznych z net10.0 (MudBlazor, SpecFlow
  `4.0.7-beta`, Playwright, xUnit, FluentValidation, Moq, AutoFixture). Podbijamy je
  tylko wtedy, gdy build lub testy tego wymagają.
- CI (`.github/workflows/ci-cd.yml`): `DOTNET_VERSION` z `8.0.x` na `10.0.x` oraz
  ścieżka `bin/Debug/net8.0/playwright.ps1` na `net10.0`.
- Aktualizacja dokumentacji i tekstów wspominających .NET 8: `README.md`,
  `Components/Pages/AboutTechnicalities.razor`, `.ai/*`, `.github/instructions/*`.
- Brak zmian w zachowaniu aplikacji widocznym dla użytkownika.

## Capabilities

### New Capabilities
<!-- Brak: to zmiana platformy, nie zachowania systemu. -->

### Modified Capabilities
<!-- Brak zmian wymagań. Zmiana ma ustawione `skip_specs: true` (w repozytorium nie ma
     jeszcze żadnych specyfikacji, a podbicie frameworka nie zmienia zachowania). -->

## Impact

- **Kod**: pliki `*.csproj`; ewentualne poprawki kompilacji po zmianie wersji
  (zmiany łamiące w ASP.NET Core 9/10 i EF Core 9/10).
- **Zależności**: pakiety NuGet opisane wyżej; SpecFlow `4.0.7-beta` to najbardziej
  ryzykowna zależność.
- **CI/CD**: workflow buduje i testuje na `10.0.x`; `actions/setup-dotnet@v4` musi
  umieć pobrać SDK 10.
- **Wdrożenie**: Azure Web App `Fiszki` musi obsługiwać runtime .NET 10, w przeciwnym
  razie deploy z `main` po scaleniu zakończy się błędem lub aplikacja się nie uruchomi.
  Tej konfiguracji nie da się zmienić z poziomu repozytorium.
- **Dokumentacja**: odniesienia do .NET 8 w README, stronie "Technikalia" i plikach `.ai/`.
