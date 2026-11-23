# Design

## Context

Cała logika listy jest w `Components/Pages/Flashcards.razor` (stany ładowania, błędu, pusty, lista,
liczniki, filtry po źródle utworzenia, widok kartowy i listowy, odwracanie kart) oraz w
`FlashcardService.ListAsync` (do 500, domyślnie 100 najnowszych, malejąco po czasie utworzenia).
Scenariusze: `FlashcardsUI.feature` (7, w tym jeden `@ignore`). Dane dla scenariuszy dostarczają
hooki przypisujące zasiane konta po tagach; ich dane logowania nie są opisywane w wiki.
Motywacja w `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Jedna strona opisująca, co użytkownik widzi i może zrobić na liście, z regułami i ograniczeniami.
- Jawne rozdzielenie tego, co testuje scenariusz, od tego, co wynika tylko z kodu (filtry nie są
  testowane przez żaden aktywny scenariusz).

**Non-Goals:**
- Powtarzanie treści stron usuwania i tworzenia ręcznego (tylko linki).
- Dane kont testowych.

## Decisions

### D1. Jedna strona funkcji dla całej strony Flashcards
Lista, liczniki, filtry, widoki i odwracanie to jedna całość w jednym komponencie i na jednym
ekranie, więc jedna strona funkcji, a nie pięć. Usuwanie i tworzenie mają własne strony, do których
ta linkuje. *Alternatywa:* osobne strony dla filtrów i widoków. Odrzucone: dużo odsyłaczy, mało treści.

### D2. Zachowania "tylko z kodu" jawnie oznaczone
Filtry, liczniki, kolejność nowej karty i strefa czasu wyświetlania nie mają testu, więc każda reguła
ma oznaczenie sposobu potwierdzenia.

### D3. Bez danych kont testowych
Strona opisuje mechanizm przypisywania zasianych kont ogólnie ("konto z pustą kolekcją", "konto z
przykładowymi fiszkami"), bez adresów e-mail i haseł.

## Risks / Trade-offs

- [Zachowanie czasu lokalnego zależy od hostingu] → opisane jako wniosek z faktu renderowania po
  stronie serwera.
- [Strona się dubluje z fragmentami stron usuwania i tworzenia] → te strony linkują do nowej i
  zachowują tylko własne reguły.

## Migration Plan

Zmiana addytywna, tylko pliki w `wiki/`. Wycofanie: usunięcie dodanej strony i wpisów.
