# Design

## Context

Menu (`NavMenu.razor`) i układ (`MainLayout.razor`) są wspólne dla wszystkich stron; trasy
definiują dyrektywy `@page`. Strony informacyjne to statyczna treść (łącznie ok. 1500 linii
znaczników), z jedną ciekawostką: strona Examples pokazuje zakodowane na stałe przykładowe fiszki.
CI to jeden workflow (`.github/workflows/ci-cd.yml`), a start aplikacji i inicjalizacja bazy są w
`Program.cs`. Scenariusz `Navigation.feature` ma jeden krok. Motywacja w `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Odpowiedzi na pytania "co jest w menu, kto co widzi, co mówią strony informacyjne".
- Zestawienie "twierdzenie na stronie vs stan w kodzie", sprawdzone wyszukiwaniem w kodzie.
- Opis CI/CD i startu aplikacji w zakresie potrzebnym do oceny twierdzeń o wdrożeniu.

**Non-Goals:**
- Przepisywanie treści stron informacyjnych (wiki streszcza, nie kopiuje).
- Dane osobowe autora (strona About Me opisana ogólnie).
- Naprawa nieścisłości.

## Decisions

### D1. Trzy strony: nawigacja, strony informacyjne, CI/CD i start
Nawigacja dotyczy zachowania, strony informacyjne treści, a CI/CD architektury; łączenie ich
utrudniłoby odpowiedzi analitykom i programistom. *Alternatywa:* jedna długa strona. Odrzucone.

### D2. Twierdzenia stron informacyjnych weryfikowane względem kodu
Każde sprawdzalne twierdzenie dostaje werdykt: zgodne, niezgodne albo niesprawdzalne w repo
(np. skalowanie w Azure). Niezgodne trafiają do `Discrepancies`.

### D3. Rozbieżności nie tylko z `.ai/`
Hierarchia zaufania (kod ponad dokumentami) obejmuje też teksty w samej aplikacji, więc
`Discrepancies` opisuje rozjazdy tekst-UI a kod.

### D4. Bez danych osobowych i sekretów
Strona o autorze jest opisana jako profil twórcy aplikacji, bez nazwisk, stanowisk i odnośników.
Z CI zapisywane są tylko nazwy rodzajów sekretów, nigdy wartości.

## Risks / Trade-offs

- [Teksty informacyjne się zmieniają] → strony informacyjne są źródłami stron wiki, więc zmiana
  będzie wykryta jako dryf.
- [Niesprawdzalne twierdzenia o infrastrukturze] → oznaczone jako niesprawdzalne w repo.

## Migration Plan

Zmiana addytywna, tylko pliki w `wiki/`. Wycofanie: usunięcie dodanych stron i wpisów.
