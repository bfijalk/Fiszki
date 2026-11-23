# wiki-structure Specification

## Purpose

Definiuje układ, konwencje i zasady zaufania do źródeł dla wiki o projekcie Fiszki,
tak aby każdy agent mógł ją budować i czytać w spójny, przewidywalny sposób.

## Requirements

### Requirement: Layout of the wiki
The repository SHALL contain a `wiki/` directory with `index.md`, `log.md`, a schema file
describing the conventions, and page directories `features/`, `business-rules/`,
`architecture/`, `data-model/` and a `glossary.md`.

#### Scenario: Fresh bootstrap
- **WHEN** the wiki has been bootstrapped
- **THEN** `wiki/index.md`, `wiki/log.md` and the schema file exist
- **AND** every page directory exists and is referenced from `index.md`

### Requirement: Wiki is written in English and owned by the agent
All wiki pages SHALL be written in English. Pages SHALL be created and edited by the
agent; humans SHALL NOT be required to edit them manually.

#### Scenario: Page language
- **WHEN** an agent creates or updates a page
- **THEN** its content is in English, including when the triggering question was in another language

### Requirement: Source files are never modified by the wiki
The wiki SHALL treat application code, `.feature` files, `openspec/` and `.ai/` as
read-only sources. No wiki operation SHALL modify them.

#### Scenario: Ingest touches only the wiki
- **WHEN** an ingest completes
- **THEN** the only changed files are under `wiki/`

### Requirement: Source trust hierarchy
The wiki SHALL resolve conflicts between sources in this order: application code and
`.feature` files, then `openspec/specs/`, then `.ai/`. Claims that originate only from
`.ai/` SHALL be marked `confidence: low` unless verified against code.

#### Scenario: Conflict between code and a historical document
- **WHEN** `.ai/` states a rule that the code contradicts
- **THEN** the page states the behavior shown by the code
- **AND** records the discrepancy in a `Discrepancies` section citing both sources

### Requirement: Page frontmatter
Every wiki page except `index.md` and `log.md` SHALL have YAML frontmatter with `title`,
`type`, `summary`, `tags`, `sources`, `verified_commit` and `confidence`.

#### Scenario: Valid page
- **WHEN** a page is created
- **THEN** all required frontmatter fields are present
- **AND** `sources` lists repository paths and `verified_commit` is a commit SHA

### Requirement: Feature page template
A page describing a user-visible feature SHALL separate `Business rules` from
`Implementation`, so analysts and developers can each find their part. Business rules
SHALL cite a `.feature` scenario or an OpenSpec requirement when one exists.

#### Scenario: Feature with Gherkin coverage
- **WHEN** a feature has `.feature` scenarios
- **THEN** its `Business rules` section references those scenarios
- **AND** its `Implementation` section lists the relevant components, services and entities

### Requirement: Code references are stable
References to code SHALL use file path plus symbol name. Line numbers MAY be added but
SHALL NOT be the only locator.

#### Scenario: Reference after a refactor shifts lines
- **WHEN** lines in a referenced file move but the symbol still exists
- **THEN** the reference remains valid

### Requirement: Index and log
`index.md` SHALL list every page with a link, a one-line description and its category.
`log.md` SHALL be append-only, with entries starting `## [YYYY-MM-DD] <operation> | <title>`.
An entry SHALL be appended after bootstrap, ingest, lint and saving a synthesis. Answering a
question without saving anything SHALL NOT require an entry.

#### Scenario: Log entry format
- **WHEN** a bootstrap, an ingest, a lint or the saving of a synthesis completes
- **THEN** a new entry is appended to `log.md` in the required format
- **AND** existing entries are unchanged

#### Scenario: Plain question
- **WHEN** a question is answered and nothing is saved
- **THEN** `log.md` is not changed
