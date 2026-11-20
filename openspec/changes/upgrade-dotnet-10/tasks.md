# Tasks

## 1. Przygotowanie

- [ ] 1.1 Utworzyć gałąź `upgrade-dotnet-10` z aktualnego `main` i zweryfikować, że `git status` nie zawiera cudzych zmian w plikach `.csproj` ani w `ci-cd.yml`.
- [ ] 1.2 Zapisać stan wyjściowy: uruchomić `dotnet build Fiszki.sln` i `dotnet test Fiszki.Tests` na `net8.0` i zanotować wynik, żeby odróżnić regresje po podbiciu od błędów istniejących wcześniej.

## 2. Podbicie frameworka i pakietów

- [ ] 2.1 Ustawić `<TargetFramework>net10.0</TargetFramework>` w `Fiszki.Database`, `Fiszki.Services` i `Fiszki` i zweryfikować, że `dotnet build Fiszki.sln` kompiluje te trzy projekty bez błędów.
- [ ] 2.2 Podbić `Microsoft.EntityFrameworkCore`, `.Design`, `.InMemory` (w `Fiszki`, `Fiszki.Database`, `Fiszki.Tests`, `Fiszki.FunctionalTests`) oraz `Npgsql.EntityFrameworkCore.PostgreSQL` do jednej wersji 10.0.x i zweryfikować, że `dotnet list package` pokazuje tę samą wersję EF Core we wszystkich projektach.
- [ ] 2.3 Ustawić `net10.0` w `Fiszki.Tests`, podbić `Microsoft.Extensions.Logging` do 10.0.x i zweryfikować, że `dotnet test Fiszki.Tests` przechodzi z takim samym wynikiem jak w 1.2.
- [ ] 2.4 Ustawić `net10.0` w `Fiszki.FunctionalTests`, podbić `Microsoft.Extensions.Configuration` i `.Json` do 10.0.x i zweryfikować, że `dotnet build Fiszki.FunctionalTests` generuje pliki `.feature.cs` i kończy się bez błędów. Jeśli SpecFlow `4.0.7-beta` nie działa na net10.0, zatrzymać się i uzgodnić z użytkownikiem dalsze kroki (patrz design.md, decyzja 4).
- [ ] 2.5 Przejrzeć ostrzeżenia kompilatora i `dotnet list package --vulnerable --deprecated` i zweryfikować, że nie pojawiły się nowe ostrzeżenia o zgodności pakietów z net10.0; podbić tylko te pakiety, których dotyczą.

## 3. Weryfikacja zachowania aplikacji

- [ ] 3.1 Sprawdzić migracje EF Core poleceniem `dotnet ef migrations has-pending-model-changes --project Fiszki.Database --startup-project Fiszki.csproj` i zweryfikować, że nie zgłasza zmian modelu. Nie generować nowej migracji bez uzgodnienia.
- [ ] 3.2 Uruchomić aplikację w trybie testowym (`./test-mode.sh`) i zweryfikować ręcznie: strona ładuje się, logowanie `test@example.com` przekierowuje na `/flashcards`, lista fiszek się wyświetla, komponenty MudBlazor renderują się poprawnie.
- [ ] 3.3 Uruchomić testy funkcjonalne lokalnie przeciw aplikacji w trybie testowym i zweryfikować, że przechodzą z takim samym wynikiem jak w stanie wyjściowym z 1.2.

## 4. CI/CD

- [ ] 4.1 W `.github/workflows/ci-cd.yml` zmienić `DOTNET_VERSION` na `'10.0.x'` i zweryfikować, że joby `build` i `unit-tests` na gałęzi przechodzą na zielono.
- [ ] 4.2 Zastąpić na sztywno wpisaną ścieżkę `bin/Debug/net8.0/playwright.ps1` wartością zależną od jednej zmiennej TFM (np. `DOTNET_TFM: 'net10.0'`) i zweryfikować, że krok instalacji Playwrighta w jobie `functional-tests` znajduje skrypt i instaluje Chromium.
- [ ] 4.3 Zweryfikować, że job `functional-tests` na PR przechodzi w całości, a `required-status-check` jest zielony.

## 5. Dokumentacja

- [ ] 5.1 Zaktualizować `README.md` (badge, opis stosu, link i wymóg SDK 10, stopka) i zweryfikować przez `grep -n "\.NET 8\|net8" README.md`, że nie zostały odniesienia do wersji 8.
- [ ] 5.2 Zaktualizować `Components/Pages/AboutTechnicalities.razor` (cztery miejsca z ".NET 8", w tym wiersz tabeli z "LTS") i zweryfikować, że strona renderuje się z poprawnymi tekstami w uruchomionej aplikacji.
- [ ] 5.3 Zaktualizować odniesienia do .NET 8 w `.ai/tech-stack.md`, `.ai/plan-testow.md`, `.ai/openrouter-service-implementation-plan.md` i `.github/instructions/C# blazor web instructions.instructions.md` i zweryfikować przez `grep -rn "net8\|\.NET 8" .ai .github`, że nie zostały odniesienia (poza `.ai/dotnet-stack-template.md`).
- [ ] 5.4 Po zacommitowaniu lub uzgodnieniu lokalnych zmian użytkownika zaktualizować `.ai/dotnet-stack-template.md` (wersja, `TargetFramework`, obrazy Docker `aspnet:10.0` i `sdk:10.0`, stopka) i zweryfikować przez `grep -n "8\.0\|net8" .ai/dotnet-stack-template.md`.

## 6. Integracja

- [ ] 6.1 Zweryfikować, że `grep -rIn "net8\.0" --exclude-dir=bin --exclude-dir=obj --exclude-dir=.git --exclude-dir=openspec .` nie zwraca żadnych trafień.
- [ ] 6.2 Zweryfikować, że `dotnet build Fiszki.sln -c Release` i `dotnet publish Fiszki.sln -c Release` (jak w CI) kończą się sukcesem.

## Workflow follow-up

- Przed scaleniem PR potwierdzić w portalu Azure, że Web App `Fiszki` ma stos .NET 10 (lub przełączyć go razem ze scaleniem), bo deploy z `main` uruchamia się automatycznie.
- Po scaleniu zweryfikować zielony przebieg jobu `deploy` i działanie aplikacji produkcyjnej.
- Zarchiwizować zmianę (`openspec-archive-change`) po spełnieniu wymagań review.
