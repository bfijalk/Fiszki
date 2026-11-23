# wiki-ingest Specification

## Purpose

Definiuje, jak agent aktualizuje wiki, gdy zmieniają się źródła (kod, scenariusze,
specyfikacje OpenSpec), tak aby wiedza była kumulowana i nie dryfowała względem kodu.

## Requirements

### Requirement: Ingest is driven by source changes
An ingest SHALL be started either for a named feature (cold start) or for the changes in
sources since the `verified_commit` of the affected pages. It SHALL identify affected
pages from each page's `sources` list.

#### Scenario: Incremental ingest after a code change
- **WHEN** files listed in a page's `sources` changed after its `verified_commit`
- **THEN** the ingest reviews that page against the current sources
- **AND** pages whose sources did not change are left untouched

### Requirement: Ingest updates all affected pages
An ingest SHALL update every page affected by the change: feature pages, business rules,
architecture and data-model pages and the glossary. It SHALL add to existing knowledge
instead of rewriting pages from scratch.

#### Scenario: One change affects several pages
- **WHEN** a validation rule changes
- **THEN** the feature page, the business-rules page and any data-model page that cites it are all updated

### Requirement: Contradictions are surfaced
When new source content contradicts existing wiki content, the ingest SHALL NOT overwrite
silently. It SHALL record both claims, their sources and the date in a `Discrepancies`
section, or replace the claim only when the source hierarchy clearly decides it.

#### Scenario: Code contradicts an existing claim
- **WHEN** code no longer matches a documented rule
- **THEN** the page is corrected to the code's behavior
- **AND** the change is noted in the log entry

### Requirement: Indexing is part of ingest
Every ingest SHALL, before finishing, update `index.md` for new, renamed or re-described
pages, refresh `summary`, `tags` and `verified_commit` on every reviewed page, and append
a log entry.

#### Scenario: New feature page
- **WHEN** an ingest creates a page
- **THEN** `index.md` links to it with a one-line description
- **AND** the page's `verified_commit` equals the commit the ingest ran against

### Requirement: Ingest follows archiving of a change
Archiving an OpenSpec change SHALL end with an ingest of the pages affected by that
change. The ingest SHALL read the updated `openspec/specs/` and the current code, and
SHALL run as the last step, after specs are synced and the change is moved to the archive.

#### Scenario: Change archived
- **WHEN** an OpenSpec change is archived
- **THEN** an incremental ingest runs for pages whose `sources` were affected
- **AND** the archive outcome reports the pages created or updated

#### Scenario: Change touches nothing documented
- **WHEN** an archived change affects no source listed in any page
- **THEN** the ingest reports that no pages needed an update and appends a log entry

### Requirement: Ingest reports its result
An ingest SHALL end with a report listing created pages, updated pages, discrepancies
found and claims that could not be verified.

#### Scenario: Unverifiable claim
- **WHEN** a claim from `.ai/` cannot be confirmed in code
- **THEN** it appears in the report and is marked `confidence: low` on the page
