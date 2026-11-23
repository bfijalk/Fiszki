# Proposal

## Why

Rejestracja, logowanie i wylogowanie to podstawa każdej funkcji Fiszek (wszystkie główne strony
wymagają zalogowania), a wiki nie opisuje ich wcale. Analitycy pytają o politykę haseł, o to, co
dzieje się po rejestracji i jak długo trwa logowanie. Odpowiedzi nie są oczywiste: kod odbiega od
historycznych notatek (tokeny JWT, polityka haseł, algorytm hashowania), a stan zalogowania żyje
tylko w pamięci połączenia.

## What Changes

- Do `wiki/` trafia wiedza o rejestracji, logowaniu i wylogowaniu oraz encji `User`, zebrana przez
  `wiki-ingest` z `Register.feature`, `Login.feature`, stron `Register`, `Login`, `Logout`, menu,
  `UserService`, `CustomAuthStateProvider`, walidatorów, encji i testów.
- Powstają strony: rejestracja, logowanie i wylogowanie, reguły danych logowania (e-mail i hasło),
  encja użytkownika, oraz architektura stanu uwierzytelnienia.
- Aktualizacja `application-layers`, glosariusza, indeksu i logu.
- Rozbieżności z `.ai/` w `Discrepancies`; kod wygrywa.

Poza zakresem: zmiany w kodzie, naprawa znalezionych problemów (np. rejestracja mimo niezgodnych
haseł), dokumentowanie listy i filtrów fiszek oraz nawigacji (osobne zmiany).

## Capabilities

### New Capabilities

Brak. Zmiana dodaje treść do wiki i nie zmienia zachowania żadnej capability.

### Modified Capabilities

Brak. Zmiana ma `skip_specs: true`.

## Impact

- Nowe i zmienione pliki wyłącznie w `wiki/`.
- Brak zmian w kodzie aplikacji, projektach .NET, CI i `openspec/config.yaml`.
- Strony nie zawierają haseł ani wartości sekretów; wzmianki o danych testowych tylko ogólnie.
