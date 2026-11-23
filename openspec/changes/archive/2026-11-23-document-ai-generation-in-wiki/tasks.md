# Tasks

## 1. Zakres

- [x] 1.1 Wylistować pliki związane z generowaniem w repo i porównać z listą źródeł w `design.md` (Context); zweryfikować, że żaden istotny plik nie został pominięty bez powodu

## 2. Ingest

- [x] 2.1 Wywołać skill `wiki-ingest` w trybie cold start dla "AI flashcard generation"; zweryfikować, że powstała strona funkcji z sekcjami `Business rules` i `Implementation` oraz strona architektury integracji z OpenRouter
- [x] 2.2 Zweryfikować, że `wiki/index.md`, `wiki/glossary.md` i `wiki/log.md` zostały zaktualizowane, a każda nowa lub zmieniona strona ma `verified_commit` równy aktualnemu HEAD

## 3. Weryfikacja

- [x] 3.1 Uruchomić `wiki/tools/lint` i zweryfikować 0 błędów (ostrzeżenia o otwartych `Discrepancies` są dozwolone)
- [x] 3.2 Sprawdzić wyrywkowo co najmniej 5 twierdzeń z nowych stron względem kodu (plik i symbol) i zapisać wynik w raporcie
- [x] 3.3 Zadać przez skill `wiki-query` pytanie o regułę biznesową i pytanie o implementację generowania; zweryfikować, że odpowiedzi cytują nowe strony
- [x] 3.4 Zweryfikować, że w wiki nie ma wartości klucza API i że zmiany tej zmiany dotyczą tylko `wiki/` i `openspec/changes/document-ai-generation-in-wiki/`
