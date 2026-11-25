# Fiszki Wiki Index

Catalog of every page in this wiki. Read this first, then open the pages that match the
question. Maintained by the agent; every ingest updates it. See [SCHEMA.md](SCHEMA.md).

Format of an entry: `- [[page-name]] - one-line description`.

## Features

How user-visible features work: business rules and implementation.

- [[ai-flashcard-generation]] - how a user generates flashcard proposals with AI, reviews and saves them: input limits, accept and reject, what is saved, placeholder fallback
- [[manual-flashcard-creation]] - how a user creates a flashcard by hand in the dialog: required fields, tags, limits, what is shown, and the service-only update with no screen
- [[user-registration]] - how a visitor creates an account: fields, the mismatch message that does not block registration, email and password rules, duplicates, automatic login
- [[user-login-and-logout]] - how a user signs in and out: messages, inactive accounts, protected pages, menu, how long the login lasts, what is missing
- [[flashcard-list-and-filters]] - what the Flashcards page shows: states, newest-first list of active cards capped at 100, counters that include archived cards and filter, the learning progress bar, card and list views, flipping, and what is missing
- [[flashcard-archiving]] - how a user marks a flashcard as learned ("I learned it"), the archived view with restore and delete, the learning progress percentage and archived count, and what counts toward 100%
- [[site-navigation]] - the side menu for logged-out and logged-in users and its order and separators, open and protected pages, layout, error handling, and how it is tested
- [[information-pages]] - what Home, How to Start, Examples, About Technicalities and About Me contain, and a table of their claims that do not match the code
- [[flashcard-deletion]] - how a user permanently deletes their own flashcard, active or archived: confirmation dialog, ownership check, cascade to learning progress, effect on counters, progress and points
- [[leaderboard]] - how active users are ranked by points (1 per flashcard plus 3 per archived flashcard): the flashcards, archived, learning progress and points columns, ordering and tie-break by the latest creation or archiving, masked emails, 30 per page, the motivation banner, and what the tests cover

## Business rules

Cross-cutting rules that apply to more than one feature.

- [[flashcard-ownership]] - users read, update, delete, archive and restore only their own flashcards; foreign ids behave as not found
- [[flashcard-content-limits]] - length and tag limits for manual create, AI save and update, what screens enforce, and where the limits disagree
- [[credential-rules]] - email and password rules at registration and login side by side, the gaps between them and the weak policy

## Architecture

Layers, components, services, integration points.

- [[application-layers]] - Blazor Server layers, direct UI-to-service calls with no HTTP API, user identity flow, database provider and test mode
- [[openrouter-integration]] - the minimal OpenRouter chat client: configuration, request, error mapping, no retries or rate limiting
- [[generation-job-flow]] - in-memory generation jobs, background task, polling, states, what cancel and failure really do
- [[authentication-session]] - the in-memory login state per browser connection, unused cookie setup, page protection, roles with no effect
- [[ci-cd-and-startup]] - what the CI/CD workflow and application startup really do, including what is missing: migrations, post-deploy health check, rollback

## Data model

Entities, relations, constraints, migrations.

- [[flashcard-entity]] - flashcards table: fields including the nullable archived-at time, creation source, ownership and delete cascade
- [[user-entity]] - users table: unique case-insensitive email, role, active flag, counters, cascade, and which fields are ever used or changed
- [[learning-progress-entity]] - learning_progress table: spaced-repetition state, one-to-one with a flashcard, cascade delete; not the learning progress percentage

## Syntheses

Saved answers, comparisons and analyses with lasting value.

_No pages yet._

## Reference

- [[glossary]] - domain terms and synonyms used across the wiki
