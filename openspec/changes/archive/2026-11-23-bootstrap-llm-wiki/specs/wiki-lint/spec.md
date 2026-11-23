# Spec Delta

## Purpose

Definiuje weryfikację zdrowia wiki, w szczególności wykrywanie dryfu względem kodu,
tak aby wiki pozostawała wiarygodnym źródłem wiedzy o projekcie.

## ADDED Requirements

### Requirement: Structural checks
Lint SHALL detect pages missing from `index.md`, dead `[[links]]`, orphan pages with no
inbound links and pages with missing or invalid frontmatter.

#### Scenario: Orphan page
- **WHEN** a page has no inbound links and is not an entry point
- **THEN** lint reports it as orphaned

### Requirement: Code reference verification
Lint SHALL verify that every cited file path exists and every cited symbol is still
present in that file, and SHALL report those that are not.

#### Scenario: Renamed class
- **WHEN** a page cites a class that was renamed
- **THEN** lint reports the broken reference with the page and the cited symbol

### Requirement: Drift detection
Lint SHALL compare each page's `verified_commit` with changes to its `sources` and
report pages whose sources changed afterwards.

#### Scenario: Source changed after verification
- **WHEN** a file in a page's `sources` changed after `verified_commit`
- **THEN** lint reports the page as stale with the changed files

### Requirement: Content checks
Lint SHALL report claims with no cited source, unresolved `Discrepancies` sections and
concepts mentioned on several pages without their own page.

#### Scenario: Claim without a source
- **WHEN** a page states a business rule without a citation
- **THEN** lint reports it

### Requirement: Lint is read-only
Lint SHALL produce a report and SHALL NOT change pages. Fixes SHALL be applied only
after the user accepts them, as an ingest.

#### Scenario: Lint run
- **WHEN** lint completes
- **THEN** the working tree has no changes except an appended `log.md` entry
- **AND** the report lists findings and proposed fixes
