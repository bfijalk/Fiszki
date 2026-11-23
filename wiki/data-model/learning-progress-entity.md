---
title: Learning progress entity
type: data-model
summary: The learning_progress table and LearningProgress entity that hold spaced-repetition state per flashcard, and how it is deleted with its flashcard.
tags: [learning-progress, spaced-repetition, entity, table, database, review, cascade, ease-factor]
sources:
  - Fiszki.Database/Entities/LearningProgress.cs
  - Fiszki.Database/Configurations/LearningProgressConfiguration.cs
  - Fiszki.Database/Migrations/20251014085238_InitialCreate.cs
  - Fiszki.Services/Services/TestDataSeeder.cs
verified_commit: cdf3374897d4b0f5eab4affaaba6a6874064698e
confidence: high
---

# Learning progress entity

## Entity

One record per flashcard holding its review schedule for spaced repetition. Stored in the
`learning_progress` table and mapped from
`Fiszki.Database/Entities/LearningProgress.cs` `LearningProgress`.

## Fields

| Field | Type | Constraints | Meaning |
|---|---|---|---|
| `Id` | GUID | primary key | Record identifier |
| `FlashcardId` | GUID | **unique** | The flashcard this record belongs to |
| `UserId` | GUID | indexed together with `NextReviewDate` | Owner |
| `EaseFactor` | double | default 2.5 | Spaced-repetition ease factor |
| `Interval` | int | | Current review interval |
| `Repetitions` | int | | Number of successful repetitions |
| `NextReviewDate` | timestamp | default now (UTC) | When the flashcard is due |
| `LastReviewDate` | timestamp, nullable | | Last review time |
| `CreatedAt` / `UpdatedAt` | timestamp | UTC | Bookkeeping |

## Relations

- **Flashcard:** one-to-one, `FlashcardId` is unique. Deleting the flashcard deletes this
  record (`DeleteBehavior.Cascade`).
  (`Fiszki.Database/Configurations/LearningProgressConfiguration.cs` `Configure`)
- **User:** many records to one user. Deleting the user deletes their records.

## Lifecycle

In the current code no application flow creates or updates learning-progress records. The
only code that creates them is the test data seeder
(`Fiszki.Services/Services/TestDataSeeder.cs` `TestDataSeeder`). The table and mapping are in
place, so the cascade rule is structural today: it matters for seeded data and for when a
review feature writes these records.

## Related

- [[flashcard-entity]]
- [[flashcard-deletion]]
