---
title: OpenRouter integration
type: architecture
summary: The minimal OpenRouter chat client Fiszki uses for AI generation: how it is configured and called, which options are actually used, how HTTP errors are mapped, and that there are no retries or rate limiting.
tags: [openrouter, ai, llm, client, http, configuration, api-key, structured-output, json-schema, errors, retry, rate-limit, gpt-4o-mini]
sources:
  - Fiszki.Services/Interfaces/OpenRouter/IOpenRouterChatService.cs
  - Fiszki.Services/Services/OpenRouter/OpenRouterChatService.cs
  - Fiszki.Services/Services/OpenRouter/RateLimiters.cs
  - Fiszki.Services/Models/OpenRouter/OpenRouterOptions.cs
  - Fiszki.Services/Models/OpenRouter/ChatModels.cs
  - Fiszki.Services/Exceptions/OpenRouter/OpenRouterExceptions.cs
  - Fiszki.Services/Services/GenerationService.cs
  - Fiszki.Services/DependencyInjection.cs
  - appsettings.json
  - appsettings.Test.json
  - .github/workflows/ci-cd.yml
  - README.md
verified_commit: 52f627aae9f0b25fa16da531aa2d9e66b421bcb9
confidence: high
---

# OpenRouter integration

## Responsibility

A small client that sends chat requests to the OpenRouter API and returns the text or a parsed
JSON object. It is used by one caller today: the generation service, see
[[ai-flashcard-generation]]. The code calls itself an "MVP version": it supports
non-streaming completion and structured (JSON schema) completion, nothing else.

## Structure

- **Contract:** `Fiszki.Services/Interfaces/OpenRouter/IOpenRouterChatService.cs`
  `IOpenRouterChatService` has `CreateSession`, `SetSystemMessage`, `AddUserMessage`,
  `AddAssistantMessage`, `CreateOptions`, `CompleteAsync` and `CompleteStructuredAsync`.
- **Client:** `Fiszki.Services/Services/OpenRouter/OpenRouterChatService.cs`
  `OpenRouterChatService` implements it with an `HttpClient`.
- **Models:** `Fiszki.Services/Models/OpenRouter/ChatModels.cs` (session, messages, options,
  results) and `Fiszki.Services/Models/OpenRouter/OpenRouterOptions.cs` `OpenRouterOptions`.
- **Errors:** `Fiszki.Services/Exceptions/OpenRouter/OpenRouterExceptions.cs`.

## How a request works

1. The caller creates a session for a model, sets a system message, adds a user message and
   builds options (temperature, maximum tokens, optional JSON schema).
2. `CompleteStructuredAsync` attaches the schema and calls `CompleteAsync`.
3. The client sends one `POST chat/completions` to the base address with a bearer token, an
   application-identifying `User-Agent`, the optional `HTTP-Referer` and `X-Title` headers from
   configuration, and a correlation id. The request asks for a strict JSON-schema response when a
   schema is given. The timeout comes from configuration.
4. The first choice of the response is returned with token usage, finish reason and duration.
   For structured calls the text is parsed into the requested type.

What generation sends: model `openai/gpt-4o-mini`, temperature 0.7, at most 2000 tokens, and a
schema named `flashcard_generation_response` (see
`Fiszki.Services/Services/GenerationService.cs` `GenerateFlashcardsWithOpenRouter`).

## Errors

Any HTTP error is turned into a typed exception
(`Fiszki.Services/Services/OpenRouter/OpenRouterChatService.cs` `HandleErrorResponseAsync`):

| HTTP status | Exception |
|---|---|
| 401, 403 | `OpenRouterAuthException` |
| 404 | `OpenRouterModelNotFoundException` |
| 429 | `OpenRouterRateLimitException` (carries `RetryAfter` when the header is present) |
| 400 | `OpenRouterFatalException` |
| 500, 502, 503, 504 | `OpenRouterTransientException` |
| anything else | `OpenRouterFatalException` |

Other failures (network, timeout) are wrapped in `OpenRouterTransientException`. An empty or
unparsable response is `OpenRouterFatalException`; a structured response that does not fit the
requested type is `OpenRouterSchemaValidationException`. The error body from the API is written
to the log. The caller in the generation flow does not look at the exception type: it treats
every failure the same way and falls back to placeholder cards.

## What is not implemented

- **No retries.** The "transient" and "fatal" exception types exist, but nothing retries.
- **No rate limiting in use.** `Fiszki.Services/Services/OpenRouter/RateLimiters.cs`
  `SemaphoreRateLimiter` exists but is never registered or called.
- **No streaming**, no model registry, no cost calculation: the result's cost is always 0 (a
  TODO in the code).
- **Some options are never read.** `MaxConcurrentRequests`, `EnableStreaming`,
  `EnableStructuredValidation` and `ModelCacheTtl` are defined and set in the settings files, but
  no code uses them (the limiter that would read `MaxConcurrentRequests` is itself unused).
- **No tests** for the real client. The only test that touches it mocks the interface
  (`Fiszki.Tests/Services/GenerationServiceTests.cs`).

## Configuration

Section `OpenRouter`, bound to `Fiszki.Services/Models/OpenRouter/OpenRouterOptions.cs`
`OpenRouterOptions`:

| Key | Used | Meaning |
|---|---|---|
| `ApiKey` | yes | Bearer token. **Required**: the client cannot be created when it is empty |
| `BaseUri` | yes | API base address, default `https://openrouter.ai/api/v1/` |
| `Referer` | yes | Optional `HTTP-Referer` header |
| `Title` | yes | Optional `X-Title` header |
| `RequestTimeout` | yes | Must be positive; default 60 seconds |
| `MaxConcurrentRequests`, `EnableStreaming`, `EnableStructuredValidation`, `ModelCacheTtl` | no | See "What is not implemented" |

The key is never stored in the repository's main settings: `appsettings.json` has it empty, and
`README.md` tells developers to set it with user secrets or the environment variable
`OpenRouter__ApiKey`. The test settings file contains a short value (8 characters) that is not shaped like an
OpenRouter key. CI starts the application in test mode with the key taken from a repository
secret (`.github/workflows/ci-cd.yml`), so the functional tests there run against an application
that can call the real model; a local test-mode run uses the short value instead. The name of the base-address key is `BaseUri`; `README.md` calls
it `BaseUrl`, which the code does not read.

## Wiring

`Fiszki.Services/DependencyInjection.cs` `AddFiszkiServices` binds the options, registers a typed
HTTP client for the class and registers `IOpenRouterChatService` as scoped. See
[[application-layers]].

## Behavior worth knowing (code reading, no tests)

- **No key, no generation page.** The client constructor throws when the key is empty. The
  generation service needs the client, so without a key the Generate page cannot create its
  service. With the short key of the test settings the client is created, the real call is
  rejected as unauthorized, and generation falls back to placeholder cards. Inferred from the
  constructor code; not covered by a test.
- **Large requests may be cut off (inference, not verified).** The response is limited to 2000
  tokens while up to 50 cards, each with an example, can be requested. A response cut off in the
  middle would fail to parse and generation would fall back to placeholder cards.

## Discrepancies

The historical plan for this client (openrouter-service-implementation-plan.md in the old `.ai`
folder) describes a much larger design: streaming, retries with a circuit breaker, a token-bucket
rate limiter, a model registry and cost calculation. The code implements only the minimal
non-streaming and structured completion described above. Also, `Components/Pages/AboutTechnicalities.razor`
tells users the integration has "retry logic"; the code has none. The code wins.

## Related

- [[ai-flashcard-generation]]
- [[generation-job-flow]]
- [[application-layers]]
