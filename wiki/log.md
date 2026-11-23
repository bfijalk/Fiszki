# Wiki Log

Append-only. Never edit or remove existing entries.

Entry format: `## [YYYY-MM-DD] <operation> | <title>` where operation is one of
`bootstrap`, `ingest`, `query`, `lint`. Follow with a short body: pages created or
updated, discrepancies found, unverified claims.

Useful: `grep "^## \[" wiki/log.md | tail -5`

## [2026-11-20] bootstrap | Wiki skeleton created

Created directory layout, `index.md`, `log.md`, `glossary.md`, `SCHEMA.md` and page
templates. No feature pages yet.

## [2026-11-20] ingest | Flashcard deletion (cold start)

Created: features/flashcard-deletion, business-rules/flashcard-ownership,
architecture/application-layers, data-model/flashcard-entity,
data-model/learning-progress-entity. Updated: glossary (11 terms), index.

Verified against commit 5d1568b. Sources: FlashcardDeletion.feature, Flashcards.razor,
FlashcardService, entity configurations, unit tests. The historical .ai notes were used as a
hint only.

Discrepancies (code wins): the old API plan describes `DELETE /flashcards/{id}` and a batch
delete endpoint, but the code has no HTTP API and no batch delete; the old UI plan says
MudBlazor dialogs and server-side paging, but the Flashcards page uses a Bootstrap dialog and
loads up to 100 flashcards.

Unverified claims: none. Findings worth a follow-up: the Gherkin step "the flashcard should be
deleted" does not assert that the flashcard disappeared; the Cancel path, error handling and
counters have no automated test; a filtered empty list shows "No flashcards yet" and hides the
filter controls.
