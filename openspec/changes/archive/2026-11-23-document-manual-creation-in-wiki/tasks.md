# Tasks

## 1. Zakres

- [x] 1.1 Wylistować pliki ręcznego tworzenia i walidacji treści fiszek i porównać z `design.md` (Context); zweryfikować, że nic istotnego nie pominięto

## 2. Ingest

- [x] 2.1 Wywołać skill `wiki-ingest` w trybie cold start dla "manual flashcard creation and flashcard content validation"; zweryfikować, że powstała strona funkcji z sekcjami `Business rules` i `Implementation` oraz strona `flashcard-content-limits`
- [x] 2.2 Poprawić stronę `ai-flashcard-generation` (pokrycie testem zapisu przez UI wg `FlashcardCreation.feature`) i zweryfikować, że nie zawiera już twierdzenia o braku takiego testu
- [x] 2.3 Zweryfikować aktualizację `index.md`, `glossary.md`, `log.md` oraz `verified_commit` na nowych i zmienionych stronach

## 3. Weryfikacja

- [x] 3.1 Uruchomić `wiki/tools/lint` i zweryfikować 0 błędów
- [x] 3.2 Sprawdzić skryptem wartości limitów (1000, 2000, 50, 10, 500) względem walidatorów oraz co najmniej 3 inne twierdzenia względem kodu
- [x] 3.3 Zadać przez `wiki-query` pytanie biznesowe i pytanie o implementację; zweryfikować, że odpowiedzi cytują nowe strony
- [x] 3.4 Zweryfikować, że zmiany dotyczą tylko `wiki/` i `openspec/changes/document-manual-creation-in-wiki/`
