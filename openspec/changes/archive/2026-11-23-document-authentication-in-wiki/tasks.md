# Tasks

## 1. Zakres

- [x] 1.1 Wylistować pliki rejestracji, logowania, wylogowania, stanu uwierzytelnienia i encji użytkownika i porównać z `design.md` (Context); zweryfikować, że nic istotnego nie pominięto

## 2. Ingest

- [x] 2.1 Wywołać skill `wiki-ingest` w trybie cold start dla "user registration, login, logout and the User entity"; zweryfikować, że powstały strony: rejestracja, logowanie i wylogowanie (z sekcjami `Business rules` i `Implementation`), reguły danych logowania, encja użytkownika i architektura stanu uwierzytelnienia
- [x] 2.2 Zweryfikować aktualizację `application-layers`, `index.md`, `glossary.md`, `log.md` oraz `verified_commit` na nowych i zmienionych stronach

## 3. Weryfikacja

- [x] 3.1 Uruchomić `wiki/tools/lint` i zweryfikować 0 błędów
- [x] 3.2 Sprawdzić skryptem reguły walidatorów (e-mail, hasło) i co najmniej 5 innych twierdzeń względem kodu, w tym twierdzenia negatywne (brak resetu hasła, nieużywane ciasteczka, role bez skutku)
- [x] 3.3 Zadać przez `wiki-query` pytanie biznesowe i pytanie o implementację; zweryfikować, że odpowiedzi cytują nowe strony
- [x] 3.4 Zweryfikować, że w wiki nie ma haseł ani danych kont testowych oraz że zmiany dotyczą tylko `wiki/` i `openspec/changes/document-authentication-in-wiki/`
