# Proposal

## Why

Strona Flashcards to główny ekran aplikacji po zalogowaniu: lista fiszek, liczniki, filtrowanie,
widok kartowy i listowy, odwracanie kart i stany specjalne (ładowanie, błąd, pusty). Wiki opisuje
dziś tylko fragmenty tej strony (usuwanie, tworzenie ręczne), a analitycy pytają, co użytkownik
widzi, jak działają filtry i jakie są ograniczenia listy (limit 100, brak wyszukiwania i sortowania).

## What Changes

- Do `wiki/` trafia strona o liście fiszek i filtrach, zebrana przez `wiki-ingest` z
  `Flashcards.razor`, `FlashcardService` (lista i pobieranie), `FlashcardsUI.feature`, testów i kroków.
- Aktualizacja stron, które dotykają tej strony (`flashcard-deletion`, `manual-flashcard-creation`),
  glosariusza, indeksu i logu.
- Rozbieżności z `.ai/` (paginacja, filtrowanie i sortowanie po stronie serwera) w `Discrepancies`.

Poza zakresem: zmiany w kodzie, naprawa znalezionych problemów (np. ukrywanie filtrów przy pustej
liście po filtrze), nawigacja i strony informacyjne (osobna zmiana), dane kont testowych.

## Capabilities

### New Capabilities

Brak. Zmiana dodaje treść do wiki i nie zmienia zachowania żadnej capability.

### Modified Capabilities

Brak. Zmiana ma `skip_specs: true`.

## Impact

- Nowe i zmienione pliki wyłącznie w `wiki/`.
- Brak zmian w kodzie aplikacji, projektach .NET, CI i `openspec/config.yaml`.
- Strony nie zawierają danych kont testowych (loginów, haseł) ani adresów e-mail kont.
