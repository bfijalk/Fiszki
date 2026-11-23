# Tasks

## 1. Przygotowanie

- [x] 1.1 Utworzyć gałąź `upgrade-dotnet-10` z aktualnego `main` i zweryfikować, że `git status` nie zawiera cudzych zmian w plikach `.csproj` ani w `ci-cd.yml`.
- [x] 1.2 Zapisać stan wyjściowy: uruchomić `dotnet build Fiszki.sln` i `dotnet test Fiszki.Tests` na `net8.0` i zanotować wynik, żeby odróżnić regresje po podbiciu od błędów istniejących wcześniej. Stan wyjściowy (net8.0): build OK, 0 błędów, 1 ostrzeżenie CS0169; testy jednostkowe 52/52 (uruchomione z DOTNET_ROLL_FORWARD=Major, bo lokalnie jest tylko runtime 10).

## 2. Podbicie frameworka i pakietów

- [x] 2.1 Ustawić `<TargetFramework>net10.0</TargetFramework>` w `Fiszki.Database`, `Fiszki.Services` i `Fiszki` i zweryfikować, że `dotnet build Fiszki.sln` kompiluje te trzy projekty bez błędów.
- [x] 2.2 Podbić `Microsoft.EntityFrameworkCore`, `.Design`, `.InMemory` (w `Fiszki`, `Fiszki.Database`, `Fiszki.Tests`, `Fiszki.FunctionalTests`) oraz `Npgsql.EntityFrameworkCore.PostgreSQL` do jednej wersji 10.0.x i zweryfikować, że `dotnet list package` pokazuje tę samą wersję EF Core we wszystkich projektach. Wynik: EF Core 10.0.15 i Npgsql 10.0.4 (wymaga EF Core >= 10.0.15) we wszystkich projektach; EF Core 10 nie wspiera net8.0, więc 2.3 i 2.4 wykonano razem z 2.2.
- [x] 2.3 Ustawić `net10.0` w `Fiszki.Tests`, podbić `Microsoft.Extensions.Logging` do 10.0.x i zweryfikować, że `dotnet test Fiszki.Tests` przechodzi z takim samym wynikiem jak w 1.2.
- [x] 2.4 Ustawić `net10.0` w `Fiszki.FunctionalTests`, podbić `Microsoft.Extensions.Configuration` i `.Json` do 10.0.x i zweryfikować, że `dotnet build Fiszki.FunctionalTests` generuje pliki `.feature.cs` i kończy się bez błędów. Jeśli SpecFlow `4.0.7-beta` nie działa na net10.0, zatrzymać się i uzgodnić z użytkownikiem dalsze kroki (patrz design.md, decyzja 4). Wynik: SpecFlow 4.0.7-beta działa na net10.0, wygenerowano 8 plików .feature.cs, build bez błędów.
- [x] 2.5 Przejrzeć ostrzeżenia kompilatora i `dotnet list package --vulnerable --deprecated` i zweryfikować, że nie pojawiły się nowe ostrzeżenia o zgodności pakietów z net10.0; podbić tylko te pakiety, których dotyczą. Wynik: brak nowych ostrzeżeń (zniknęło NU1903 dla Microsoft.Extensions.Caching.Memory 8.0.0), brak podatnych pakietów; xunit 2.9.3 oznaczony "Legacy" (xunit.v3) istniał wcześniej i nie dotyczy net10.0, więc nie podbijany.

## 3. Weryfikacja zachowania aplikacji

- [x] 3.1 Sprawdzić migracje EF Core poleceniem `dotnet ef migrations has-pending-model-changes --project Fiszki.Database --startup-project Fiszki.Database` i zweryfikować, że nie zgłasza zmian modelu. Nie generować nowej migracji bez uzgodnienia. Wynik: "No changes have been made to the model since the last migration." Uwaga: globalny dotnet-ef to 9.0.9 (starszy niż runtime 10.0.15), działa, ale warto go zaktualizować (dotnet tool update -g dotnet-ef).
- [x] 3.2 Uruchomić aplikację w trybie testowym (`./test-mode.sh`) i zweryfikować ręcznie: strona ładuje się, logowanie `test@example.com` przekierowuje na `/flashcards`, lista fiszek się wyświetla, komponenty MudBlazor renderują się poprawnie. Wynik: aplikacja startuje na net10.0 w trybie testowym (dane zasiane), strony /, /login, /register, /about-technicalities oraz zasoby MudBlazor zwracają 200, /flashcards przekierowuje anonima na login; logowanie, lista fiszek i widoki sprawdzone testami Playwright z 3.3 (nie ręcznie w przeglądarce).
- [x] 3.3 Uruchomić testy funkcjonalne lokalnie przeciw aplikacji w trybie testowym i zweryfikować, że przechodzą z takim samym wynikiem jak w stanie wyjściowym z 1.2. Wynik (net10.0, aplikacja uruchomiona jak w CI przez dotnet run z profilem startowym, klucz OpenRouter w zmiennej środowiskowej): 16 zaliczonych, 0 niezaliczonych, 1 pominięty (Ignored w kodzie). Bez klucza 2 scenariusze generowania padały na "OpenRouter API key is required". Stanu wyjściowego dla testów funkcjonalnych na net8.0 nie zapisano (brak runtime 8 lokalnie), kryterium przyjęto jako pełne zaliczenie.

## 4. CI/CD

- [x] 4.1 W `.github/workflows/ci-cd.yml` zmienić `DOTNET_VERSION` na `'10.0.x'` i zweryfikować, że joby `build` i `unit-tests` na gałęzi przechodzą na zielono. Wynik: w CI (PR #22) joby Build i Unit Tests przeszły na SDK 10.0.x.
- [x] 4.2 Zastąpić na sztywno wpisaną ścieżkę `bin/Debug/net8.0/playwright.ps1` wartością zależną od jednej zmiennej TFM (np. `DOTNET_TFM: 'net10.0'`) i zweryfikować, że krok instalacji Playwrighta w jobie `functional-tests` znajduje skrypt i instaluje Chromium. Wynik: w CI (PR #22) krok Playwrighta znalazł skrypt i zainstalował Chromium.
- [x] 4.3 Zweryfikować, że job `functional-tests` na PR przechodzi w całości, a `required-status-check` jest zielony. Wynik: PR #22 w pełni zielony (Functional Tests 4 min 41 s, Required Status Check pass).

## 5. Dokumentacja

- [x] 5.1 Zaktualizować `README.md` (badge, opis stosu, link i wymóg SDK 10, stopka) i zweryfikować przez `grep -n "\.NET 8\|net8" README.md`, że nie zostały odniesienia do wersji 8.
- [x] 5.2 Zaktualizować `Components/Pages/AboutTechnicalities.razor` (cztery miejsca z ".NET 8", w tym wiersz tabeli z "LTS") i zweryfikować, że strona renderuje się z poprawnymi tekstami w uruchomionej aplikacji. Wynik: strona /about-technicalities renderuje 4x ".NET 10" i 0x ".NET 8" (sprawdzone na uruchomionej aplikacji).
- [x] 5.3 Zaktualizować odniesienia do .NET 8 w `.ai/tech-stack.md`, `.ai/plan-testow.md`, `.ai/openrouter-service-implementation-plan.md` i `.github/instructions/C# blazor web instructions.instructions.md` i zweryfikować przez `grep -rn "net8\|\.NET 8" .ai .github`, że nie zostały odniesienia (poza `.ai/dotnet-stack-template.md`). Dodatkowo poprawiono ".NET Runtime: 8.0" w .ai/plan-testow.md.
- [x] 5.4 Po zacommitowaniu lub uzgodnieniu lokalnych zmian użytkownika zaktualizować `.ai/dotnet-stack-template.md` (wersja, `TargetFramework`, obrazy Docker `aspnet:10.0` i `sdk:10.0`, stopka) i zweryfikować przez `grep -n "8\.0\|net8" .ai/dotnet-stack-template.md`. Wynik: zaktualizowano TFM, wersje EF Core/Npgsql/Extensions i obrazy Docker; plik jest nowym, niezacommitowanym plikiem użytkownika, więc nie wchodzi do commita tej zmiany.

## 6. Integracja

- [x] 6.1 Zweryfikować, że `grep -rIn "net8\.0" --exclude-dir=bin --exclude-dir=obj --exclude-dir=.git --exclude-dir=openspec .` nie zwraca żadnych trafień. Wynik: brak trafień w repozytorium (poza openspec/).
- [x] 6.2 Zweryfikować, że `dotnet build Fiszki.sln -c Release` i `dotnet publish Fiszki.sln -c Release` (jak w CI) kończą się sukcesem. Wynik: build i publish Release bez błędów (0 błędów, 1 ostrzeżenie CS0169 sprzed zmiany); runtimeconfig opublikowanej aplikacji ma tfm net10.0.

## Workflow follow-up

- Przed scaleniem PR potwierdzić w portalu Azure, że Web App `Fiszki` ma stos .NET 10 (lub przełączyć go razem ze scaleniem), bo deploy z `main` uruchamia się automatycznie.
- Po scaleniu zweryfikować zielony przebieg jobu `deploy` i działanie aplikacji produkcyjnej.
- Zarchiwizować zmianę (`openspec-archive-change`) po spełnieniu wymagań review.
