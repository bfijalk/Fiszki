# Fiszki Wiki Index

Catalog of every page in this wiki. Read this first, then open the pages that match the
question. Maintained by the agent; every ingest updates it. See [SCHEMA.md](SCHEMA.md).

Format of an entry: `- [[page-name]] - one-line description`.

## Features

How user-visible features work: business rules and implementation.

- [[flashcard-deletion]] - how a user permanently deletes their own flashcard: confirmation dialog, ownership check, cascade to learning progress

## Business rules

Cross-cutting rules that apply to more than one feature.

- [[flashcard-ownership]] - users read, update and delete only their own flashcards; foreign ids behave as not found

## Architecture

Layers, components, services, integration points.

- [[application-layers]] - Blazor Server layers, direct UI-to-service calls with no HTTP API, user identity flow, database provider and test mode

## Data model

Entities, relations, constraints, migrations.

- [[flashcard-entity]] - flashcards table: fields, creation source, ownership and delete cascade
- [[learning-progress-entity]] - learning_progress table: spaced-repetition state, one-to-one with a flashcard, cascade delete

## Syntheses

Saved answers, comparisons and analyses with lasting value.

_No pages yet._

## Reference

- [[glossary]] - domain terms and synonyms used across the wiki
