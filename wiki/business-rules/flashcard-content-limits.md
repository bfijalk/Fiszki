---
title: Flashcard content limits
type: business-rule
summary: The length and count limits on flashcard text and tags for each way a card gets into the system (manual create, AI save, update), what the screens enforce versus the service, and where the limits disagree.
tags: [limits, validation, length, characters, front, back, question, answer, tags, maximum, manual, ai, update, consistency, trimming]
sources:
  - Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs
  - Fiszki.Services/Validation/UpdateFlashcardCommandValidator.cs
  - Fiszki.Services/Validation/SaveProposalsCommandValidator.cs
  - Fiszki.Services/Validation/StartGenerationCommandValidator.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Mapping/FlashcardMapper.cs
  - Components/Pages/Flashcards.razor
  - Components/Pages/Generate/ProposalReviewDialog.razor
  - Fiszki.Database/Configurations/FlashcardConfiguration.cs
  - Fiszki.Tests/Services/Validation/ValidatorTests.cs
verified_commit: 5367383718e4da80f89f35abf89c2b78f6f682c8
confidence: high
---

# Flashcard content limits

## Rule

Limits on flashcard text and tags are set by the services' validators, not by the database (the
text columns are unbounded, see [[flashcard-entity]]) and mostly not by the screens. The limits
differ between the ways a flashcard gets in. All lengths are counted in characters.

| Path | Front (question) | Back (answer) | Example | Notes | Tags |
|---|---|---|---|---|---|
| Manual create | required, at most 1000 | required, at most 2000 | not used | not used | at most 10, each at most 50 |
| Update (service only) | same as manual create | same as manual create | not used | not used | same as manual create |
| Save of AI proposals | required, at most **500** | required, at most **500** | at most 1000, **not stored** | at most 1000, **not stored** | not used (saved cards have no tags) |

Input to AI generation has its own limits (text 50 to 5000 characters, language, 1 to 50 cards),
see [[ai-flashcard-generation]].

"Required" means not empty and not only spaces. At least one proposal must be accepted when
saving AI proposals.

## Where it applies

- [[manual-flashcard-creation]]: the first row, and the update row for the service-only edit.
- [[ai-flashcard-generation]]: the third row, checked when the user clicks Save Selected.

## What the screens enforce

- **Manual create dialog:** only that question and answer are not empty or spaces. No length
  limit and no tag limit in the form; the rest is caught after clicking Create Card and shown as
  "Validation failed". (`Components/Pages/Flashcards.razor` `CreateCard`)
- **AI proposal edit dialog:** only that front and back are not empty. No length limit, so a
  longer text is caught when the user saves the selection. (`Components/Pages/Generate/ProposalReviewDialog.razor` `CanSave`)

## Cleaning before saving

- **Manual create and update:** question and answer are trimmed; tags are trimmed, empty tags are
  dropped and duplicates are removed ignoring letter case, keeping the first spelling.
  (`Fiszki.Services/Services/FlashcardService.cs` `CreateAsync`,
  `Fiszki.Services/Services/FlashcardService.cs` `UpdateAsync`)
- **AI proposals:** the text is stored exactly as it is in the proposal, **not trimmed**.
  (`Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`)
- The 10-tag limit is checked on the tags as sent, before duplicates are removed, and each tag's
  length before trimming.

## Evidence

- Manual create limits: `Fiszki.Services/Validation/CreateFlashcardCommandValidator.cs`
  `CreateFlashcardCommandValidator`. Tested: an empty front fails, more than 10 tags fail, a valid
  card passes. **Not tested:** the 1000 and 2000 limits, the back being empty, the 50-character tag
  limit. (`Fiszki.Tests/Services/Validation/ValidatorTests.cs` `CreateFlashcardCommandValidatorTests`)
- Update limits: `Fiszki.Services/Validation/UpdateFlashcardCommandValidator.cs`
  `UpdateFlashcardCommandValidator` (also requires a non-empty flashcard id). No test.
- AI save limits: `Fiszki.Services/Validation/SaveProposalsCommandValidator.cs`
  `SaveProposalsCommandValidator`. No test; the service tests mock this validator.

## Edge cases

- **The paths disagree.** A card written by hand can have a question of up to 1000 characters,
  but the same text cannot be saved as an AI card (limit 500). An AI card saved at 500 characters
  can later be updated, through the service, up to 1000 and 2000.
- **An AI proposal can look fine and still fail to save.** The edit dialog accepts any length, so
  a long edit is only rejected at the final save, and the message the user gets is "Failed to save
  flashcards: Validation failed" without naming the field. The whole save fails, not only that
  card.
- **Example and notes are validated but thrown away.** Their limits are checked, yet they are
  never stored.

## Discrepancies

The historical API plan (api-plan.md in the old `.ai` folder) lists one limit for all paths:
question and answer at most 2000 characters, at most 10 tags of at most 40 characters, distinct
ignoring case. The code has three different sets (table above) and allows tags of 50 characters.
The code wins.

## Related

- [[manual-flashcard-creation]]
- [[ai-flashcard-generation]]
- [[flashcard-entity]]
