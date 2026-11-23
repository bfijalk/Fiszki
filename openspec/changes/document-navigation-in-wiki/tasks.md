# Tasks

## 1. Zakres

- [x] 1.1 Wylistować pliki nawigacji, układu, stron informacyjnych, workflow CI i `Program.cs` i porównać z `design.md` (Context); zweryfikować, że nic istotnego nie pominięto

## 2. Ingest

- [x] 2.1 Wywołać skill `wiki-ingest` w trybie cold start dla "navigation, layout and information pages, plus CI/CD and application startup"; zweryfikować, że powstały strony: nawigacja, strony informacyjne (z zestawieniem twierdzeń i stanu faktycznego) oraz CI/CD i start aplikacji
- [x] 2.2 Zweryfikować poprawkę etykiety "About Me" na istniejących stronach oraz aktualizację `application-layers`, `index.md`, `glossary.md`, `log.md` i `verified_commit` na nowych i zmienionych stronach

## 3. Weryfikacja

- [x] 3.1 Uruchomić `wiki/tools/lint` i zweryfikować 0 błędów
- [x] 3.2 Sprawdzić skryptem co najmniej 10 twierdzeń względem kodu i workflow, w tym twierdzenia negatywne (brak linku Register w menu, brak kroku migracji, brak Application Insights, brak wdrożenia z ręcznego uruchomienia)
- [x] 3.3 Zadać przez `wiki-query` pytanie biznesowe i pytanie o implementację; zweryfikować, że odpowiedzi cytują nowe strony
- [x] 3.4 Zweryfikować, że w wiki nie ma danych osobowych z podstrony o autorze ani wartości sekretów i że zmiany dotyczą tylko `wiki/` i `openspec/changes/document-navigation-in-wiki/`
