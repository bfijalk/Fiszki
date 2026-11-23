# Proposal

## Why

Wiedza o tym, jak działa Fiszki, jakie reguły biznesowe za tym stoją i jak zostało to
zaimplementowane, jest rozproszona po kodzie, plikach `.feature`, historycznych
dokumentach w `.ai/` (częściowo nieaktualnych) i głowach osób w zespole. Programiści
i analitycy biznesowi nie mają jednego, wiarygodnego miejsca, które odpowiada na pytania
"jak działa funkcja X?" i "jakie reguły ją obowiązują?". Nowe zmiany będą prowadzone przez
OpenSpec, a folder `.ai/` zostanie wygaszony, więc potrzebujemy trwałego następcy,
który nie starzeje się w ukryciu.

## What Changes

- Nowy katalog `wiki/` w repozytorium: baza wiedzy o projekcie Fiszki w Markdown,
  napisana po angielsku, w całości utrzymywana przez agenta LLM (wzorzec "LLM Wiki"
  Andreja Karpathy'ego: wiedza kompilowana przyrostowo, a nie wyszukiwana od zera
  przy każdym pytaniu).
- Schema dla agenta (konwencje, szablony stron, workflow), z której korzystają zarówno
  programiści, jak i analitycy pracujący przez agentów.
- Trzy operacje agenta: **ingest** (aktualizacja wiki ze zmian w źródłach),
  **query** (odpowiadanie na pytania z cytowaniem stron) i **lint** (weryfikacja
  spójności i aktualności wiki względem kodu).
- Mechanizm wyszukiwania zaprojektowany od nowa, wywoływalny przez agenta.
- Jednorazowy "cold start": pierwszy przebieg ingestu z kodu na wybranej funkcjonalności
  jako dowód działania end-to-end.
- Hierarchia zaufania do źródeł: kod i `.feature` ponad `openspec/specs` ponad `.ai/`.
  `.ai/` traktowane wyłącznie jako podpowiedź o niskiej pewności.
- Ingest do wiki jako ostatni krok archiwizacji zmiany OpenSpec, wpisany do
  `openspec/config.yaml` (`operations.archive.guidance`), aby nie trzeba było o nim
  pamiętać przy każdej zmianie.
- Folder `.ai/` jest wygaszany: po przeniesieniu potrzebnej wiedzy do wiki i OpenSpec
  można go usunąć (krok następczy, po przeglądzie jego zawartości).

Poza zakresem: zmiany w kodzie aplikacji, pełne pokrycie wszystkich funkcjonalności
(to kolejne zmiany), interfejs graficzny dla czytelników bez agenta, ingest uruchamiany
z CI.

## Capabilities

### New Capabilities
- `wiki-structure`: układ katalogów `wiki/`, konwencje stron (frontmatter, linkowanie,
  język angielski), szablon strony funkcjonalności, `index.md` i `log.md`, hierarchia
  zaufania do źródeł.
- `wiki-ingest`: przyrostowa aktualizacja wiki ze zmian w źródłach (kod, `.feature`,
  `openspec/specs`), wykrywanie sprzeczności, obowiązkowa aktualizacja indeksu i logu.
- `wiki-query`: odpowiadanie na pytania programistów i analityków z cytowaniem stron
  wiki, jawne wskazywanie luk, opcjonalny zapis syntez.
- `wiki-lint`: weryfikacja wiki, w tym sprawdzanie odnośników do kodu, martwych linków,
  stron osieroconych i dryfu względem źródeł.
- `wiki-search`: wyszukiwanie stron wiki wywoływalne przez agenta, z rankingiem
  i fragmentami wyników.

### Modified Capabilities

Brak. `openspec/specs/` jest obecnie puste.

## Impact

- Nowe pliki: `wiki/**`, plik schemy dla agenta, skrypt(y) wyszukiwania i lintu.
- Brak zmian w kodzie aplikacji, projektach .NET ani w CI.
- Zależności: narzędzia wyszukiwania opisane w `design.md` (lokalne, bez usług
  zewnętrznych).
- Konfiguracja: `openspec/config.yaml` dostaje wskazówkę dla operacji archiwizacji
  (ingest do wiki jako ostatni krok). Dotyczy każdej przyszłej zmiany OpenSpec.
- Proces: po archiwizacji zmiany wiki jest odświeżana automatycznie przez agenta, a
  nie z pamięci zespołu.
- `.ai/`: po wygaszeniu do usunięcia. Uwaga: zawiera niezacommitowane zmiany, które
  trzeba świadomie przejrzeć przed usunięciem.
