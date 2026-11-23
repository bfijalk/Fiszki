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
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs
  - Fiszki.Services/Services/UserService.cs
  - Fiszki.Services/Services/CustomAuthStateProvider.cs
  - Fiszki.Database/Entities/User.cs
verified_commit: 6a27f79b9a9bb38c9e68a0f1e35a693394ffb792
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
| Manual flashcard | hand-made card, created by the user | A flashcard typed in the Create Manual Flashcard dialog; creation source `Manual` | [[manual-flashcard-creation]] |
| Create dialog | create modal, Add Manual Card, Create Manually | The "Create Manual Flashcard" dialog opened from the Flashcards page | [[manual-flashcard-creation]] |
| Tags | labels, keywords | Optional free-form labels stored with a flashcard, at most 10 of up to 50 characters; not shown anywhere in the UI | [[manual-flashcard-creation]], [[flashcard-content-limits]] |
| Content limits | length limits, maximum length | The maximum text and tag sizes enforced by the validators, which differ between manual create, AI save and update | [[flashcard-content-limits]] |
| Concurrency check | stale update, expected update time | The service refuses an update when the caller's last seen update time no longer matches the stored one | [[manual-flashcard-creation]] |
| Registration | sign-up, create account | Creating an account on the Register page; the user is logged in automatically afterwards | [[user-registration]] |
| Login | sign-in, authentication | Signing in with email and password on the Login page; it lasts only for the current browser connection | [[user-login-and-logout]], [[authentication-session]] |
| Logout | sign-out | Clearing the login, from the menu button or the logout page | [[user-login-and-logout]] |
| Circuit | connection, session | The server-side connection of one browser tab to the Blazor application; the login lives as long as it | [[authentication-session]] |
| Authentication state | logged-in state, claims | The in-memory record of who is logged in, with user id, email and role claims | [[authentication-session]] |
| Credentials | email and password, login data | The email and password rules, which differ between registration and login | [[credential-rules]] |
| Role | user role, Basic, Admin | Account role stored on the user and put in the login claims; nothing checks it | [[user-entity]], [[authentication-session]] |
| Active account | inactive account, IsActive | A flag that blocks login when false; nothing in the application sets it to false | [[user-entity]], [[user-login-and-logout]] |
| Password hash | BCrypt, stored password | The only form in which the password is stored: a BCrypt hash with work factor 12 | [[credential-rules]], [[user-entity]] |
