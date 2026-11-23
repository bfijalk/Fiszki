# Proposal

## Why

Po dokumentacji generowania AI wiki nadal nie opisuje drugiej ścieżki tworzenia fiszek:
ręcznego dodawania z okna na stronie Flashcards, ani reguł walidacji treści fiszek. Te reguły
są rozproszone i niespójne (inne limity przy ręcznym tworzeniu niż przy zapisie fiszek AI),
a analitycy pytają o nie przy każdej zmianie wymagań.

## What Changes

- Do `wiki/` trafia wiedza o ręcznym tworzeniu fiszek, zebrana przez `wiki-ingest` z
  `ManualFlashcardCreation.feature`, `FlashcardCreation.feature`, okna tworzenia w
  `Flashcards.razor`, `FlashcardService`, poleceń i walidatorów oraz testów.
- Powstaje strona funkcji (ręczne tworzenie) oraz strona reguł `flashcard-content-limits`
  zbierająca limity treści fiszek ze wszystkich ścieżek (ręczna, AI, aktualizacja).
- Poprawka istniejącej strony `ai-flashcard-generation`: scenariusz `FlashcardCreation.feature`
  pokrywa zapis wygenerowanych fiszek przez UI, więc zdanie o braku takiego testu jest nieprawdziwe.
- Aktualizacja `wiki/index.md`, `wiki/glossary.md`, `wiki/log.md` oraz stron dotkniętych zmianą.

Poza zakresem: zmiany w kodzie, edycja fiszek w UI (nie istnieje), rejestracja i logowanie
(osobna zmiana).

## Capabilities

### New Capabilities

Brak. Zmiana dodaje treść do wiki i nie zmienia zachowania żadnej capability.

### Modified Capabilities

Brak. Zmiana ma `skip_specs: true`.

## Impact

- Nowe i zmienione pliki wyłącznie w `wiki/`.
- Brak zmian w kodzie aplikacji, projektach .NET, CI i `openspec/config.yaml`.
