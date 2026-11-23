# Proposal

## Why

Wiki nie opisuje, jak użytkownik porusza się po aplikacji (menu, trasy, strony otwarte i chronione,
układ strony, strona błędu), ani co mówią strony informacyjne (główna, How to Start, Examples,
About Technicalities, About Me). Strony informacyjne zawierają dużo twierdzeń o funkcjach i
infrastrukturze, z których część nie ma pokrycia w kodzie (powtórki, edycja fiszek, ponawianie,
rollback, migracje we wdrożeniu). Analitycy i nowi programiści mogą te teksty wziąć za opis
rzeczywistości.

## What Changes

- Do `wiki/` trafiają: strona nawigacji, strona o stronach informacyjnych (z zestawieniem
  twierdzeń i stanu faktycznego) oraz strona o CI/CD i starcie aplikacji, zebrane przez
  `wiki-ingest` z `NavMenu`, `MainLayout`, `App`, `Routes`, stron informacyjnych, `Program.cs`,
  workflow CI i `Navigation.feature`.
- Poprawka etykiety pozycji menu ("About Me", nie "About") na istniejących stronach.
- Aktualizacja `application-layers`, glosariusza, indeksu i logu.
- Rozbieżności (nie tylko z `.ai/`, ale też między tekstami UI a kodem) w `Discrepancies`.

Poza zakresem: zmiany w kodzie i w tekstach stron, poprawianie nieścisłości, dane osobowe z
podstrony o autorze (strona jest opisana ogólnie), wartości sekretów CI.

## Capabilities

### New Capabilities

Brak. Zmiana dodaje treść do wiki i nie zmienia zachowania żadnej capability.

### Modified Capabilities

Brak. Zmiana ma `skip_specs: true`.

## Impact

- Nowe i zmienione pliki wyłącznie w `wiki/`.
- Brak zmian w kodzie aplikacji, projektach .NET, CI i `openspec/config.yaml`.
- Strony nie zawierają danych osobowych z podstrony o autorze ani wartości sekretów.
