# Design

## Context

Stan obecny (szczegóły i motywacja w `proposal.md`):

- Pięć projektów (`Fiszki`, `Fiszki.Database`, `Fiszki.Services`, `Fiszki.Tests`,
  `Fiszki.FunctionalTests`) ma `net8.0` ustawione osobno w każdym `.csproj`. Nie ma
  `Directory.Build.props` ani `global.json`.
- Pakiety związane z frameworkiem są na 8.x (`EF Core` 8.0.8 / 8.0.0, `Npgsql.EFCore` 8.0.8,
  `Microsoft.Extensions.Configuration*` 8.0.0), a w `Fiszki.Tests` jest
  `Microsoft.Extensions.Logging` 9.0.10, czyli już wyższy niż framework.
- `Fiszki.Database` zawiera migracje EF Core (`Migrations/`).
- CI (`ci-cd.yml`) ma jedną zmienną `DOTNET_VERSION: '8.0.x'` używaną w trzech jobach oraz
  na sztywno wpisaną ścieżkę `bin/Debug/net8.0/playwright.ps1`.
- Deploy z `main` idzie na Azure Web App `Fiszki` przez `azure/webapps-deploy@v3`.
- Lokalnie jest zainstalowany tylko SDK 10.0.105.

## Goals / Non-Goals

**Goals:**
- Wszystkie projekty budują się i przechodzą testy na `net10.0`, lokalnie i w CI.
- Pakiety zależne od frameworka są w linii 10.x, więc nie ma mieszania wersji EF Core
  i `Microsoft.Extensions.*` z różnych głównych wersji.
- Odniesienia do .NET 8 w CI i dokumentacji są aktualne.

**Non-Goals:**
- Wprowadzanie nowych funkcji C# 14 / .NET 10 do istniejącego kodu.
- Podbijanie pakietów, które działają na net10.0 bez zmian (MudBlazor, xUnit, Moq,
  FluentAssertions, AutoFixture, BCrypt), poza koniecznością.
- Wprowadzanie `Directory.Build.props` ani `global.json` (osobna decyzja, patrz niżej).
- Nowe migracje bazy danych ani zmiana schematu.
- Zmiana konfiguracji Azure Web App (poza repozytorium).

## Decisions

1. **Cel `net10.0` we wszystkich pięciu projektach naraz.** Projekty są powiązane
   referencjami (`Fiszki` → `Database` i `Services`, testy → oba), a mieszanie TFM
   wymagałoby osobnej obsługi. Alternatywa: podbijać warstwami. Odrzucona, bo przy pięciu
   małych projektach daje więcej pracy bez zysku.

2. **Zachowujemy `TargetFramework` w każdym `.csproj` zamiast wprowadzać
   `Directory.Build.props`.** Zmiana mechaniczna jest mała i łatwa do przejrzenia w PR.
   Centralizacja to refaktor niezwiązany z celem. Alternatywa: wspólny props. Odroczona.

3. **Pakiety Microsoft podbijamy do najnowszej stabilnej linii 10.x** (w momencie
   planowania: EF Core / `Microsoft.Extensions.*` 10.0.x, `Npgsql.EntityFrameworkCore.PostgreSQL`
   10.0.x), a nie do 11.x. Wersja 11 jest nowsza, ale użytkownik wskazał .NET 10, a linia 11
   nie odpowiada docelowemu frameworkowi. Konkretną wersję patch ustalamy przy
   implementacji, jedną dla wszystkich pakietów EF Core, żeby uniknąć konfliktów.

4. **Pakiety zewnętrzne zostawiamy, dopóki build i testy przechodzą.** Pakiety
   skompilowane pod net8.0 działają na net10.0. Podbijamy je tylko w razie błędu
   lub ostrzeżenia o zgodności. Dotyczy to zwłaszcza `SpecFlow 4.0.7-beta`
   (generacja kodu z plików `.feature` przez MSBuild) i `Microsoft.NET.Test.Sdk 17.8.0`.
   Jeśli SpecFlow nie działa na net10.0, awaryjnie można wyłączyć podbicie tylko
   `Fiszki.FunctionalTests` (zostaje na `net8.0`), ale wtedy trzeba zaktualizować
   ścieżkę Playwrighta i zdecydować o zakresie. Wymaga to osobnej decyzji z użytkownikiem.

5. **CI: `DOTNET_VERSION: '10.0.x'`, ścieżka Playwrighta budowana bez wpisanego na stałe
   TFM.** Zamiast `bin/Debug/net10.0/playwright.ps1` używamy ścieżki zależnej od
   wartości w jednym miejscu (np. zmienna `DOTNET_TFM`), żeby następny upgrade nie wymagał
   szukania po pliku. Alternatywa: tylko zamiana `net8.0` na `net10.0`. Prostsza, ale
   zostawia ten sam problem na przyszłość.

6. **Migracje EF Core nie są regenerowane.** Istniejące migracje są niezależne od wersji
   frameworka. Weryfikujemy, że `dotnet ef migrations has-pending-model-changes` (lub
   odpowiednik) nie pokazuje różnic po podbiciu EF Core. Jeśli pokaże, trzeba
   rozstrzygnąć, czy dodać migrację.

7. **Dokumentacja jest aktualizowana w tej samej zmianie**, w tym strona
   `AboutTechnicalities.razor`, bo to tekst widoczny dla użytkownika. Pliki w `.ai/`
   i `.github/instructions/` aktualizujemy tak samo, bo prowadzą asystentów AI
   do pisania kodu pod .NET 8. Wyjątek: `.ai/dotnet-stack-template.md` ma lokalne,
   niezacommitowane zmiany użytkownika, więc dotykamy go dopiero po ich zacommitowaniu
   lub uzgodnieniu.

## Risks / Trade-offs

- **[Azure Web App bez runtime .NET 10]** → Deploy z `main` po scaleniu może się
  nie udać lub aplikacja nie wstanie. Mitygacja: przed scaleniem PR potwierdzić
  w portalu Azure, że `Fiszki` ma stos .NET 10 (lub przełączyć go razem ze scaleniem).
  Tego nie da się sprawdzić z repozytorium.
- **[SpecFlow 4.0.7-beta niezgodny z net10.0]** → Testy funkcjonalne mogą się nie
  zbudować. Mitygacja: zbudować `Fiszki.FunctionalTests` jako pierwszy po zmianie TFM;
  przy błędzie zastosować ścieżkę z decyzji 4.
- **[Zmiany łamiące w ASP.NET Core 9/10 i EF Core 9/10]** (np. zachowanie `Include`,
  parametryzacja zapytań, uwierzytelnianie cookie, Blazor Server) → Mitygacja: pełny
  przebieg testów jednostkowych i funkcjonalnych, w tym ręczne sprawdzenie logowania
  i listy fiszek w trybie testowym.
- **[`setup-dotnet` w CI nie znajduje SDK 10.0.x]** → Mitygacja: uruchomić CI na
  gałęzi zmiany przed scaleniem.
- **[Wymuszenie nowego SDK dla wszystkich deweloperów]** → Każdy, kto buduje lokalnie,
  musi mieć SDK 10. Mitygacja: zaktualizować README (wymagany SDK) w tej samej zmianie.
- **[Rozbieżność wersji pakietów]** → Jeśli części EF Core / `Microsoft.Extensions.*`
  trafią na różne wersje główne, zobaczymy błędy runtime. Mitygacja: jedna wersja dla całej rodziny.

## Migration Plan

1. Praca na osobnej gałęzi, jedna zmiana = jeden PR.
2. Podbicie TFM i pakietów, `dotnet build` i `dotnet test` lokalnie.
3. Aktualizacja CI i dokumentacji, uruchomienie CI na gałęzi.
4. Przed scaleniem: potwierdzić runtime .NET 10 na Azure Web App `Fiszki`.
5. Scalenie → automatyczny deploy z `main`.

Rollback: cofnięcie commita scalającego (`git revert`) przywraca `net8.0` i CI na 8.0.x;
wymaga to też przywrócenia stosu .NET w Azure, jeśli został zmieniony.

## Open Questions

- Czy Azure Web App `Fiszki` jest już ustawiony na .NET 10? Nie zmienia to zakresu zmian
  w repozytorium, ale ustala termin scalenia.
