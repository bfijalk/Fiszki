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

## [2026-11-23] ingest | AI flashcard generation (cold start)

Created: features/ai-flashcard-generation, architecture/openrouter-integration,
architecture/generation-job-flow. Updated: data-model/flashcard-entity,
business-rules/flashcard-ownership, architecture/application-layers, glossary (8 terms), index.

Verified against commit 52f627a. Sources: the generation Gherkin scenario, the Generate page and
its components, GenerationService, the OpenRouter client, validators, unit tests, CI workflow and
settings files. The historical .ai notes were used as a hint only.

Discrepancies (code wins): source text limits (notes say 1000 to 10000, code 50 to 5000); AI
failure (notes say an error is shown, code falls back to placeholder cards and reports success);
user counters (notes say they are updated, no code does it); source text in logs (notes say it is
excluded, code logs it); a generation HTTP API (none exists); OpenRouter client scope (notes
describe streaming, retries and a rate limiter, code has none of them); the About page's "retry
logic"; README's OpenRouter__BaseUrl (code reads BaseUri).

Unverified claims: none. Two statements are inferences and are labeled as such: the Generate page
cannot create its service without an API key, and a 2000-token cap may cut off large requests.
TestConstants.cs has uncommitted changes in the repository but not in the cited constant, so it
is not listed as a source.

Findings worth a follow-up: no accept action for a single proposal; Example and Notes are not
saved; the "leave without saving" warning is not wired; page cancel does not cancel the job; the
page uses none of the job's error text; SemaphoreRateLimiter and four options are unused; no test
covers saving through the UI, the prompt or the real OpenRouter client.
