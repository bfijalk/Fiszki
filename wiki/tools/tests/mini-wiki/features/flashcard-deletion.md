---
title: Flashcard deletion
type: feature
summary: How a user deletes a flashcard and what happens to related data.
tags: [flashcards, deletion, delete]
sources:
  - Fiszki.Services/Services/FlashcardService.cs
verified_commit: 0000000000000000000000000000000000000000
confidence: high
---

# Flashcard deletion

## Business rules

- A confirmation dialog is shown before the flashcard is removed.
- Only the owner of a flashcard can remove it.

## Implementation

- `FlashcardService` `DeleteAsync` removes the entity.
