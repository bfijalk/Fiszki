# Design

## Context

Ścieżka ręczna to okno "Create Manual Flashcard" w `Components/Pages/Flashcards.razor`,
`FlashcardService.CreateAsync`, `CreateFlashcardCommand` i jego walidator, a także
`UpdateAsync` z `UpdateFlashcardCommand` (obecny w serwisie, bez odpowiednika w UI: metoda
`EditCard` jest zaślepką). Scenariusze: `ManualFlashcardCreation.feature` (3) i
`FlashcardCreation.feature` (generowanie, zapis i sprawdzenie listy). Motywacja w `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Jedna strona z regułami i przepływem ręcznego tworzenia.
- Jedna strona z limitami treści zbierająca różne limity i ich niespójności.
- Rozdzielenie "co zrobi UI" od "co egzekwuje serwis".

**Non-Goals:**
- Usuwanie rozbieżności w kodzie (np. różne limity).
- Edycja fiszek w UI, której nie ma.

## Decisions

### D1. Osobna strona reguł dla limitów treści
Limity są wspólne dla kilku funkcji (ręczne tworzenie, zapis fiszek AI, aktualizacja), więc
należą do `business-rules/`, a nie do jednej strony funkcji. *Alternatywa:* limity na stronie
funkcji. Odrzucone: analityk szukałby ich w trzech miejscach.

### D2. Poprawki wcześniejszych stron są częścią ingestu
Gdy nowe źródło obala wcześniejsze twierdzenie (tu: pokrycie testem zapisu AI), strona jest
poprawiana w tej samej zmianie, z odnotowaniem w logu, a nie zostawiana do późniejszego lintu.

### D3. Weryfikacja limitów przez porównanie z kodem
Wartości limitów (1000, 2000, 50, 10, 500) są sprawdzane skryptem względem plików walidatorów.

## Risks / Trade-offs

- [Strona limitów dubluje liczby z innych stron] → liczby w jednym miejscu, inne strony linkują.
- [UI nie ogranicza długości, więc błąd pojawia się dopiero przy zapisie] → opisane wprost
  jako zachowanie wynikające z kodu.

## Migration Plan

Zmiana addytywna, tylko pliki w `wiki/`. Wycofanie: usunięcie dodanych stron i wpisów.
