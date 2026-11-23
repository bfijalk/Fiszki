# Tasks

## 1. Zakres

- [x] 1.1 Wylistować pliki listy fiszek (strona, serwis, DTO, scenariusze, kroki, testy) i porównać z `design.md` (Context); zweryfikować, że nic istotnego nie pominięto

## 2. Ingest

- [x] 2.1 Wywołać skill `wiki-ingest` w trybie cold start dla "flashcard list, counters, filters, views and flip on the Flashcards page"; zweryfikować, że powstała strona funkcji z sekcjami `Business rules` i `Implementation`
- [x] 2.2 Zweryfikować aktualizację `flashcard-deletion`, `manual-flashcard-creation`, `index.md`, `glossary.md`, `log.md` oraz `verified_commit` na nowych i zmienionych stronach

## 3. Weryfikacja

- [x] 3.1 Uruchomić `wiki/tools/lint` i zweryfikować 0 błędów
- [x] 3.2 Sprawdzić skryptem co najmniej 8 twierdzeń względem kodu, w tym limit 100 i kolejność, filtry po źródle oraz twierdzenia negatywne (brak wyszukiwania, sortowania, paginacji, wyświetlania tagów, scenariusza z filtrem)
- [x] 3.3 Zadać przez `wiki-query` pytanie biznesowe i pytanie o implementację; zweryfikować, że odpowiedzi cytują nową stronę
- [x] 3.4 Zweryfikować, że w wiki nie ma danych kont testowych oraz że zmiany dotyczą tylko `wiki/` i `openspec/changes/document-flashcard-list-in-wiki/`
