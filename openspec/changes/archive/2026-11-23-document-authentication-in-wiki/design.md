# Design

## Context

Uwierzytelnianie nie używa ciasteczek ani tokenów: `CustomAuthStateProvider` trzyma zalogowanego
użytkownika w polu obiektu zarejestrowanego jako scoped, więc stan trwa tyle co połączenie
(obwód) Blazor Server. Konfiguracja ciasteczek istnieje w `Program.cs`, ale nic z niej nie
korzysta. Strony: `Login.razor` (MudBlazor), `Register.razor` (zwykły HTML), `Logout.razor`, menu
w `NavMenu.razor`. Serwis: `UserService` (BCrypt, kolejność kontroli, komunikaty). Scenariusze:
`Login.feature` (2) i `Register.feature` (1). Motywacja w `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Odpowiedzi na pytania o politykę e-maila i hasła, przebieg rejestracji i logowania, trwałość
  sesji i dostęp do stron.
- Jawne wskazanie niespójności (inne reguły e-maila przy rejestracji i logowaniu).

**Non-Goals:**
- Naprawa niespójności.
- Dokumentowanie funkcji, których nie ma w kodzie, poza zapisaniem ich braku.

## Decisions

### D1. Osobna strona reguł danych logowania
Reguły e-maila i hasła (rejestracja vs logowanie) są wspólne i niespójne, więc trafiają do
`business-rules/credential-rules`, a strony funkcji na nią linkują. *Alternatywa:* reguły na
stronach funkcji. Odrzucone: rozjazd między rejestracją a logowaniem widać dopiero obok siebie.

### D2. Architektura stanu uwierzytelnienia osobno od funkcji
Zachowanie sesji (czas życia, brak ciasteczka, domyślna reakcja na brak autoryzacji, role)
dotyczy wszystkich stron, więc jest stroną `architecture/`, a nie częścią rejestracji lub logowania.

### D3. Brak funkcji zapisywany jako fakt z kodu
"Brak resetu hasła, weryfikacji e-maila, blokady konta" jest zapisany jako obserwacja kodu i
sprawdzony wyszukiwaniem, a nie jako zalecenie.

### D4. Bez haseł i sekretów
Strony opisują reguły, nie wartości kont testowych ani haseł.

## Risks / Trade-offs

- [Zachowanie zależne od domyślnych komunikatów frameworka (np. "Not authorized")] → oznaczone
  jako wynik czytania kodu i domyślnego zachowania frameworka.
- [Rozbieżności z `.ai/` są liczne] → osobna sekcja `Discrepancies` na każdej stronie, której dotyczą.

## Migration Plan

Zmiana addytywna, tylko pliki w `wiki/`. Wycofanie: usunięcie dodanych stron i wpisów.
