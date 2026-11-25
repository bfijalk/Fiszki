---
title: Flashcard ownership
type: business-rule
summary: A user can read, update, delete, archive and restore only their own flashcards; the service scopes every flashcard operation by user id.
tags: [flashcards, ownership, authorization, security, user, access, isolation, archive, restore]
sources:
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Tests/Services/FlashcardServiceTests.cs
  - Fiszki.Tests/Services/FlashcardArchivingTests.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
  - Fiszki.Services/Services/LeaderboardService.cs
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: high
---

# Flashcard ownership

## Rule

Every flashcard belongs to exactly one user. All flashcard operations take the acting user's
id and act **only** on flashcards owned by that user:

- Create stores the flashcard under the acting user.
- Get, update, delete, archive and restore look the flashcard up by its id **and** the acting user's id.
- List returns only the acting user's flashcards, newest first (active or archived, as asked).
- The summary (counters and learning progress) counts only the acting user's flashcards.

A flashcard that exists but belongs to someone else behaves exactly like one that does not
exist: get returns nothing, update fails with a "not found" error, delete, archive and restore do nothing and
report no error. The service therefore never reveals that another user's flashcard exists.

## Where it applies

- [[flashcard-deletion]]: deleting another user's flashcard is a silent no-op.
- [[flashcard-archiving]]: archiving or restoring another user's flashcard is a silent no-op.
- [[ai-flashcard-generation]]: saved AI flashcards belong to the user who saved them.
- [[leaderboard]]: the one place where one user sees data about other users, and it exposes only counts of
  their flashcards and a masked email, never the content (`Fiszki.Services/Services/LeaderboardService.cs` `GetAsync`).

## Evidence

- Lookups filter on both ids. (`Fiszki.Services/Services/FlashcardService.cs` `GetAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `ListAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `UpdateAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `DeleteAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `ArchiveAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `RestoreAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `GetSummaryAsync`)
- Create assigns the acting user. (`Fiszki.Services/Services/FlashcardService.cs` `CreateAsync`)
- Saving accepted AI proposals assigns the acting user to every new flashcard.
  (`Fiszki.Services/Services/GenerationService.cs` `SaveProposalsAsync`,
  `Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`)
- Delete with another user's id leaves the flashcard and its owner unchanged.
  (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `DeleteAsync_ShouldNotDeleteOtherUsersCard`)

- Archive and restore with another user's id leave the flashcard unarchived or archived as it was.
  (`Fiszki.Tests/Services/FlashcardArchivingTests.cs` `ArchiveAsync_OtherUsersCard_ChangesNothing`,
  `Fiszki.Tests/Services/FlashcardArchivingTests.cs` `RestoreAsync_OtherUsersCard_ChangesNothing`)

Test coverage: the delete, archive and restore paths are covered by unit tests. The filters in get, list and update
are visible in the service code; this page does not claim tests for them.

## Edge cases

- The acting user's id is supplied by the caller. The service does not authenticate; the UI
  takes it from the signed-in user's claims, see [[application-layers]].
- Deleting a user cascades to their flashcards, see [[flashcard-entity]].

## Related

- [[flashcard-deletion]]
- [[flashcard-archiving]]
- [[flashcard-entity]]
- [[application-layers]]
- [[leaderboard]]
