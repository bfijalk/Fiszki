---
name: wiki-ingest
description: Create or update pages of the Fiszki project wiki (wiki/) from the source code, .feature files and OpenSpec specs. Use when asked to "ingest" or document a feature in the wiki (cold start), to refresh stale wiki pages after code changes (incremental), or as the LAST step after archiving an OpenSpec change. Use it before wiki-query when the wiki has no coverage of a feature.
---

# wiki-ingest

Feeds the wiki. The rules live in **`wiki/SCHEMA.md`**; read it first and follow it. This
skill only says how to enter the workflow. Pages are written in **English**.

Never modify anything outside `wiki/`. Sources (code, `.feature`, `openspec/`, `.ai/`) are
read-only. `.ai/` is a low-confidence hint only and must never appear in a page's `sources`.

## Pick the mode

| Input | Mode |
|---|---|
| A feature name ("flashcard deletion", "login") | **Cold start**: document that feature from scratch |
| Nothing, or "refresh" | **Incremental**: review pages whose sources changed |
| An OpenSpec change name that was just archived | **After archive**: incremental, scoped to that change |

## Cold start

1. Find the sources: the feature's `.feature` file, UI under `Components/`, services,
   commands and validators under `Fiszki.Services/`, entities and migrations under
   `Fiszki.Database/`, unit tests under `Fiszki.Tests/`, and any `openspec/specs/`.
2. Read them fully. Apply the trust hierarchy in SCHEMA section 2.
3. Create the feature page from `wiki/templates/feature.md`. Then create or update the
   business-rule, architecture and data-model pages and the glossary entries that this
   feature touches.
4. Tell the user the 3 to 7 key findings and ask what to emphasize, unless they said to
   skip questions.

## Incremental

1. Run `wiki/tools/lint --json` and take the findings with `code == "stale"`. Each names
   the page and the changed source files. (Equivalent per page:
   `git diff --name-only <verified_commit>..HEAD -- <sources>`.)
2. Review only those pages against the current sources. Add and correct; do not rewrite
   from scratch.
3. A source change can also create **new** behavior no page covers: create the pages.

## After archive

The change was just archived, so `openspec/specs/` is updated.

1. Find what the change touched: files under `openspec/changes/archive/<date>-<name>/`
   (proposal, design, specs, tasks) and `git diff --name-only` for the code it changed.
2. Run the incremental steps for the pages whose `sources` overlap those files, and
   create pages for new capabilities.
3. If the change affects **no source listed in any page**, do not invent work: report
   that no pages needed an update and still append a log entry.

## Always, before finishing (mandatory)

1. Apply SCHEMA rules for contradictions: never overwrite silently; use `## Discrepancies`.
2. On every page you reviewed, refresh `summary`, `tags`, `sources` and `confidence`, and
   set `verified_commit` to `git rev-parse HEAD`. If a listed source has **uncommitted**
   changes, say so in the report and set `confidence: medium`, because the page may
   describe code that is not committed yet.
3. Update `wiki/index.md` for every created, renamed or re-described page.
4. Append an entry to `wiki/log.md`: `## [YYYY-MM-DD] ingest | <title>`.
5. Run `wiki/tools/lint --json`. Fix every **error** you introduced; report warnings.
6. End with a **report**: pages created, pages updated, discrepancies found, claims you
   could not verify (these get `confidence: low`).
