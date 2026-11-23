---
name: wiki-lint
description: Check the health of the Fiszki project wiki (wiki/): dead links, pages missing from the index, orphans, broken code references, and pages that drifted from the code since their verified_commit. Use when asked to lint, audit or health-check the wiki, or to find out which wiki pages are stale. Produces a report only; it never edits pages.
---

# wiki-lint

Reports on wiki health. The rules and the list of finding codes are in
**`wiki/SCHEMA.md`** (section 8, Lint). **This skill never changes pages.** The only file
it may touch is `wiki/log.md`, to append one entry at the end.

## Steps

1. Run `wiki/tools/lint --json` from the repository root. It is deterministic and
   read-only. Keep its output as the base of the report.
2. Add the **judgment checks** the tool cannot do, by reading the pages:
   - claims about behavior or business rules with no cited source;
   - concepts mentioned on several pages that have no page or glossary entry of their own;
   - `## Discrepancies` sections that the current sources now resolve;
   - pages whose `summary` or `tags` no longer match their content (search quality).
3. Write the **report**, grouped by severity:
   - Errors (tool exit code 1): what is broken, on which page, and the proposed fix.
   - Warnings: `stale` pages (name the changed source files), orphans, open discrepancies.
   - Judgment findings from step 2, clearly marked as agent findings.
   - Gaps: up to 5 questions the wiki cannot answer yet and the sources to ingest.
4. For each proposed fix say which skill applies it. Content fixes and re-verification of
   stale pages are done by **`wiki-ingest`**, and only **after the user accepts them**.
   Do not apply any fix as part of linting.
5. Append one entry to `wiki/log.md`: `## [YYYY-MM-DD] lint | <N errors, M warnings>`
   with one line per finding group. Existing entries are never edited.

After linting, the working tree must contain no changes other than that log entry.
