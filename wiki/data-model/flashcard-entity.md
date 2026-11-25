---
title: Flashcard entity
type: data-model
summary: The flashcards table and Flashcard entity: fields including the nullable archived-at time that marks a flashcard as learned, ownership by a user, creation source, tags, and delete cascade rules.
tags: [flashcards, entity, table, database, schema, tags, creation-source, cascade, ef-core, leaderboard, points, archived, archived-at, learned, migration]
sources:
  - Fiszki.Database/Entities/Flashcard.cs
  - Fiszki.Database/Configurations/FlashcardConfiguration.cs
  - Fiszki.Database/Migrations/20251014085238_InitialCreate.cs
  - Fiszki.Database/Entities/User.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
  - Fiszki.Services/Services/LeaderboardService.cs
  - Fiszki.Database/Migrations/20261124100000_AddFlashcardArchivedAt.cs
  - docs/migrations/20261124100000_AddFlashcardArchivedAt.sql
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: high
---

# Flashcard entity

## Entity

A flashcard is one question and answer pair owned by a user. It is stored in the
`flashcards` table and mapped from `Fiszki.Database/Entities/Flashcard.cs` `Flashcard`.

## Fields

| Field | Type | Constraints | Meaning |
|---|---|---|---|
| `Id` | GUID (`uuid`) | primary key | Flashcard identifier |
| `UserId` | GUID (`uuid`) | indexed, required | Owner, see [[flashcard-ownership]] |
| `FrontContent` | `text` | required | Question (front side) |
| `BackContent` | `text` | required | Answer (back side) |
| `CreationSource` | enum stored as `text` (`Ai`, `Manual`) | column `creation_source` | How the flashcard was created |
| `AiModel` | `text`, nullable | column `ai_model` | AI model that produced it, for AI flashcards |
| `OriginalTextHash` | `text`, nullable | column `original_text_hash` | Hash of the source text it was generated from |
| `Tags` | list of strings (`text[]`) | column `tags` | Free-form tags |
| `CreatedAt` | `timestamp with time zone` | column `created_at` | Creation time (UTC) |
| `UpdatedAt` | `timestamp with time zone` | column `updated_at` | Last change time (UTC) |
| `ArchivedAt` | `timestamp with time zone`, nullable | column `archived_at` | When the user marked the flashcard as learned; null while it is active, see [[flashcard-archiving]] |

Column names for the text fields, tags, timestamps and the archive time are snake_case, set in
`Fiszki.Database/Configurations/FlashcardConfiguration.cs` `Configure`. Column types are
from the initial migration. The text columns are unbounded `text`: the table itself sets no
length limit.

## Relations

- **User (many flashcards to one user):** required. Deleting the user deletes their
  flashcards (`DeleteBehavior.Cascade`).
  (`Fiszki.Database/Configurations/FlashcardConfiguration.cs` `Configure`)
- **Learning progress (one flashcard to at most one record):** see [[learning-progress-entity]].
  Deleting the flashcard deletes its learning-progress record.

The same cascade rules are in the initial migration
(`Fiszki.Database/Migrations/20251014085238_InitialCreate.cs` `ReferentialAction`).

## Lifecycle

- Created as `Manual` by `Fiszki.Services/Services/FlashcardService.cs` `CreateAsync` from the
  Create Manual Flashcard dialog, see [[manual-flashcard-creation]]. Text and tags are cleaned
  and limited as described in [[flashcard-content-limits]].
- Created as `Ai` from accepted proposals: `Fiszki.Services/Services/GenerationService.cs`
  builds entities with `Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`. This page
  does not cover the generation flow itself. Only the front and back text are stored for these
  flashcards: the example and notes shown during review are dropped, tags stay empty, and
  `AiModel` and `OriginalTextHash` are left empty, so no record says which model wrote a card.
  See [[ai-flashcard-generation]].
- Updated by `FlashcardService.UpdateAsync`, which sets `UpdatedAt`. No screen calls it today:
  the edit button is a stub, see [[manual-flashcard-creation]].
- Archived (`ArchivedAt` set to now, `UpdatedAt` set too) by `FlashcardService.ArchiveAsync` when the user confirms
  "I learned it", and restored (`ArchivedAt` back to null) by `FlashcardService.RestoreAsync`; both quietly do nothing for a
  missing, foreign or already-in-state card. Archiving does not delete or change the content. The column was added by
  `Fiszki.Database/Migrations/20261124100000_AddFlashcardArchivedAt.cs`; existing rows got null, so they stayed active. See
  [[flashcard-archiving]].
- Removed for good (no soft delete) by the deletion flow, see [[flashcard-deletion]]. Archiving is not a soft delete: an
  archived card is still counted and can be restored. Archived cards can be deleted too.
- Counted on the [[leaderboard]]: every row owned by an active user is one point, whatever its
  `CreationSource`, and a row with `ArchivedAt` set is worth 3 more, so creating, archiving, restoring and deleting
  flashcards moves the owner in the ranking. The newest `CreatedAt` and `ArchivedAt` of the user's rows decide ties
  (`Fiszki.Services/Services/LeaderboardService.cs` `GetAsync`).

## Related

- [[flashcard-deletion]]
- [[flashcard-archiving]]
- [[flashcard-ownership]]
- [[learning-progress-entity]]
- [[leaderboard]]
