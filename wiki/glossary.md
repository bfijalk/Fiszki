---
title: Glossary
type: glossary
summary: Domain terms of the Fiszki project with synonyms and Polish equivalents, so questions in either language reach the right page.
tags: [glossary, terminology, synonyms, polish, fiszka]
sources:
  - Fiszki.Database/Entities/Flashcard.cs
  - Fiszki.Database/Entities/LearningProgress.cs
  - Components/Pages/Flashcards.razor
verified_commit: 5d1568b818c698f75aa953c7422003d1563bec9d
confidence: high
---

# Glossary

Each entry lists English synonyms and Polish equivalents so lexical search finds the right
page from either language. The Polish equivalents are translation aids written for search;
they are not taken from the code, which is in English.

| Term | Synonyms / Polish | Meaning | Pages |
|---|---|---|---|
| Flashcard | card, fiszka, karta | One question and answer pair owned by a user; table `flashcards` | [[flashcard-entity]], [[flashcard-deletion]] |
| Front / Question | front side, awers, przod, pytanie | `FrontContent`; shown as "Question" in the UI | [[flashcard-entity]] |
| Back / Answer | back side, rewers, tyl, odpowiedz | `BackContent`; shown as "Answer" in the UI | [[flashcard-entity]] |
| Creation source | source, AI generated, manual, zrodlo, reczna, wygenerowana | `Ai` or `Manual`; drives the "AI Generated" and "Manual" counters | [[flashcard-entity]] |
| Owner | user, account, wlasciciel, uzytkownik | The user whose id is stored on a flashcard; only the owner can read, change or delete it | [[flashcard-ownership]] |
| Learning progress | spaced repetition, review schedule, postep nauki, powtorki | Per-flashcard review state (ease factor, interval, next review date) | [[learning-progress-entity]] |
| Delete confirmation dialog | confirm deletion, modal, okno potwierdzenia, potwierdzenie usuniecia | The "Confirm Deletion" dialog shown before a flashcard is deleted | [[flashcard-deletion]] |
| Permanent deletion | hard delete, remove, usuniecie, trwale usuniecie | The flashcard row is removed; there is no soft delete, trash or undo | [[flashcard-deletion]] |
| Idempotent deletion | silent no-op, repeated delete, idempotentne usuwanie | Deleting an unknown or foreign flashcard id does nothing and raises no error | [[flashcard-deletion]], [[flashcard-ownership]] |
| Cascade delete | cascading, usuwanie kaskadowe | Deleting a parent row removes dependent rows: user to flashcards, flashcard to learning progress | [[flashcard-entity]], [[learning-progress-entity]] |
| Test mode | in-memory database, tryb testowy | Run with an in-memory database instead of PostgreSQL (`Test` environment, `TestMode` setting or `--test-mode`) | [[application-layers]] |
