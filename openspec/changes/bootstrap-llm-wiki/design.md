# Design

## Context

Repozytorium Fiszki (Blazor + .NET 10) nie ma jednego miejsca z wiedzą o działaniu
funkcji. Źródła prawdy to kod, osiem plików `.feature` (Gherkin, reguły biznesowe wprost)
i dopiero rodzący się `openspec/specs/` (obecnie pusty). Folder `.ai/` zawiera
historyczne plany, częściowo nieaktualne, i będzie wygaszony. Motywacja w `proposal.md`.

Z wiki korzystają programiści i analitycy biznesowi, obie grupy przez agentów. Wiki
jest po angielsku, a pytania mogą być po polsku.

## Goals / Non-Goals

**Goals:**
- Wiki o Fiszkach w repo, którą agent buduje przyrostowo i która nie dryfuje po cichu.
- Dwie perspektywy na tę samą wiedzę: reguły biznesowe i implementacja.
- Wyszukiwanie wywoływalne przez agenta, deterministyczne i lokalne.
- Weryfikowalność: każde twierdzenie da się sprawdzić w źródle.

**Non-Goals:**
- Pełne pokrycie wszystkich funkcji w tej zmianie (tylko cold start na jednej).
- Interfejs dla czytelników bez agenta.
- Wyszukiwanie semantyczne (embeddingi) w tej zmianie; kontrakt na nie przygotowany.
- Uruchamianie ingestu z CI. Ingest z archiwizacji OpenSpec jest w zakresie (D9).

## Decisions

### D1. Wiki w `wiki/` w tym repo, schema w `wiki/SCHEMA.md`
Wiki wersjonuje się razem z kodem, więc zmiana funkcji i aktualizacja wiki mogą wejść w
jednym PR. Schema leży w `wiki/SCHEMA.md`, a nie w głównym `CLAUDE.md`, żeby instrukcje
bibliotekarza wiki nie mieszały się z instrukcjami do pracy nad kodem. Krótki wskaźnik
do `SCHEMA.md` trafia do pliku instrukcji agenta w katalogu głównym.
*Alternatywa:* osobne repo. Odrzucone, bo źródłem jest kod tego repo, a dryf byłby
trudniejszy do wykrycia.

### D2. Źródłem jest kod, a nie `raw/`
Nie ma warstwy `raw/` z kopiami. Schema deklaruje ścieżki-źródła: `Components/`,
`Fiszki.Services/`, `Fiszki.Database/`, `Fiszki.FunctionalTests/Features/*.feature`,
`openspec/specs/`, a `.ai/` jako podpowiedź. Hierarchia zaufania: kod i `.feature` >
`openspec/specs` > `.ai/`.
*Alternatywa:* kopiować dokumenty do `raw/`. Odrzucone: duplikacja żywych źródeł.

### D3. Aktualność przez `sources` + `verified_commit`
Każda strona w frontmatterze deklaruje `sources` (ścieżki) i `verified_commit` (SHA, przy
którym ostatnio zweryfikowano ją z kodem). Ingest przyrostowy i lint obliczają
nieaktualność deterministycznie: `git diff --name-only <verified_commit>..HEAD -- <sources>`.
To zamienia "agent pamięta, co się zmieniło" na weryfikowalny fakt.
*Alternatywa:* jeden globalny znacznik "ostatni ingest". Odrzucone: nie wskazuje, które
strony są nieaktualne.

### D4. Odnośniki do kodu przez ścieżkę i symbol
Odnośnik to `ścieżka` + nazwa symbolu (klasa, metoda), np. `Fiszki.Services/Services/FlashcardService.cs`
`DeleteAsync`. Numery linii są opcjonalne, bo dryfują. Lint sprawdza istnienie pliku i
symbolu.

### D5. Operacje jako projektowe skille Claude Code
Trzy cienkie skille w `.claude/skills/`: `wiki-ingest`, `wiki-query`, `wiki-lint`. Każdy
odsyła do `wiki/SCHEMA.md` i nie powiela reguł. Analityk nie musi znać komend: zadaje
pytanie, a skill `wiki-query` się dopasowuje. Treść reguł jest w jednym miejscu
(schema), a skille są tylko punktami wejścia.
*Alternatywa:* same instrukcje w `CLAUDE.md`. Odrzucone: brak jawnych punktów wejścia
i mieszanie z instrukcjami kodu.

### D6. Wyszukiwanie: lokalny CLI z rankingiem leksykalnym (BM25), stały kontrakt
Agent wywołuje jedną komendę, np. `wiki/tools/search <query> [--type T] [--tag T]
[--limit N] [--json]`. Narzędzie:
1. wczytuje strony z `wiki/`, parsuje frontmatter,
2. buduje indeks w pamięci przy każdym uruchomieniu (przy dziesiątkach stron to
   milisekundy, więc brak trwałego indeksu do utrzymania i unieważniania),
3. rankuje BM25 z wagami pól: `title` i `tags` > `summary` > treść,
4. zwraca ścieżkę, tytuł, typ, summary, fragment i wynik.

Indeks nie jest przechowywany, więc nowa wiedza jest wyszukiwalna natychmiast po
zapisaniu strony. "Indeksowanie po nowej wiedzy" sprowadza się do dwóch rzeczy:
aktualizacji `index.md` (nawigacja dla agenta) i poprawnego frontmattera (wejście do
wyszukiwania). Żaden krok budowania indeksu nie może zostać zapomniany.

Zapytanie po polsku: agent tłumaczy je na angielskie terminy przed wywołaniem
wyszukiwania (wiki jest po angielsku), a odpowiada w języku pytającego. To jest
zachowanie opisane w schemie, bo ranking leksykalny nie przekroczy bariery językowej.

Implementacja: aplikacja jednoplikowa .NET (file-based app), bo toolchain jest już
wymagany przez repo i nie dokładamy Pythona ani Node. Dwa szczegóły wyniknęły przy
implementacji:
- **Plik `wiki/tools/wikitool` nie ma rozszerzenia `.cs`.** `Fiszki.csproj` leży w
  katalogu głównym, a SDK kompiluje każde `*.cs` pod nim, więc plik `.cs` trafiłby do
  aplikacji webowej i zepsuł build. Bez rozszerzenia (ze shebangiem `#!/usr/bin/env
  dotnet`) SDK go ignoruje, a `dotnet run --file` działa. Dzięki temu nie zmieniamy
  żadnego projektu .NET.
- **Jeden program z podkomendami (`search`, `lint`) zamiast jednego pliku na narzędzie**,
  żeby parser frontmattera i definicja "co jest stroną" były w jednym miejscu. Polecenia
  `wiki/tools/search` i `wiki/tools/lint` to cienkie wrappery bashowe, które przekazują
  argumenty po `--` (inaczej `dotnet run` przechwytuje np. `--help`).
*Alternatywy:*
- Sam `ripgrep` + `index.md`: bez rankingu i bez filtrów po typie i tagach; słabnie przy
  rozroście wiki.
- Wektorowe/hybrydowe wyszukiwanie od razu (np. lokalne embeddingi): dodatkowa
  zależność, model do pobrania, trwały indeks do unieważniania. Przedwczesne dla
  skali dziesiątek stron.
- Zewnętrzna usługa: odrzucona (poufność kodu, zależność od sieci).

Gdy wiki urośnie i recall leksykalny przestanie wystarczać, ranking można zamienić na
hybrydowy za tym samym kontraktem (spec `wiki-search`), bez zmian w skillach.

### D7. Lint jako skrypt + osąd agenta
Kontrole deterministyczne (martwe linki, osierocone strony, istnienie ścieżek i symboli,
dryf z D3, frontmatter) wykonuje skrypt `wiki/tools/lint`, tak aby dawały powtarzalny
wynik. Kontrole wymagające osądu (twierdzenie bez źródła, koncept bez strony, sprzeczności)
robi agent, korzystając z wyniku skryptu. Lint nie zmienia stron.

### D8. Cold start jako dowód: jedna funkcja, "flashcard deletion"
Zakres zadań implementacyjnych kończy się jednym pełnym ingestem (usuwanie fiszek, bo
istnieje `FlashcardDeletion.feature`), jednym zapytaniem testowym i jednym lintem. Pełne
pokrycie to kolejne zmiany.

### D9. Ingest jako ostatni krok archiwizacji, przez `config.yaml`
Do `openspec/config.yaml` dochodzi `operations.archive.guidance` z instrukcją: po
zsynchronizowaniu specs i przeniesieniu zmiany do archiwum uruchom skill `wiki-ingest`
w trybie przyrostowym i dołącz jego raport do podsumowania archiwizacji. Ingest jest
ostatni, bo czyta zaktualizowane `openspec/specs/` (źródło nr 2 w hierarchii) i
aktualny kod. To wskazówka (guidance), a nie twardy hak, więc działa przez agenta
wykonującego archiwizację; lint dryfu (D3) łapie przypadki, w których krok został
pominięty.
*Alternatywa:* hook w CI lub git. Odrzucone: ingest wymaga agenta i osądu, a CI nie ma
dostępu do kontekstu zmiany. Wpis do `config.yaml` jest edycją konfiguracji workflow i
jest zadaniem w `tasks.md`, wykonywanym dopiero w fazie apply.

### D10. `.ai/` jest wygaszane i usuwane
`.ai/` nie jest trwałym źródłem. Po cold starcie i przeglądzie jego zawartości folder
można usunąć, a hierarchia zaufania w schemie traci trzeci poziom. Usunięcie jest
krokiem następczym, a nie zadaniem śledzonym: `.ai/` zawiera niezacommitowane zmiany
(`agent-rules.md`, `dotnet-stack-template.md`), które właściciel musi świadomie
zachować lub odrzucić. Wiki nie może zależeć od istnienia `.ai/`, więc `sources` stron
nie wskazują na ten folder.

## Risks / Trade-offs

- [Wiki starzeje się szybciej niż jest odświeżana] → `verified_commit` + lint dryfu
  + ostrzeżenie o nieaktualności w odpowiedziach query + ingest w archiwizacji (D9).
- [Wskazówka w `config.yaml` może zostać zignorowana albo zmniejszyć przejrzystość
  archiwizacji] → raport ingestu w podsumowaniu archiwizacji; lint dryfu wykrywa
  pominięcia.
- [Usunięcie `.ai/` kasuje wiedzę, której wiki jeszcze nie ma] → przegląd zawartości
  przed usunięciem; niezacommitowane pliki wymagają decyzji właściciela.
- [Halucynacje przy opisie kodu] → obowiązkowe odnośniki do plików i symboli,
  weryfikowane lintem; twierdzenia niezweryfikowane dostają `confidence: low`.
- [Dziedziczenie nieścisłości z `.ai/`] → hierarchia zaufania, niska pewność dla
  twierdzeń tylko z `.ai/`.
- [Ranking leksykalny nie łapie synonimów] → `tags` i `summary` utrzymywane przez
  ingest, `glossary.md` z synonimami; rozbudowa rankingu możliwa bez zmiany kontraktu.
- [Skrypty narzędziowe to dodatkowy kod do utrzymania] → minimalny zakres, jeden plik
  na narzędzie, bez zależności poza biblioteką standardową.
- [Ingest dużych zmian kosztuje tokeny] → ingest przyrostowy tylko dla stron, których
  `sources` się zmieniły.

## Migration Plan

Zmiana jest addytywna: nowe pliki w `wiki/` i `.claude/skills/` oraz wpis w
`openspec/config.yaml`. Brak migracji i wpływu na aplikację. Wycofanie to usunięcie
tych katalogów, wskaźnika w pliku instrukcji i wpisu `operations.archive` z konfiguracji.

## Open Questions

- Czy przy usuwaniu `.ai/` przenosić z niego cokolwiek do `openspec/specs/`? Rozstrzyga
  przegląd zawartości przed usunięciem, poza zakresem tej zmiany.
