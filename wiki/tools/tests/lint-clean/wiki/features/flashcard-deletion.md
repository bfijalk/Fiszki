---
title: Flashcard deletion
type: feature
summary: How a user deletes a flashcard.
tags: [flashcards, deletion]
sources:
  - Fiszki.FunctionalTests/Features/FlashcardDeletion.feature
  - Fiszki.Services/Services/FlashcardService.cs
  - openspec/specs/flashcard-deletion/spec.md
verified_commit: __COMMIT__
confidence: high
---

# Flashcard deletion

## Business rules

- Deletion needs confirmation. (`Fiszki.FunctionalTests/Features/FlashcardDeletion.feature`, scenario "Delete a flashcard with confirmation")
- The owner may delete. (`openspec/specs/flashcard-deletion/spec.md`, requirement "Owner can delete a flashcard")

## Implementation

- `Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync` removes the entity, see [[flashcard-entity]].
