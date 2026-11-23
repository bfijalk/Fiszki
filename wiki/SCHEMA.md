# Wiki Schema

This file is the contract for everyone who builds or reads this wiki. It is written for
the agent that maintains the wiki and for the agents that answer developers' and
business analysts' questions with it.

The wiki documents the **Fiszki** project: how each feature works, which business rules
govern it, and how it is implemented. It is compiled knowledge: the agent reads sources
once, writes pages, and keeps them current. Questions are answered from the pages, not
from the raw code.

## 1. Principles

1. **Language.** All pages are written in **English**, even when the question that
   triggered the work was in another language.
2. **Agent-owned.** Pages are created and edited by the agent. Humans are not required to
   edit them.
3. **Sources are read-only.** Application code, `.feature` files, `openspec/` and `.ai/`
   are never modified by any wiki operation. A wiki operation changes files under
   `wiki/` only (plus the log entry, which is also under `wiki/`).
4. **Every claim is verifiable.** A statement about behavior cites a source (see
   section 5). A claim without a citation is a defect.
5. **Add, do not rewrite.** Ingests extend and correct existing pages. They do not
   regenerate pages from scratch.

## 2. Sources and trust hierarchy

Paths treated as sources:

| Path | What it holds |
|---|---|
| `Components/` | Blazor UI: pages, layout, components |
| `Fiszki.Services/` | Business logic: services, commands, validation, exceptions, mapping |
| `Fiszki.Database/` | Entities, EF Core configuration, migrations |
| `Fiszki.FunctionalTests/Features/*.feature` | Gherkin scenarios: business rules stated as behavior |
| `Fiszki.Tests/` | Unit tests: evidence of intended service behavior |
| `Program.cs`, `appsettings*.json` | Wiring and configuration |
| `openspec/specs/` | Capability requirements (WHEN/THEN), filled as changes are archived |
| `.ai/` | Historical plans. **Hint only**, being phased out |

When sources disagree, the higher level wins:

1. Application code and `.feature` files (actual behavior)
2. `openspec/specs/` (intended requirements)
3. `.ai/` (historical; may be outdated)

Rules:
- A claim that comes **only** from `.ai/` gets `confidence: low` unless it is verified
  against code.
- When a lower-level source contradicts a higher one, the page states the higher-level
  behavior and records the difference in a `## Discrepancies` section that cites both
  sources.
- Pages must not list `.ai/` paths in `sources`, because that folder will be deleted.
  Mention an `.ai/` origin in the discrepancy or in the ingest report instead.

## 3. Layout

```
wiki/
  index.md        catalog of all pages (read first)
  log.md          append-only operation log
  SCHEMA.md       this file
  glossary.md     domain terms and synonyms (English + Polish)
  features/       one page per user-visible feature
  business-rules/ rules shared by several features
  architecture/   layers, components, integrations
  data-model/     entities, relations, constraints
  synthesis/      saved answers and analyses
  templates/      page templates (copy, do not link)
  tools/          search and lint commands
```

Page file names are kebab-case ASCII, for example `flashcard-deletion.md`. Page links use
`[[page-name]]` (file name without `.md`).

## 4. Page frontmatter

Every page except `index.md` and `log.md` starts with YAML frontmatter:

```yaml
---
title: Flashcard deletion
type: feature            # feature | business-rule | architecture | data-model | synthesis | glossary
summary: One sentence that says what the page covers and when to read it.
tags: [flashcards, deletion]
sources:                 # repository paths this page is derived from
  - Fiszki.FunctionalTests/Features/FlashcardDeletion.feature
  - Fiszki.Services/Services/FlashcardService.cs
verified_commit: <40-char SHA the page was last verified against>
confidence: high         # high | medium | low
---
```

Field rules:
- `summary` and `tags` feed search. Keep the summary to one sentence. Put synonyms in
  `tags` and in `glossary.md`.
- `sources` lists every file whose change could make the page wrong. It drives drift
  detection.
- `verified_commit` is the commit the agent verified the page against. Update it on every
  page an ingest reviews, even when the page text did not change.
- `confidence` is `low` when any key claim is unverified or comes only from `.ai/`.

## 5. Referencing code and requirements

- **Code:** cite file path **and symbol**, for example
  `Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync`. A line number may be
  added in parentheses but is never the only locator, because lines drift.
- **Business rules:** cite the `.feature` scenario by file and scenario name, or the
  OpenSpec requirement as `openspec/specs/<capability>/spec.md` and requirement name,
  whenever one exists.

## 6. Feature page template

A feature page keeps what analysts need apart from what developers need:

```
# <Feature>
## Summary
## Business rules        <- for analysts; cite .feature scenarios / OpenSpec requirements
## User flow
## Implementation        <- for developers; components, services, entities, with code references
## Dependencies and side effects
## Discrepancies         <- only when sources disagree
## Related               <- [[links]] to other pages
```

Full templates are in `wiki/templates/`.

## 7. Index and log

- `index.md` lists **every** page as `- [[page-name]] - one-line description` under its
  category. Update it whenever a page is created, renamed or re-described.
- `log.md` is append-only. Entry format: `## [YYYY-MM-DD] <operation> | <title>` with
  operation in `bootstrap`, `ingest`, `query`, `lint`. Existing entries are never edited.
  An entry is written after bootstrap, ingest, lint, and when a synthesis is saved (that is
  the `query` entry). Answering a question without saving anything writes **no** entry, so
  asking the wiki never leaves a change in the working tree.

## 8. Operations

### Ingest (feeding the wiki)

Three modes:

- **Cold start for a feature:** the user names a feature. Read its `.feature` file, the
  UI, services, validation, entities and unit tests. Create the pages.
- **Incremental:** for each page, run
  `git diff --name-only <verified_commit>..HEAD -- <sources of that page>`. Review only
  pages whose sources changed.
- **After archive:** an OpenSpec change was just archived. See "Ingest after archiving a
  change" below.

Steps (both modes):
1. Read the relevant sources fully. Apply the trust hierarchy (section 2).
2. Create or update the feature page, and every business-rule, architecture,
   data-model page and glossary entry that the change affects. One change often touches
   several pages.
3. Surface contradictions in `## Discrepancies`. Never overwrite silently.
4. Refresh `summary`, `tags`, `sources`, `verified_commit`, `confidence` on every page
   reviewed.
5. Update `index.md` (this is the indexing step and it is mandatory).
6. Append a log entry.
7. End with a **report**: created pages, updated pages, discrepancies, claims that could
   not be verified.

### Ingest after archiving a change

The wiki is refreshed as the **last step** of archiving an OpenSpec change, so nobody has
to remember it. The order is fixed:

1. The delta specs are synced into `openspec/specs/`.
2. The change is moved to `openspec/changes/archive/`.
3. **Ingest** runs, because it must read the updated `openspec/specs/` and the final code.

The ingest finds what the change touched: the files in its archive folder and the code
files it changed. It reviews every page whose `sources` overlap those files and creates
pages for new capabilities. The archive summary then includes the ingest report.

- **The change affects nothing documented:** no page lists any of the touched files in
  `sources`. Do not invent work. Report that no pages needed an update and still append a
  log entry (`## [YYYY-MM-DD] ingest | <change> (no pages affected)`).
- **How the step is triggered:** `openspec/config.yaml` has an advisory instruction under
  `operations.archive` that tells the agent doing the archive to run this ingest. The OpenSpec
  CLI does not run it. If the step is skipped, lint reports the affected pages as `stale`, so
  the omission is detected and fixed by an incremental ingest.

### Query (reading the wiki)

1. Read `index.md`; use search when the index is not enough (section 9, added with the
   search tool).
2. Open the relevant pages and follow links.
3. Answer in the **language of the question**. Search in English: translate the
   question's terms into English before searching, because pages are in English.
4. Lead with `Business rules` for business questions and with `Implementation` (with code
   references) for technical questions.
5. Cite the wiki pages used.
6. If the wiki does not cover the question, say so, name the gap and suggest an ingest.
   Do not guess.
7. If a cited page has a source changed after its `verified_commit`, warn that the page
   may be outdated.
8. If the answer has lasting value, offer to save it as a `synthesis/` page. Create it,
   update `index.md` and append to `log.md` only after the user agrees.

### Lint (checking wiki health)

Run `wiki/tools/lint --json` from the repository root, then add the judgment checks the
tool cannot do: claims with no source, concepts mentioned on several pages without their
own page, whether `Discrepancies` can now be resolved. Lint produces a report and
**does not change pages**. Fixes are applied only after the user accepts them, as an
ingest.

The tool is **read-only**: it reads files and runs read-only git commands, and never
writes. It exits `1` when there is at least one error and `0` otherwise (warnings do not
fail). `--json` returns `{pages, errors, warnings, findings[]}`; each finding has
`severity`, `code`, `path` and `message`.

| Code | Severity | Meaning |
|---|---|---|
| `frontmatter-missing` / `frontmatter-field` | error | No frontmatter block; a required field is missing, empty or invalid (type, confidence, 40-character `verified_commit`) |
| `feature-sections` | error | A feature page lacks `## Business rules` or `## Implementation` |
| `duplicate-slug` | error | Two pages share a file name, so `[[links]]` are ambiguous |
| `not-in-index` / `index-missing` | error | A page is not listed in `index.md`; `index.md` is missing |
| `dead-link` | error | `[[name]]` matches no page (links inside code are ignored) |
| `code-ref-path` | error | A cited file path does not exist |
| `code-ref-symbol` | error | `` `path` `Symbol` ``: the symbol is no longer in that file |
| `scenario-missing` / `requirement-missing` | error | A cited `.feature` scenario or OpenSpec requirement does not exist |
| `source-missing` / `source-forbidden` | error | A `sources` entry does not exist, or points into `.ai/` |
| `commit-unknown` | error | `verified_commit` is not a commit of this repository |
| `log-format` / `log-edited` / `log-missing` | error | A log entry is malformed; existing log lines were changed or removed (append-only); no log |
| `stale` | warning | Files in `sources` changed after `verified_commit` (drift); the finding names them |
| `orphan` | warning | No other page links to this page (the glossary is exempt) |
| `discrepancy-open` | warning | The page has a non-empty `## Discrepancies` section |
| `no-git` | warning | Not a git repository; drift was not checked |

A `stale` page is not wrong, it is **unverified**: re-run an incremental ingest for it.
Verification of the tool itself is `wiki/tools/tests/run-tests.sh` (search and lint),
using fixtures under `wiki/tools/tests/`. Run it after changing the tool.

## 9. Search

`index.md` is the first stop for navigation. Use search when the index does not make the
right page obvious, when the wiki has grown beyond what the index lists comfortably, or
to find every page that mentions a term.

```
wiki/tools/search <query> [--type T] [--tag T]... [--limit N] [--json]
```

Run it from the repository root. It needs the .NET 10 SDK; each call takes about a
second. It is local, offline, and builds no persistent index: every call reads the
current pages, so a page is searchable as soon as it is saved.

| Option | Meaning |
|---|---|
| `<query>` | Free text, **in English**. Stop words are ignored; plurals are folded. |
| `--type T` | Only pages whose `type` is `T` (`feature`, `business-rule`, `architecture`, `data-model`, `synthesis`, `glossary`). |
| `--tag T` | Only pages tagged `T`. Repeat to require several tags. |
| `--limit N` | At most `N` results. Default 10. |
| `--json` | Machine-readable output (use this from agents). |

Output with `--json` is an array of objects with `path`, `title`, `type`, `summary`,
`snippet` and `score`, best match first. Without `--json` it prints a numbered,
human-readable list. When nothing matches, the output is empty (`[]` with `--json`) and
the exit code is still `0`. Exit code `2` means a usage error, for example a query
made only of stop words.

How results are ranked: BM25 over four fields with weights: `title` (5), `tags` (4),
`summary` (3), body (1). A match in the title, tags or summary therefore outranks a
match only in the body text. This is why `summary` and `tags` must be accurate.

Rules for agents:
- **Translate first.** Pages are in English and ranking is lexical. If the question is in
  Polish, search with English terms (for example `usuwanie fiszki` -> `flashcard deletion`
  `delete`). Use the synonyms and Polish equivalents in [[glossary]] for domain terms.
- **Try synonyms.** The search does not understand meaning. If a query returns nothing,
  retry with a synonym or a tag before concluding the wiki has a gap.
- **Filter when the question is typed.** Business questions: `--type business-rule`
  and `--type feature`. Data questions: `--type data-model`.
- **Read the page, not the snippet.** The snippet only shows why a page matched.
- An empty result after a few synonym tries means a **gap**: say so (see Query, step 6).

Example:

```
wiki/tools/search "flashcard deletion" --type feature --json --limit 3
```

The tool's behavior is covered by `wiki/tools/tests/run-tests.sh`, which runs against a
fictional wiki in `wiki/tools/tests/mini-wiki/`. Run it after changing the tool.
