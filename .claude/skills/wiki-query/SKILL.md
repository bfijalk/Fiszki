---
name: wiki-query
description: Answer questions about the Fiszki project from its wiki (wiki/): how a feature works, what business rules apply, how it is implemented, which entities and services are involved. Use whenever a developer or business analyst asks about behavior, rules, implementation or data model of Fiszki, in any language (e.g. "jak dziala usuwanie fiszki", "jakie sa reguly walidacji", "how is generation implemented"). Answers in the language of the question.
---

# wiki-query

Answers from the wiki, not from the raw code. The rules live in **`wiki/SCHEMA.md`**
(sections 8 and 9); follow them. This skill is read-only unless the user agrees to save a
synthesis.

## Steps

1. **Read `wiki/index.md`** and pick the pages that match the question.
2. **Search if the index is not enough.** Pages are in English and ranking is lexical, so
   translate the question's terms into English first (use `wiki/glossary.md` for domain
   terms and Polish equivalents), then run:
   `wiki/tools/search "<english terms>" --json` (add `--type` / `--tag` for typed questions).
   If nothing is found, retry with a synonym or a tag before concluding there is a gap.
3. **Open the pages** and follow `[[links]]` where the answer continues.
4. **Answer in the language of the question.**
   - Business question: lead with `Business rules`, quote the rule and its cited
     `.feature` scenario or OpenSpec requirement.
   - Implementation question: lead with `Implementation`, with file path and symbol.
5. **Cite the wiki pages** you used, by path.
6. **Warn about staleness.** Run `wiki/tools/lint --json` and check whether a cited page
   has a finding with `code == "stale"`. If so, say that page may be outdated and name
   the changed files.
7. **State gaps.** If the wiki does not cover the question, say so explicitly, name what
   is missing, and suggest `wiki-ingest` for that feature. **Do not guess** and do not fill
   the gap from memory or by reading code and presenting it as wiki-backed. If the user
   asks you to look at the code anyway, say clearly that the answer is not from the wiki.
8. **Offer to save** an answer of lasting value (a comparison, an analysis) as a page in
   `wiki/synthesis/`. Only after the user agrees: create it from
   `wiki/templates/synthesis.md`, update `wiki/index.md`, append
   `## [YYYY-MM-DD] query | <title>` to `wiki/log.md`, and run `wiki/tools/lint --json`.

Do not edit any other wiki page while answering.
