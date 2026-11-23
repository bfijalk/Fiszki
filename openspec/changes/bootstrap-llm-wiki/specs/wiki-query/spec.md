# Spec Delta

## Purpose

Definiuje, jak agent odpowiada programistom i analitykom biznesowym na pytania o
działanie, implementację i reguły biznesowe Fiszek, korzystając wyłącznie z wiki.

## ADDED Requirements

### Requirement: Answers come from the wiki and cite it
A query SHALL be answered from wiki pages and SHALL cite the pages used. It SHALL NOT
present knowledge from outside the wiki as wiki-backed.

#### Scenario: Question covered by the wiki
- **WHEN** a user asks how a feature works
- **THEN** the answer cites the wiki pages it is based on

### Requirement: Gaps are stated explicitly
When the wiki does not cover the question, the agent SHALL say so and name the gap
instead of guessing. It MAY then suggest an ingest.

#### Scenario: Undocumented feature
- **WHEN** no page covers the asked feature
- **THEN** the answer states that the wiki has no coverage
- **AND** proposes ingesting the relevant source

### Requirement: Answers are tailored to the asker
The agent SHALL answer in the language of the question. For business questions it SHALL
lead with `Business rules`. For implementation questions it SHALL lead with
`Implementation`, including code references.

#### Scenario: Analyst asks in Polish
- **WHEN** an analyst asks in Polish about a rule
- **THEN** the answer is in Polish and based on English wiki pages
- **AND** quotes of rules reference the cited scenario or requirement

### Requirement: Staleness is disclosed
When a cited page's sources changed after its `verified_commit`, the answer SHALL warn
that the page may be outdated.

#### Scenario: Page behind the code
- **WHEN** a source of a cited page changed since `verified_commit`
- **THEN** the answer includes a staleness warning naming the page

### Requirement: Valuable answers can be saved
The agent SHALL offer to save an answer with lasting value as a synthesis page, and SHALL
create it, update `index.md` and append to `log.md` only after the user agrees.

#### Scenario: User accepts
- **WHEN** the user agrees to save a comparison
- **THEN** a synthesis page is created with frontmatter and the index and log are updated
