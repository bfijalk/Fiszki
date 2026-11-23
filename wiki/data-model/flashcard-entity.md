---
title: Flashcard entity
type: data-model
summary: The flashcards table and Flashcard entity: fields, ownership by a user, creation source, tags, and delete cascade rules.
tags: [flashcards, entity, table, database, schema, tags, creation-source, cascade, ef-core, leaderboard, points]
sources:
  - Fiszki.Database/Entities/Flashcard.cs
  - Fiszki.Database/Configurations/FlashcardConfiguration.cs
  - Fiszki.Database/Migrations/20251014085238_InitialCreate.cs
  - Fiszki.Database/Entities/User.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
  - Fiszki.Services/Services/LeaderboardService.cs
verified_commit: cdf3374897d4b0f5eab4affaaba6a6874064698e
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

Column names for the text fields, tags and timestamps are snake_case, set in
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
- Removed for good (no soft delete) by the deletion flow, see [[flashcard-deletion]].
- Counted as a point on the [[leaderboard]]: every row owned by an active user is one point, whatever its
  `CreationSource`, so creating and deleting flashcards moves the owner in the ranking
  (`Fiszki.Services/Services/LeaderboardService.cs` `GetAsync`).

## Related

- [[flashcard-deletion]]
- [[flashcard-ownership]]
- [[learning-progress-entity]]
- [[leaderboard]]
