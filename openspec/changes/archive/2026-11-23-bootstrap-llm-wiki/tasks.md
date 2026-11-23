# Tasks

## 1. Szkielet wiki i schema

- [x] 1.1 Utworzyć `wiki/` z katalogami `features/`, `business-rules/`, `architecture/`, `data-model/`, plikami `index.md`, `log.md`, `glossary.md` i zweryfikować, że wszystkie istnieją, a `index.md` odwołuje się do każdego katalogu
- [x] 1.2 Napisać `wiki/SCHEMA.md` (po angielsku): ścieżki-źródła, hierarchia zaufania, frontmatter, szablon strony funkcjonalności, format odnośników do kodu, format wpisu logu, reguła "agent odpowiada w języku pytania, szuka po angielsku"; zweryfikować, że każde wymaganie ze `wiki-structure` ma odpowiadający zapis w schemie
- [x] 1.3 Dodać szablony stron (`feature`, `business-rule`, `architecture`, `data-model`, `synthesis`) w `wiki/templates/` i zweryfikować, że każdy zawiera pełny frontmatter
- [x] 1.4 Dodać krótki wskaźnik do `wiki/SCHEMA.md` w pliku instrukcji agenta w katalogu głównym (utworzyć, jeśli brak; nie nadpisywać istniejącej treści) i zweryfikować, że agent ma dostęp do schemy

## 2. Wyszukiwanie

- [x] 2.1 Zaimplementować `wiki/tools/search` (aplikacja jednoplikowa .NET, bez zależności zewnętrznych): wczytywanie stron, parsowanie frontmattera, ranking BM25 z wagami pól, opcje `--type`, `--tag`, `--limit`, `--json`; zweryfikować na zestawie testowych stron, że wynik zawiera path, title, type, summary, snippet, score
- [x] 2.2 Dodać testy zachowania wyszukiwania na fikcyjnej mini-wiki w katalogu testowym: trafienie w tytuł przed trafieniem w treść, filtr typu, pusty wynik z kodem wyjścia 0; zweryfikować, że testy przechodzą
- [x] 2.3 Udokumentować użycie wyszukiwania w `wiki/SCHEMA.md` (kiedy czytać `index.md`, kiedy wywołać search) i zweryfikować, że udokumentowana komenda działa dokładnie tak, jak opisano

## 3. Lint

- [x] 3.1 Zaimplementować `wiki/tools/lint`: martwe linki, strony poza `index.md`, strony osierocone, niepoprawny frontmatter; zweryfikować na mini-wiki z celowo wprowadzonymi błędami, że każdy jest zgłoszony
- [x] 3.2 Dodać weryfikację odnośników do kodu (istnienie pliku i symbolu) oraz wykrywanie dryfu przez `git diff --name-only <verified_commit>..HEAD -- <sources>`; zweryfikować testem na mini-repo, że zmieniony plik oznacza stronę jako nieaktualną
- [x] 3.3 Zapewnić, że lint niczego nie modyfikuje (poza wpisem logu przy uruchomieniu przez agenta) i zweryfikować, że `git status` po uruchomieniu nie pokazuje zmian w stronach

## 4. Skille operacji

- [x] 4.1 Dodać `.claude/skills/wiki-ingest` (cold start dla funkcji oraz tryb przyrostowy z `verified_commit`) odsyłający do `wiki/SCHEMA.md`; zweryfikować, że skill jest widoczny na liście skilli i opisuje oba tryby oraz obowiązkowy raport
- [x] 4.2 Dodać `.claude/skills/wiki-query` (czytanie `index.md`, wywołanie search, cytowanie stron, jawne luki, ostrzeżenie o nieaktualności, propozycja zapisu syntezy); zweryfikować na pytaniu bez pokrycia w wiki, że skill zgłasza lukę zamiast zgadywać
- [x] 4.3 Dodać `.claude/skills/wiki-lint` (uruchomienie skryptu + kontrole wymagające osądu, raport bez zmian stron); zweryfikować, że skill wywołuje `wiki/tools/lint` i kończy raportem

## 5. Cold start: usuwanie fiszek

- [x] 5.1 Wykonać ingest funkcji "flashcard deletion" z kodu i `FlashcardDeletion.feature` (z `.ai/` tylko jako podpowiedź); zweryfikować, że powstała strona funkcji z sekcjami `Business rules` i `Implementation`, powiązane strony danych i glosariusz, a źródła w repo nie zostały zmienione
- [x] 5.2 Zweryfikować po ingeście wpisy w `index.md` i `log.md` oraz `verified_commit` na każdej utworzonej stronie
- [x] 5.3 Zadać pytanie testowe po polsku (reguły biznesowe) i po angielsku (implementacja) i zweryfikować, że odpowiedzi cytują strony wiki, a pierwsza leży w języku pytania
- [x] 5.4 Uruchomić lint na wiki po cold starcie i zweryfikować, że nie zgłasza martwych linków ani błędnych odnośników do kodu

## 6. Ingest w archiwizacji OpenSpec

- [x] 6.1 Dodać do `openspec/config.yaml` wpis `operations.archive.guidance`: po synchronizacji specs i przeniesieniu zmiany do archiwum uruchomić `wiki-ingest` (tryb przyrostowy) i dołączyć raport do podsumowania; zachować istniejącą treść pliku i zweryfikować, że `openspec validate` oraz `openspec list --json` nadal działają
- [x] 6.2 Zweryfikować krok w praktyce: zarchiwizować próbną zmianę (np. na tymczasowej gałęzi) i sprawdzić, że ingest wykonuje się jako ostatni krok, raport trafia do podsumowania, a `log.md` ma nowy wpis; próbną zmianę i jej skutki następnie wycofać
- [x] 6.3 Opisać krok w `wiki/SCHEMA.md` (kolejność: sync specs, archiwum, ingest) i zweryfikować, że `wiki-ingest` obsługuje wejście "zmiana właśnie zarchiwizowana" oraz scenariusz "zmiana nie dotyka niczego udokumentowanego"

## 7. Integracja

- [x] 7.1 Zmienić jeden plik źródłowy funkcji usuwania (na tymczasowej gałęzi), uruchomić lint i zweryfikować, że strona jest zgłoszona jako nieaktualna, a ingest przyrostowy aktualizuje tylko ją; zmianę źródła następnie wycofać
- [x] 7.2 Zweryfikować wszystkie scenariusze ze specyfikacji `wiki-structure`, `wiki-ingest`, `wiki-query`, `wiki-lint` i `wiki-search` względem zachowania z zadań 5, 6.2 i 7.1 oraz uruchomić `openspec validate bootstrap-llm-wiki --strict`

## Workflow follow-up

- Skille `wiki-ingest`, `wiki-query` i `wiki-lint` zostały po pewnym czasie wykryte przez sesję; `wiki-query` wywołano przez narzędzie Skill na pytaniu z pokryciem w wiki i zadziałał end-to-end. `wiki-ingest` i `wiki-lint` nie były jeszcze wywołane jako skille (ich kroki wykonano ręcznie w zadaniach 5.1 i 6.2 oraz 7.1); warto to zrobić przy pierwszym prawdziwym użyciu.
- Archiwizować zmianę po spełnieniu wymagań przeglądu projektu. To będzie pierwsza archiwizacja z nowym krokiem ingestu.
- Usunąć folder `.ai/` po przeglądzie jego zawartości. Najpierw rozstrzygnąć niezacommitowane pliki (`agent-rules.md`, `dotnet-stack-template.md`) i usunąć trzeci poziom hierarchii zaufania ze `wiki/SCHEMA.md`.
- Rozważyć kolejne zmiany: pokrycie pozostałych funkcji, wyszukiwanie semantyczne.
