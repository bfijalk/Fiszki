---
title: Glossary
type: glossary
summary: Domain terms of the Fiszki project with English synonyms, so that lexical search finds the right page whichever word is used.
tags: [glossary, terminology, synonyms]
sources:
  - Fiszki.Database/Entities/Flashcard.cs
  - Fiszki.Database/Entities/LearningProgress.cs
  - Components/Pages/Flashcards.razor
  - Fiszki.Services/Services/GenerationService.cs
  - Components/Pages/Generate/ProposalsSection.razor
  - Components/Pages/Generate/SourceInputForm.razor
verified_commit: 52f627aae9f0b25fa16da531aa2d9e66b421bcb9
confidence: high
---

# Glossary

Each entry lists English synonyms so lexical search finds the right page whichever word is
used. The wiki is written in English only; questions asked in another language are
translated by the agent before searching.

| Term | Synonyms | Meaning | Pages |
|---|---|---|---|
| Flashcard | card | One question and answer pair owned by a user; table `flashcards` | [[flashcard-entity]], [[flashcard-deletion]] |
| Front / Question | front side, prompt | `FrontContent`; shown as "Question" in the UI | [[flashcard-entity]] |
| Back / Answer | back side, explanation | `BackContent`; shown as "Answer" in the UI | [[flashcard-entity]] |
| Creation source | source, AI generated, manual | `Ai` or `Manual`; drives the "AI Generated" and "Manual" counters | [[flashcard-entity]] |
| Owner | user, account | The user whose id is stored on a flashcard; only the owner can read, change or delete it | [[flashcard-ownership]] |
| Learning progress | spaced repetition, review schedule | Per-flashcard review state (ease factor, interval, next review date) | [[learning-progress-entity]] |
| Delete confirmation dialog | confirm deletion, modal | The "Confirm Deletion" dialog shown before a flashcard is deleted | [[flashcard-deletion]] |
| Permanent deletion | hard delete, remove | The flashcard row is removed; there is no soft delete, trash or undo | [[flashcard-deletion]] |
| Idempotent deletion | silent no-op, repeated delete | Deleting an unknown or foreign flashcard id does nothing and raises no error | [[flashcard-deletion]], [[flashcard-ownership]] |
| Cascade delete | cascading | Deleting a parent row removes dependent rows: user to flashcards, flashcard to learning progress | [[flashcard-entity]], [[learning-progress-entity]] |
| Test mode | in-memory database | Run with an in-memory database instead of PostgreSQL (`Test` environment, `TestMode` setting or `--test-mode`) | [[application-layers]] |
| Proposal | suggestion, generated card, candidate | A flashcard suggested by the AI that is not saved until the user accepts and saves it; it has a front, a back, an example and notes | [[ai-flashcard-generation]], [[generation-job-flow]] |
| Source text | input text, topic text | The text, 50 to 5000 characters, typed on the Generate page; the prompt uses it as a topic | [[ai-flashcard-generation]] |
| Maximum cards | max cards, card limit | Upper limit of proposals to request, 1 to 50, default 20 | [[ai-flashcard-generation]] |
| Accept All / Reject All | bulk accept, bulk reject | Bulk review actions on proposals; there is no accept action for a single card | [[ai-flashcard-generation]] |
| Generation job | job, generation run | In-memory record of one generation with a state and its proposals | [[generation-job-flow]] |
| Placeholder proposals | fallback, mock generation, dummy cards | Fake cards ("Polish Word 1") produced when the AI call fails, shown as a normal successful generation | [[ai-flashcard-generation]], [[openrouter-integration]] |
| OpenRouter | LLM gateway, AI provider | The external service used to call language models | [[openrouter-integration]] |
| Structured output | JSON schema response | An AI answer constrained to a JSON schema and parsed into objects | [[openrouter-integration]] |
