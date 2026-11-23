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

Verified against commit 5367383. Sources: the generation Gherkin scenario, the Generate page and
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

## [2026-11-23] ingest | Manual flashcard creation (cold start)

Created: features/manual-flashcard-creation, business-rules/flashcard-content-limits. Updated:
features/ai-flashcard-generation (corrected a test-coverage statement), data-model/flashcard-entity,
glossary (5 terms), index.

Verified against commit 5367383. Sources: the manual creation and AI creation Gherkin scenarios,
the Flashcards page, FlashcardService, commands and validators, unit and validator tests, step
definitions and page object.

Correction: ai-flashcard-generation said saving through the UI was untested; FlashcardCreation.feature
does click Save Selected, but its checks pass when nothing was saved (empty or seeded collection,
no-op steps), so the conclusion stands with a more precise explanation.

Discrepancies (code wins): limits (notes say 2000 for question and answer and 40 per tag; code
1000, 2000 and 50); a learning-progress record on manual create (none is created); editing (notes
describe PUT or PATCH and an edit dialog; the service has the update, no screen); client-side
validation (notes say forms mirror the limits; the dialog does not); per-proposal accept (notes
say yes; the code accepts only in bulk). Findings worth a follow-up: the Edit button does nothing and the service update has no screen; tags are stored but
shown and filtered nowhere; the dialog shows only "Validation failed" for over-long text; limits
differ between manual create (1000, 2000), AI save (500, 500) and update; the AI-saved text is not
trimmed; no test covers lengths, the update validator or a successful update.

## [2026-11-23] ingest | Registration, login, logout and the User entity (cold start)

Created: features/user-registration, features/user-login-and-logout,
business-rules/credential-rules, data-model/user-entity, architecture/authentication-session.
Updated: architecture/application-layers, glossary (9 terms), index.

Verified against commit 5367383. Sources: the register and login Gherkin scenarios, the Register,
Login and Logout pages, the menu and routes, UserService, CustomAuthStateProvider, validators,
entity, configuration and migration, unit and validator tests, step definitions. The historical
.ai notes were used as a hint only. No passwords or test account data were recorded.
RegisterPage.cs has uncommitted changes in the repository and is not listed as a source.

Discrepancies (code wins): password policy (notes: 8 characters with upper, lower and digit; code:
3 characters and a digit); email format at registration (notes: required; code: not checked);
hashing (notes: Argon2id; code: BCrypt); tokens and HTTP endpoints for register and login (none
exist; login is kept in memory per connection); database plan (enum role type with lower-case
values, snake_case columns and database defaults; code uses text roles, PascalCase columns and no
defaults).

Unverified claims: none. Two statements depend on framework behavior and are labeled: the exact
default "not authorized" text, and how long a dropped connection keeps the login.

Findings worth a follow-up: the password-mismatch message does not block registration; an account
registered with a non-email value can never sign in; the registration page trims the password but
login does not; inactive accounts see "Invalid credentials"; roles, the active flag and the
counters have no effect or are never changed; cookie authentication is configured but unused; the
registration page logs the typed email to the server console; no scenario covers a successful
registration, logout or page protection.

## [2026-11-23] ingest | Flashcard list, counters and filters (cold start)

Created: features/flashcard-list-and-filters. Updated: features/flashcard-deletion and
features/manual-flashcard-creation (links), glossary (6 terms), index.

Verified against commit 5367383. Sources: the Flashcards page, FlashcardService list and get,
the DTO, the UI Gherkin scenarios, step definitions, page object, test-user hooks and the page's
stylesheet. The historical .ai notes were used as a hint only. No test account data was recorded.

Discrepancies (code wins): listing (notes describe paging with a page size, sorting, text search
and tag and source filters on the server; the page loads the 100 newest cards and filters by source
only, in the browser session); UI (notes describe server-side pagination with a table component; the
page uses plain Bootstrap markup); single-card fetch (exists in the service, no screen uses it).

Unverified claims: none. Two statements are labeled: the date is expected in the server's time
zone (inferred from server-side rendering), and whether the stylesheet link with the Razor Pages
tilde form loads was not checked.

Findings worth a follow-up: a filter that matches nothing hides the counters, so it cannot be
cleared without a reload; counters undercount with more than 100 cards and older cards cannot be
reached; no active scenario clicks a filter and the filter steps would assert almost nothing; the
flip scenario is switched off; the page cannot search, sort, page or show tags.

## [2026-11-23] ingest | Navigation, information pages and CI/CD (cold start)

Created: features/site-navigation, features/information-pages, architecture/ci-cd-and-startup.
Updated: features/user-login-and-logout (menu label), architecture/application-layers, glossary
(8 terms), index.

Verified against commit 5367383. Sources: the menu, layout, routes, app shell, the five information
pages, the error page, Program.cs, the CI workflow, the navigation scenario and step definitions.
The historical .ai notes were used as a hint only. The About Me page is described generically; no
personal data and no secret values were recorded.

Discrepancies (code wins): redirect to login (notes say yes; the code shows a default message);
component library (notes say MudBlazor everywhere; the menu and several pages use Bootstrap); error
handling (notes describe an error boundary and toasts; the code has neither). In-app texts versus
code, recorded as a verdict table on information-pages: spaced repetition and study sessions,
progress tracking, flashcard editing, tags, "Register in the navigation", examples from other users,
retry logic, token cost management, repository pattern, database migration, health check and rollback
in deployment, manual-run deployment, Application Insights, security scanning, "ASP.NET Core 8".

Unverified claims: none. Cloud settings named on the About Technicalities page (scaling,
certificates, private endpoint, backups, custom domain, zero-downtime) are marked not checkable
because they are not in the repository.

Findings worth a follow-up: the About page text overstates the product in many places; a leftover
"About" link in every page's top bar opens external documentation; the error page shows developer
text to end users; there is no Register link in the menu; the deployment has no migration step, health
check or rollback; the navigation has one test.

## [2026-11-23] ingest | Archive of bootstrap-llm-wiki and five document-*-in-wiki changes

Mode: after archive. Six OpenSpec changes were archived on 2026-11-23: `bootstrap-llm-wiki` (synced
five new main specs: wiki-ingest, wiki-lint, wiki-query, wiki-search, wiki-structure) and the five
`document-*-in-wiki` changes (documentation only, no delta specs).

Result: no pages needed an update. The archived changes touched only `wiki/`, `openspec/`, the wiki
skills and `CLAUDE.md`, and no page lists any of those as a source. All pages stay verified against
their existing commit; lint reports 0 errors and 0 stale pages.

Pages created: none. Pages updated: none. New discrepancies: none. Unverified claims: none.
