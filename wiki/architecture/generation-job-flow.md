---
title: Generation job flow
type: architecture
summary: How an AI generation job runs: an in-memory job per service instance, a background task, polling by the page every 1.5 seconds, the job states, and what cancel, failure and saving really do.
tags: [generation, job, background, polling, status, cancel, queued, generating, completed, failed, in-memory, scoped, circuit]
sources:
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/Interfaces/IGenerationService.cs
  - Fiszki.Services/Models/Generation/GenerationStatusEnum.cs
  - Fiszki.Services/Models/Generation/GenerationStatusDto.cs
  - Fiszki.Services/Models/Generation/GenerationJobDto.cs
  - Fiszki.Services/Exceptions/GenerationException.cs
  - Fiszki.Services/DependencyInjection.cs
  - Components/Pages/GeneratePage.razor.cs
  - Fiszki.Tests/Services/GenerationServiceTests.cs
verified_commit: 5367383718e4da80f89f35abf89c2b78f6f682c8
confidence: high
---

# Generation job flow

## Responsibility

Starting a generation does not wait for the AI. The service creates a **job**, starts the work
in the background and returns at once; the page then asks for the job's status until it is done.
The job exists only in memory. User-facing behavior is in [[ai-flashcard-generation]]; the AI call
itself is in [[openrouter-integration]].

## Structure

```
 GeneratePage                      GenerationService (scoped)
 ------------                      --------------------------
 StartAsync(command) ------------> validate command
                                   create job (Queued), keep in in-memory dictionary
                                   start background task ------> Generating
        <--- job id, Queued                                       call AI (or fallback)
                                                                  Completed (proposals)
 loop every 1.5 s:                                                or Failed
   GetStatusAsync(job id) ------>  read job from the dictionary
        <--- status, proposals
 stop when Completed or Failed
```

- **Job store:** a `ConcurrentDictionary` field of `Fiszki.Services/Services/GenerationService.cs`
  `GenerationService`, keyed by job id. Jobs are never removed.
- **Job states** (`Fiszki.Services/Models/Generation/GenerationStatusEnum.cs`
  `GenerationStatusEnum`): `Idle` (used only by the page before anything starts), `Queued`,
  `Generating`, `Completed`, `Failed`, `Canceled`.
- **Lifetime:** `IGenerationService` is registered as **scoped**
  (`Fiszki.Services/DependencyInjection.cs` `AddFiszkiServices`). In an interactive Blazor Server
  page a scoped service lives as long as the user's connection (circuit), so the jobs live and
  die with it. A reload starts again with an empty store.

## How the pieces behave

- **Start.** `StartAsync` validates the command (see [[ai-flashcard-generation]] for the limits)
  and throws a validation error if it is invalid; no job is created then. Otherwise it returns the
  job id, the queue time and `Queued`.
- **Background work.** The task sets `Generating`, calls the AI, stores the proposals and sets
  `Completed`. If an exception escapes, it sets `Failed` and the message "Failed to generate
  flashcards. Please try again.". The AI call has its own fallback to placeholder cards, so an AI
  error does not reach this `Failed` path.
- **Status.** `GetStatusAsync` returns the state, the number of proposals, the proposals and the
  error text. An unknown job id throws `GenerationException` with "Generation job {id} not found".
- **Polling in the page.** `Components/Pages/GeneratePage.razor.cs` `PollGenerationStatus` waits
  1.5 seconds between calls and stops on `Completed` or `Failed`. It copies the proposals into the
  page state. It does not use the job's error text: an error message on the page comes only from an
  exception thrown by the calls.
- **Cancel.** `CancelAsync` sets the job to `Canceled` (an unknown id is ignored). It does not stop
  the background task, and the task sets `Completed` or `Failed` when it ends, which replaces
  `Canceled`. The page does not call `CancelAsync` at all: its cancel button only stops polling.
- **Saving is separate from the job.** `SaveProposalsAsync` takes the proposals sent by the page,
  not the ones in the job store. The proposal id is reused as the id of the saved flashcard
  (`Fiszki.Services/Mapping/FlashcardMapper.cs` `ToEntity`), so saving the same proposal twice
  would hit a duplicate key (inferred, not tested).

## Interactions

- Calls: [[openrouter-integration]] for the AI request.
- Called by: `Components/Pages/GeneratePage.razor.cs` through `IGenerationService`.
- Writes to the database only in `SaveProposalsAsync`, see [[flashcard-entity]].

## Tests

`Fiszki.Tests/Services/GenerationServiceTests.cs` covers: a started job returns an id with a
queued (or already completed) status, invalid command throws, status of an existing and an unknown job, cancel of an existing job (the
status is `Canceled` right after cancel) and of an unknown job, and the placeholder fallback. It
does not cover the background task finishing after a cancel, the `Failed` path, polling, or job
cleanup.

## Related

- [[ai-flashcard-generation]]
- [[openrouter-integration]]
- [[application-layers]]
