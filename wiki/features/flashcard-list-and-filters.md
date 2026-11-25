---
title: Flashcard list, counters and filters
type: feature
summary: What the Flashcards page shows a logged-in user: the loading, error and empty states, the newest-first list of active cards capped at 100, the Total, AI and Manual counters (which include archived cards and also filter), the learning progress bar, the Active and Archived switch, card and list views, flipping, and what is missing (search, sorting, paging, tags).
tags: [flashcards, list, flashcards-page, counters, statistics, filter, ai-generated, manual, card-view, list-view, flip, empty-state, loading, error, ordering, limit, newest-first, no-search, no-sorting, no-pagination, archived, learning-progress, archived-view, active]
sources:
  - Fiszki.FunctionalTests/Features/FlashcardsUI.feature
  - Components/Pages/Flashcards.razor
  - Components/App.razor
  - wwwroot/css/flashcards.css
  - Fiszki.Services/Interfaces/IFlashcardService.cs
  - Fiszki.Services/Services/FlashcardService.cs
  - Fiszki.Services/Models/FlashcardDto.cs
  - Fiszki.Services/Models/FlashcardSummaryDto.cs
  - docs/TEST_SELECTORS.md
  - Fiszki.Tests/Services/FlashcardServiceTests.cs
  - Fiszki.FunctionalTests/Steps/FlashcardSteps.cs
  - Fiszki.FunctionalTests/PageObjects/FlashcardsPage.cs
  - Fiszki.FunctionalTests/Hooks/SeededTestUserHooks.cs
verified_commit: fd1cb20e701b61a782cff20bb553b4ebe7f5fcbe
confidence: high
---

# Flashcard list, counters and filters

## Summary

The Flashcards page (`/flashcards`) is where a logged-in user lands after signing in. It lists the
user's own flashcards, shows three counters that double as filters, and lets the user switch
between a card view and a list view and flip cards to see the answer. From here the user also
creates ([[manual-flashcard-creation]]), deletes ([[flashcard-deletion]]), marks cards as learned
([[flashcard-archiving]]) and starts generation ([[ai-flashcard-generation]]). The list shows **active** cards
only; learned cards are in an Archived view on the same page, and a learning progress bar sits under the counters.
The page has no search, no sorting options, no paging and does not show tags.

## Business rules

For analysts. Each rule says how it is backed: **Gherkin** (a `.feature` scenario), **unit
test**, or **code only** (read from the implementation, no automated test).

### What is shown

1. **Login is required** and the user sees only their own flashcards. Backed by: code only
   (`Components/Pages/Flashcards.razor` `Authorize`); the ownership rule is in [[flashcard-ownership]].
2. **A loading indicator is shown first** ("Loading your flashcards..."). Backed by: code only
   (`Components/Pages/Flashcards.razor` `LoadFlashcards`).
3. **A failure replaces the page content with a red message.** The message is "Unable to determine
   current user.", "Invalid user identifier." or the technical text of the error. There is no retry
   button; the user must reload. Backed by: code only (`Components/Pages/Flashcards.razor` `LoadFlashcards`).
4. **A user with no flashcards at all sees "No flashcards yet"** (a user whose cards are all archived does not, see
   [[flashcard-archiving]]) with two buttons, "Generate with AI"
   (opens the Generate page) and "Create Manually" (opens the create dialog). Backed by: Gherkin
   (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "Empty flashcards state").
5. **The list shows the newest active flashcards first, at most 100.** Archived cards are not in it. Older cards
   beyond the 100 newest are not shown and there is no way to reach them. Backed by: unit test for the order
   (`Fiszki.Tests/Services/FlashcardServiceTests.cs` `ListAsync_ReturnsOrdered`); the limit of 100
   comes from code only (`Components/Pages/Flashcards.razor` `LoadFlashcards`).
6. **Each flashcard shows its question and answer, where it came from and when it was created.**
   The source is "AI Generated" or "Manual". The date is converted with the local-time call on the
   server, so it is expected to be in the **server's** time zone, not the browser's (inferred: the
   page is rendered on the server; not checked by running it). Backed by: Gherkin for seeing existing
   flashcards by their text
   (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "View existing flashcards and statistics");
   the rest from code only.

### Counters and filters

7. **Three counters are shown when the user has at least one card:** "Total Cards", "AI Generated" and
   "Manual". They count **all** the user's cards, archived ones included, from a database count and not from the loaded
   list, so archiving does not change them and they no longer undercount above 100 cards (rule 5 still caps the list).
   Backed by code only (`Fiszki.Services/Services/FlashcardService.cs` `GetSummaryAsync`) and by: Gherkin for the counters on a seeded collection, checking that AI plus
   Manual equals Total
   (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "View existing flashcards and statistics"; that step
   also checks the learning progress and the archived count).
8. **Clicking a counter filters the visible list (active or archived) by where the cards came from.** Clicking "AI Generated" or
   "Manual" shows only those cards and marks that counter with a check; clicking it again, clicking
   "Total Cards" or double-clicking any counter shows all cards again. The counters themselves keep
   their full numbers while a filter is on. Backed by: code only
   (`Components/Pages/Flashcards.razor` `ToggleFilter`, `Components/Pages/Flashcards.razor` `ClearFilter`).
   **No active scenario clicks a filter.**
9. **A filter that matches nothing no longer leaves the user stuck.** Since archiving the "No flashcards yet" state is
   chosen from the user's total number of cards, not from the filtered list. If the chosen filter has no cards (for
   example "AI Generated" with only manual cards) the counters stay and a short message ("No active flashcards to show.")
   replaces the list, so the filter can be cleared. Backed by: code only (`Components/Pages/Flashcards.razor`
   `ApplyFilter`, the empty-list message in the markup); it replaces the old behavior that hid the counters.
9a. **Learning progress and the archived count sit under the counters**, with an "Active" / "Archived (N)" switch. Backed
    by: Gherkin (`Fiszki.FunctionalTests/Features/FlashcardArchiving.feature`, scenario "Archiving raises the learning progress and keeps the counters") and OpenSpec (`openspec/specs/flashcard-archiving/spec.md`, requirement
    "Counters include archived flashcards"). Details are in [[flashcard-archiving]].

### Views and flipping

10. **Two views, card view by default.** The "List View" / "Card View" button switches between a
    grid of cards and a plain list; its label names the view you will switch to. Backed by: Gherkin
    (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "Toggle between card and list view").
11. **Cards can be flipped in card view.** "Show Answer" turns a card to its back and "Show Question"
    turns it back. The flipped state is kept only on the page and is reset when switching views or
    reloading. List view has no flipping and shows question and answer together. Backed by: code only
    (`Components/Pages/Flashcards.razor` `FlipCard`); a scenario exists but is switched off
    (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "Flip flashcards in card view" is tagged `@ignore`).
12. **Card view also shows the source name; list view shows only an icon** (a star for AI, a pencil
    for manual) and a longer creation time. Backed by: code only.

### Buttons and what they do

13. **"Generate More Cards" opens the Generate page; "Add Manual Card" opens the create dialog.** Both
    are shown with the counters. Backed by: Gherkin for the create dialog from the empty state
    (`Fiszki.FunctionalTests/Features/FlashcardsUI.feature`, scenario "Create a manual flashcard from empty state").
14. **Each active card has three buttons: "I learned it", Edit and Delete.** The Edit button does nothing, Delete opens a
    confirmation dialog and "I learned it" opens the archive dialog. Backed by: Gherkin for the button
    (`Fiszki.FunctionalTests/Features/FlashcardArchiving.feature`, scenario "Every active flashcard has an I learned it button in both views"); the rest code only; see [[manual-flashcard-creation]], [[flashcard-deletion]] and [[flashcard-archiving]].
15. **The page does not offer search, sorting, paging or tag display.** Tags typed when creating a
    card are stored but never shown, and the only filter is by source. Backed by: code only (absence).

## User flow

1. The user signs in and the Flashcards page opens with a loading indicator.
2. With no cards the user sees the empty state; otherwise the counters, the buttons and the cards.
3. The user may filter by clicking a counter, switch the view, flip cards, create, delete or start
   generation.

## Implementation

For developers. Layers and wiring: [[application-layers]].

- **Page:** `Components/Pages/Flashcards.razor` (Bootstrap markup). State: `_isLoading`, `_error`,
  `_allFlashcards` (everything loaded for the current view), `_flashcards` (what is shown after the filter),
  `_selectedFilter`, `_isCardView`, `_flippedCards`, `_showArchived` and `_summary`. `LoadFlashcards` reads the user id
  from the `NameIdentifier` claim, calls `IFlashcardService.ListAsync(userId, take: 100, state: ...)` for the active or
  archived view and `IFlashcardService.GetSummaryAsync` for the counters and the progress. The page shows one of four
  states in order: loading, error, empty (when the user has **no flashcards at all**, `_summary.Total == 0`), list.
  An empty visible list with a non-empty collection shows a short message instead of the list.
- **Filter:** `ApplyFilter` builds `_flashcards` from `_allFlashcards` by the creation source text
  ("Ai" or "Manual"); `ToggleFilter` and `ClearFilter` change `_selectedFilter`. The counters are
  taken from `_summary`, so they ignore the filter and the view.
- **Service:** `Fiszki.Services/Services/FlashcardService.cs` `ListAsync` keeps only the user's active cards (or
  archived ones for the Archived view),
  orders them newest first by creation time, applies `skip` (default 0) and `take`, and limits `take`
  to 1 to 500. The page passes 100 and never passes `skip`, so there is no paging. `GetAsync` exists but
  the page does not use it.
- **Data shown:** `Fiszki.Services/Models/FlashcardDto.cs` `FlashcardDto` carries id, question, answer,
  creation source, AI model, tags, both times and the archive time; the page uses the question, answer, creation
  source and creation time (and, in list view of the Archived view, the "Learned" time), and ignores the model, tags and update time.
- **Styles:** `wwwroot/css/flashcards.css` styles the filter cards (hover, selected state, check
  animation). The page links it as `~/css/flashcards.css`, a form from Razor Pages, and it is the only
  place that links it. Whether the browser loads it was not verified.
- **Test selectors:** `flashcards-loading-spinner`, `flashcards-error-alert`, `flashcards-empty-state`,
  `generate-with-ai-button`, `create-manually-button`, `total-cards-stat`, `ai-cards-stat`,
  `manual-cards-stat`, `total-count`, `ai-count`, `manual-count`, `generate-more-cards-button`,
  `add-manual-card-button`, `toggle-view-mode-button`, `flashcards-card-view`, `flashcards-list-view`,
  `flashcard-item` (card view only; list rows have none), `edit-card-button`, `delete-card-button` and the archiving
  selectors listed in [[flashcard-archiving]].

### Test coverage

- **Gherkin** `Fiszki.FunctionalTests/Features/FlashcardsUI.feature` has seven scenarios, six active:
  the empty state, creating a card from the empty state, a validation error in the create dialog,
  seeing seeded cards and the counters, a "filters" scenario that only looks at counters and two more
  cards, and toggling the view. The flip scenario is switched off. Scenarios that need an empty
  collection get a dedicated test user and the others a user with sample flashcards; the choice is made
  by scenario tags in `Fiszki.FunctionalTests/Hooks/SeededTestUserHooks.cs`.
- **Unit tests** (`Fiszki.Tests/Services/FlashcardServiceTests.cs`): the order of the list
  (`ListAsync_ReturnsOrdered`) and that another user's card is not returned
  (`GetAsync_ReturnsNullForWrongUser`).
- **Weak checks:** the counters step passes whenever AI plus Manual equals Total and Total is above
  zero. The step definitions for clicking a filter exist, but no active scenario uses them, and what
  they would assert is only that a count is zero or more
  (`Fiszki.FunctionalTests/Steps/FlashcardSteps.cs` `ThenIShouldSeeOnlyAiGeneratedFlashcards`).
- **Not tested:** the filters (including a filter that matches nothing, rule 9), the limit of 100, the error state, the
  time zone, flipping, and what the Edit button does.

## Dependencies and side effects

- Shows what [[ai-flashcard-generation]] and [[manual-flashcard-creation]] save, and is the only
  place to delete cards, see [[flashcard-deletion]], and to archive and restore them, see [[flashcard-archiving]].
- Reads data described in [[flashcard-entity]].

## Discrepancies

Older notes disagree with the code. The code wins (see SCHEMA, trust hierarchy).

- **Paging, sorting and search.** The historical API plan (api-plan.md in the old `.ai` folder)
  describes listing with pages (default 20, up to 100 per page), sorting by question, creation or
  update time in either direction, a text search over question and answer, filtering by one or more
  tags and by source, and totals for the page count. The page loads the 100 newest cards with none of
  these.
- **Server-side paging in the UI.** The historical UI plans (ui-plan-prompted.md and ui-plan-basic.md
  in the old `.ai` folder) describe server-side pagination with a table or pagination component; the
  page uses plain Bootstrap markup and has no paging.
- **Single card.** The API plan describes fetching one card, with its learning progress; the service
  can fetch one card, but no screen does.

## Related

- [[manual-flashcard-creation]]
- [[flashcard-deletion]]
- [[flashcard-archiving]]
- [[ai-flashcard-generation]]
- [[flashcard-entity]]
- [[flashcard-ownership]]
- [[application-layers]]
- [[glossary]]
