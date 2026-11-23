# Proposal

## Why

Generowanie fiszek przez AI jest najbardziej złożoną funkcją Fiszek: formularz źródłowy
z walidacją, wywołanie modelu przez OpenRouter, ocena i edycja propozycji, zapis wybranych
fiszek, limity i obsługa błędów. Analitycy i programiści najczęściej pytają właśnie o nią,
a wiki ma dziś wiedzę tylko o usuwaniu fiszek. Bez tej funkcji baza wiedzy nie odpowiada na
najważniejsze pytania biznesowe.

## What Changes

- Do `wiki/` trafia wiedza o generowaniu fiszek przez AI, zebrana przez `wiki-ingest`
  (cold start) z kodu, scenariusza `FlashcardGeneration.feature` i testów jednostkowych.
- Powstają strony: funkcja (reguły biznesowe i implementacja), integracja z OpenRouter
  (architektura), a w razie potrzeby osobne strony reguł i modeli danych, jeśli kod
  pokazuje reguły wspólne z innymi funkcjami.
- Aktualizacja `wiki/index.md`, `wiki/glossary.md` i `wiki/log.md`.
- Rozbieżności między kodem a historycznymi notatkami z `.ai/` są zapisywane w sekcji
  `Discrepancies`; kod wygrywa.

Poza zakresem: zmiany w kodzie aplikacji, ręczne tworzenie fiszek (osobna zmiana),
poprawianie znalezionych błędów, dokumentowanie wartości sekretów (klucz API).

## Capabilities

### New Capabilities

Brak. Zmiana dodaje treść do wiki i nie zmienia zachowania żadnej capability.

### Modified Capabilities

Brak. Zmiana ma `skip_specs: true`, bo specyfikacje opisują zachowanie systemu, a treść
wiki nie zmienia zachowania.

## Impact

- Nowe i zmienione pliki wyłącznie w `wiki/` (strony, indeks, glosariusz, log).
- Brak zmian w kodzie aplikacji, projektach .NET, CI i `openspec/config.yaml`.
- Po archiwizacji tej zmiany krok ingestu z `config.yaml` nie będzie miał nic do
  zaktualizowania: źródła stron są już zweryfikowane.
