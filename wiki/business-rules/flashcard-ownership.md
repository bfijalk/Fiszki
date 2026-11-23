---
title: Flashcard ownership
type: business-rule
summary: A user can read, update and delete only their own flashcards; the service scopes every flashcard operation by user id.
tags: [flashcards, ownership, authorization, security, user, access, isolation]
sources:
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Tests/Services/FlashcardServiceTests.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
verified_commit: 52f627aae9f0b25fa16da531aa2d9e66b421bcb9
confidence: high
---

# Flashcard ownership

## Rule

Every flashcard belongs to exactly one user. All flashcard operations take the acting user's
id and act **only** on flashcards owned by that user:

- Create stores the flashcard under the acting user.
- Get, update and delete look the flashcard up by its id **and** the acting user's id.
- List returns only the acting user's flashcards, newest first.

A flashcard that exists but belongs to someone else behaves exactly like one that does not
exist: get returns nothing, update fails with a "not found" error, delete does nothing and
reports no error. The service therefore never reveals that another user's flashcard exists.

## Where it applies

- [[flashcard-deletion]]: deleting another user's flashcard is a silent no-op.
- [[ai-flashcard-generation]]: saved AI flashcards belong to the user who saved them.

## Evidence

- Lookups filter on both ids. (`Fiszki.Services/Services/FlashcardService.cs` `GetAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `ListAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `UpdateAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync`)
- Create assigns the acting user. (`Fiszki.Services/Services/FlashcardService.cs` `CreateAsync`)
- Saving accepted AI proposals assigns the acting user to every new flashcard.
  (`Fiszki.Services/Services/GenerationService.cs` `SaveProposalsAsync`,
  `Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`)
- Delete with another user's id leaves the flashcard and its owner unchanged.
  (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `DeleteAsync_ShouldNotDeleteOtherUsersCard`)

Test coverage: the delete path is covered by a unit test. The filters in get, list and update
are visible in the service code; this page does not claim tests for them.

## Edge cases

- The acting user's id is supplied by the caller. The service does not authenticate; the UI
  takes it from the signed-in user's claims, see [[application-layers]].
- Deleting a user cascades to their flashcards, see [[flashcard-entity]].

## Related

- [[flashcard-deletion]]
- [[flashcard-entity]]
- [[application-layers]]
