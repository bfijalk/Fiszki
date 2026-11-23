# Spec Delta

## Purpose

Definiuje wyszukiwanie stron wiki wywoływalne przez agenta, aby agent szybko znajdował
istotne strony bez czytania całej wiki i bez zależności od usług zewnętrznych.

## ADDED Requirements

### Requirement: Search is invocable by an agent
The wiki SHALL provide a local command that takes a text query and returns ranked
matching pages. It SHALL work offline and SHALL NOT require an external service.

#### Scenario: Agent searches for a feature
- **WHEN** an agent runs the search with a query
- **THEN** it receives a ranked list of pages

### Requirement: Result contents
Each result SHALL include the page path, title, type, summary and a snippet showing the
match. The command SHALL support machine-readable output.

#### Scenario: Machine-readable output
- **WHEN** the command is run with the JSON option
- **THEN** each result contains path, title, type, summary, snippet and score

### Requirement: Filtering
The command SHALL allow filtering by page `type` and `tags`, and limiting the number of
results.

#### Scenario: Filter by type
- **WHEN** the query is restricted to type `feature`
- **THEN** only feature pages are returned

### Requirement: Ranking uses structure
Matches in `title`, `tags` and `summary` SHALL rank above matches in body text.

#### Scenario: Title match wins
- **WHEN** one page has the term in its title and another only in its body
- **THEN** the title match ranks first

### Requirement: Contract is independent of the ranking method
The command's interface and output format SHALL remain unchanged if the ranking method
is replaced, so a semantic ranking can be added later without changing the agent
instructions.

#### Scenario: Swapping the ranker
- **WHEN** the ranking implementation changes
- **THEN** existing invocations and result fields continue to work

### Requirement: No match is explicit
When nothing matches, the command SHALL return an empty result and a non-error exit,
and SHALL NOT return unrelated pages.

#### Scenario: Unknown term
- **WHEN** the query matches no page
- **THEN** the output is empty and the exit status indicates success
