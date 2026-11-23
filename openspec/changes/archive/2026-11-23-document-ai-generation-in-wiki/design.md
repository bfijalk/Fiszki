# Design

## Context

Wiki działa i ma jeden udokumentowany obszar (usuwanie fiszek). Zasady ingestu są w
`wiki/SCHEMA.md`. Źródła tego obszaru, wg stanu repo: scenariusz
`Fiszki.FunctionalTests/Features/FlashcardGeneration.feature`; UI w
`Components/Pages/GeneratePage.razor` (+ `.razor.cs`) i `Components/Pages/Generate/`;
`Fiszki.Services/Services/GenerationService.cs`; klient modelu w
`Fiszki.Services/Services/OpenRouter/`; polecenia i walidatory `StartGenerationCommand` i
`SaveProposalsCommand`; modele w `Fiszki.Services/Models/Generation` i `Models/OpenRouter`;
wyjątki generowania; testy `Fiszki.Tests/Services/GenerationServiceTests.cs`; konfiguracja
`OpenRouter` w `appsettings.json`. Motywacja w `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Odpowiedzi na pytania "jak działa generowanie, jakie reguły, jak jest zaimplementowane".
- Każde twierdzenie z odnośnikiem do pliku i symbolu, sprawdzone lintem.
- Jawne rozdzielenie tego, co pokrywa test, od tego, co wynika tylko z kodu.

**Non-Goals:**
- Zachowanie żywego modelu: wiki opisuje to, co robi kod, a nie to, co zwraca zewnętrzny
  serwis.
- Naprawianie znalezionych niespójności.

## Decisions

### D1. Integracja z OpenRouter jako osobna strona architektury
Klient OpenRouter (opcje, limity współbieżności, wyjątki) jest warstwą, którą może
wykorzystać także inna funkcja. Strona funkcji opisuje zachowanie dla użytkownika, strona
architektury opisuje klienta. *Alternatywa:* wszystko na stronie funkcji. Odrzucone:
miesza reguły biznesowe z techniką i utrudnia odpowiedzi analitykom.

### D2. Reguły biznesowe z walidatorów i UI wprost
Limity i warunki (np. długość tekstu źródłowego, liczba propozycji) pochodzą z
`GenerationValidation.cs`, walidatorów i serwisu. Wartości są przepisywane z kodu z
odnośnikiem, a nie z notatek `.ai/`.

### D3. Konfiguracja bez wartości sekretów
Strona wymienia klucze konfiguracji `OpenRouter` i ich znaczenie, ale nigdy wartości klucza
API. W repo `ApiKey` jest pusty, a wartość pochodzi ze środowiska.

### D4. Pewność twierdzeń
Twierdzenia potwierdzone testem lub scenariuszem mają `confidence: high`. Zachowania
wynikające wyłącznie z czytania kodu, a niesprawdzone testem, są jawnie oznaczone w treści
("code only").

## Risks / Trade-offs

- [Kod odwołuje się do zachowania zewnętrznego modelu, którego nie da się sprawdzić] →
  wiki opisuje kontrakt i obsługę błędów w kodzie, a nie odpowiedzi modelu.
- [Strona generowania może urosnąć za bardzo] → podział na stronę funkcji i stronę
  integracji, w razie potrzeby osobna strona reguł walidacji.
- [Notatki `.ai/` rozjeżdżają się z kodem] → rozbieżności w `Discrepancies`.

## Migration Plan

Zmiana addytywna, tylko pliki w `wiki/`. Wycofanie to usunięcie dodanych stron i wpisów w
indeksie, glosariuszu i logu.
