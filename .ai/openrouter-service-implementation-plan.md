# OpenRouter Service Implementation Guide

## 1. Service Description
The OpenRouter Service encapsulates interaction with the OpenRouter API for Large Language Model (LLM) chat completions within a .NET 8 (C#) Blazor Server / ASP.NET Core application. It provides:
- Unified request construction for system/user/assistant/tool messages.
- Support for structured JSON responses via `response_format` with strict JSON Schema validation.
- Model selection, model parameter configuration, and capability introspection.
- Optional streaming handling (SSE / chunked) and non-stream completion flows.
- Resilience (retry, timeout, circuit breaker), rate limiting, and error mapping.
- Observability (logging, metrics, tracing) and cost/token accounting.
- Security (API key management, request sanitization, PII minimization, schema-based validation).

The service exposes a simple, task-oriented API: build a chat session, add messages, request completion (stream or non-stream), optionally parse structured result, and handle errors gracefully.

## 2. Constructor Description
Primary concrete class: `OpenRouterChatService` implementing `IOpenRouterChatService`.

Constructor responsibilities:
- Accept dependencies via DI: `HttpClient`, configuration/options (`IOptions<OpenRouterOptions>`), logger, metric collector, validator, retry policy provider, JSON schema validator, clock.
- Configure base address (`https://openrouter.ai/api/v1/`) if not set.
- Attach default headers (`Authorization`, `HTTP-Referer`, `X-Title`, user-agent) without leaking secrets.
- Preload or lazily initialize model capability registry.
- Prepare compiled delegates for serialization/deserialization (performance optimization).
- Validate critical options (API key present, timeouts sane).

Recommended constructor signature:
```csharp
public OpenRouterChatService(
    HttpClient httpClient,
    IOptions<OpenRouterOptions> options,
    ILogger<OpenRouterChatService> logger,
    IOpenRouterRetryPolicyProvider retryPolicies,
    IOpenRouterRateLimiter rateLimiter,
    IJsonSchemaValidator schemaValidator,
    ITokenEstimator tokenEstimator,
    ICostCalculator costCalculator,
    OpenRouterModelRegistry modelRegistry,
    TimeProvider timeProvider)
```
Where `HttpClient` is typed using `IHttpClientFactory` (`services.AddHttpClient<OpenRouterChatService>()`).

## 3. Public Methods and Fields
Interface `IOpenRouterChatService` (async-first design):

### Core Chat Operations
1. `Task<ChatCompletionResult> CompleteAsync(ChatSession session, ChatCompletionOptions? options = null, CancellationToken ct = default)`
   - Non-stream completion returning full response, structured payload if requested.
2. `IAsyncEnumerable<ChatStreamChunk> StreamAsync(ChatSession session, ChatCompletionOptions? options = null, CancellationToken ct = default)`
   - Streaming completion enumerating incremental deltas.
3. `Task<StructuredResult<T>> CompleteStructuredAsync<T>(ChatSession session, JsonSchemaDescriptor schema, ChatCompletionOptions? options = null, CancellationToken ct = default)`
   - Requests response with `response_format` and validates JSON into `T`.
4. `Task<ModelInfo> GetModelInfoAsync(string model, CancellationToken ct = default)`
   - Returns capabilities (max tokens, supports tools, json schema strictness, streaming availability).
5. `Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken ct = default)`
   - Cached list retrieval.

### Session / Builder Utilities
6. `ChatSession CreateSession(string model)`
   - Creates a session with system message placeholder.
7. `void SetSystemMessage(ChatSession session, string content)`
8. `void AddUserMessage(ChatSession session, string content, MessageMetadata? meta = null)`
9. `void AddAssistantMessage(ChatSession session, string content, MessageMetadata? meta = null)`
10. `void AddToolMessage(ChatSession session, ToolCallResult toolResult)`
11. `void AddImageUrl(ChatSession session, string url, string altText)` (multi-modal extension).

### Configuration / Parameters
12. `ChatCompletionOptions CreateOptions(Action<ChatCompletionOptionsBuilder> build)`
13. `Task<bool> ValidateSchemaAsync(JsonSchemaDescriptor schema, CancellationToken ct = default)`
14. `TokenUsageEstimate EstimateTokenUsage(ChatSession session, ChatCompletionOptions? options = null)`
15. `CostEstimate EstimateCost(ChatSession session, ChatCompletionOptions? options = null)`

### Diagnostics
16. `OpenRouterHealthStatus GetHealthSnapshot()`
17. `Task<OpenRouterQuotaStatus> GetQuotaStatusAsync(CancellationToken ct = default)`

### Public Immutable Data (Properties)
- `OpenRouterOptions Options { get; }`
- `OpenRouterModelRegistry ModelRegistry { get; }`

## 4. Private Methods and Fields
Implementation details (prefix `_` for private fields):

### Private Fields
- `_httpClient`: Configured HTTP client.
- `_logger`: Logging.
- `_retryPolicies`: Provides Polly `AsyncPolicy` composites.
- `_rateLimiter`: Token bucket / sliding window.
- `_schemaValidator`: Strict JSON schema validator.
- `_tokenEstimator`: Heuristic or model-specific estimator.
- `_costCalculator`: Pricing logic per model (cached table).
- `_modelRegistry`: Capabilities & pricing per model.
- `_options`: Bound options.
- `_timeProvider`: For time abstraction.

### Private Methods
1. `BuildRequestPayload(ChatSession session, ChatCompletionOptions options, JsonSchemaDescriptor? schema)`
   - Constructs request DTO including messages, `model`, parameters, `response_format` if present.
2. `NormalizeMessages(ChatSession session)`
   - Ensures system message at index 0; merges consecutive same-role messages if allowed.
3. `ValidateSession(ChatSession session)`
   - Schema independent: presence of system + user messages, size limits, attachments compliance.
4. `AttachHeaders(HttpRequestMessage req)`
   - Adds dynamic headers (trace id, correlation id).
5. `SendWithResilienceAsync(HttpRequestMessage req, CancellationToken ct)`
   - Wraps rate limit + retry + timeout + circuit breaker.
6. `ParseCompletionResponseAsync(HttpResponseMessage response, bool structured, JsonSchemaDescriptor? schema, CancellationToken ct)`
7. `HandleErrorResponseAsync(HttpResponseMessage response)`
   - Maps API errors to domain exceptions.
8. `ParseStreamChunks(Stream stream, CancellationToken ct)`
   - Parses SSE lines into `ChatStreamChunk` objects.
9. `AccumulateAndFinalizeStream(List<ChatStreamChunk> chunks)`
10. `ValidateAndDeserializeStructured<T>(string rawJson, JsonSchemaDescriptor schema)`
11. `ComputeTokenUsage(RawCompletionPayload payload)`
12. `ComputeCost(TokenUsage usage, string model)`
13. `TryRefreshModelsAsync()` (background refresh logic).
14. `IsRetryable(OpenRouterError error)`
15. `ObfuscateForLog(string content)` (mask secrets / PII heuristics)

## 5. Error Handling
Enumerated scenarios:
1. Network failures (DNS, connection refused, timeout).
2. HTTP status errors (400 validation, 401/403 auth, 404 model, 429 rate limit, 5xx server).
3. Malformed JSON or unexpected structure.
4. Schema validation failure (strict mode mismatch).
5. Streaming interruption (broken connection mid-chunk).
6. Rate limiter exhaustion.
7. Circuit breaker open state.
8. Cancellation requested by caller.
9. Exceeded max tokens / message size constraints.
10. Unsupported model capabilities (e.g., schema not supported).
11. Concurrency limit exceeded (service-level throttle).
12. Deserialization exceptions (numeric overflows, enum mismatch).
13. Clock skew / timestamp errors in signed headers (if future extension).

Mapped strategy:
- Custom exception types: `OpenRouterAuthException`, `OpenRouterRateLimitException`, `OpenRouterModelNotFoundException`, `OpenRouterSchemaValidationException`, `OpenRouterStreamInterruptedException`, `OpenRouterTransientException`, `OpenRouterFatalException`.
- Use pattern matching on status codes + error code field to select domain exception.
- Retry only when transient (429 with retry-after, 500/502/503/504, network timeouts) respecting exponential backoff + jitter; never retry on 400/401/403/404.
- Streaming errors: attempt graceful close; if partial content salvage tokens consumed.
- Provide `ErrorContext` object inside exceptions (RequestId, Model, Elapsed, Attempt, RawPayloadFragment).

## 6. Security Considerations
1. API Key Storage: stored in `OpenRouterOptions.ApiKey` via user secrets / environment variable; never written to logs.
2. Outbound Filtering: sanitize system/user messages to remove secrets; optional allowlist of tool function names.
3. Least Privilege: `HttpClient` scope limited to OpenRouter base URL.
4. TLS Enforcement: HTTPS only; reject downgrade attempts.
5. PII Minimization: Provide configurable redaction patterns (email, phone, credit cards) before logging.
6. Structured Response Validation: Strict JSON schema prevents injection via dynamic properties.
7. Rate Limiting: Prevent abuse / unexpected cost explosion.
8. Tamper Detection: Compare returned `model` to requested model; raise error if mismatch.
9. Timeouts: Prevent resource exhaustion with controlled `HttpClient.Timeout` + per-request cancellation tokens.
10. Safe Streaming: Surface partial results only after integrity checks (SSE event completeness).
11. Dependency Hardening: Pin packages (Polly, System.Text.Json); update regularly.
12. Outbound Headers: Avoid untrusted user input in `X-Title` or `HTTP-Referer` headers.

## 7. Step-by-Step Implementation Plan

### Step 1: Define Option Classes
Create `OpenRouterOptions`:
```csharp
public sealed class OpenRouterOptions {
    public string ApiKey { get; init; } = string.Empty;
    public Uri? BaseUri { get; init; } = new("https://openrouter.ai/api/v1/");
    public string? Referer { get; init; } // optional
    public string? Title { get; init; } // optional
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(60);
    public int MaxConcurrentRequests { get; init; } = 8;
    public bool EnableStreaming { get; init; } = true;
    public bool EnableStructuredValidation { get; init; } = true;
    public TimeSpan ModelCacheTtl { get; init; } = TimeSpan.FromMinutes(30);
}
```
Register with configuration (`builder.Services.Configure<OpenRouterOptions>(builder.Configuration.GetSection("OpenRouter"));`).

### Step 2: Define Core DTOs
Create request/response shapes (aligning OpenRouter chat completions).

`ChatMessageDto`:
```csharp
public sealed record ChatMessageDto(string Role, object Content); // Content: string | array of parts (text,image_url)
```
`ChatCompletionRequestDto`:
```csharp
public sealed record ChatCompletionRequestDto(
    string Model,
    IReadOnlyList<ChatMessageDto> Messages,
    bool? Stream = null,
    double? Temperature = null,
    double? TopP = null,
    int? MaxTokens = null,
    double? PresencePenalty = null,
    double? FrequencyPenalty = null,
    string? Seed = null,
    ResponseFormatDto? ResponseFormat = null
);
```
`ResponseFormatDto` (strict JSON schema):
```csharp
public sealed record ResponseFormatDto(
    string Type,
    JsonSchemaEnvelope Json_Schema // note JSON property name mapping
);
public sealed record JsonSchemaEnvelope(
    string Name,
    bool Strict,
    object Schema // Raw schema object
);
```
`ChatCompletionResponseDto` capturing usage + choices.

### Step 3: Define Domain Models
`ChatSession` holds list of domain `ChatMessage` objects with richer metadata (timestamps, id, classification tags).
`ChatCompletionOptions` enumerates tunable parameters; builder pattern for fluent config:
```csharp
public sealed class ChatCompletionOptions {
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public int? MaxTokens { get; set; }
    public double? PresencePenalty { get; set; }
    public double? FrequencyPenalty { get; set; }
    public string? Seed { get; set; }
    public bool Stream { get; set; }
    public JsonSchemaDescriptor? StructuredSchema { get; set; }
}
```

### Step 4: Interface Definition
Create `IOpenRouterChatService` with methods listed in Section 3.

### Step 5: Implement Resilience Policies
Use Polly to define:
- Retry: `WaitAndRetryAsync` (max 3 attempts, exponential + jitter) for transient.
- Circuit Breaker: break after e.g. 5 failures / 30s.
- Timeout: `TimeoutPolicy` < global `RequestTimeout`.
Compose via `PolicyWrap` from `IOpenRouterRetryPolicyProvider`.

### Step 6: Rate Limiting & Concurrency
Implement `IOpenRouterRateLimiter` (token bucket or `SemaphoreSlim`). Acquire before sending; release after completion. Distinguish streaming (holds slot longer) vs non-stream.

### Step 7: Request Construction
`BuildRequestPayload` tasks:
1. Normalize messages: map domain roles to API roles (`system`, `user`, `assistant`, `tool`).
2. Convert multi-modal messages: for image: part object `{ "type": "image_url", "image_url": { "url": "..." } }` plus text parts.
3. Add `response_format` when `StructuredSchema` present:
```csharp
var responseFormat = new ResponseFormatDto(
    Type: "json_schema",
    Json_Schema: new JsonSchemaEnvelope(
        Name: schema.Name,
        Strict: true,
        Schema: schema.SchemaObject // pre-validated
    )
);
```
4. Populate parameters only if not null.
5. Ensure system message appears first; if missing, fallback to default (configurable template).

### Step 8: Headers & Authentication
In `AttachHeaders`:
```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
if (!string.IsNullOrWhiteSpace(_options.Referer)) request.Headers.Add("HTTP-Referer", _options.Referer);
if (!string.IsNullOrWhiteSpace(_options.Title)) request.Headers.Add("X-Title", _options.Title);
request.Headers.UserAgent.ParseAdd("FiszkiOpenRouterClient/1.0");
```
Include correlation id header `X-Correlation-Id` sourced from `Activity.Current?.Id` or generated GUID.

### Step 9: Sending Request
`SendWithResilienceAsync`:
- Acquire rate limit token.
- Build `HttpRequestMessage` (POST to `chat/completions`).
- Serialize DTO with `System.Text.Json` using `JsonSerializerOptions` (camelCase, ignore nulls).
- Execute with `PolicyWrap` and cancellation token.
- For streaming: set `Stream = true`; parse SSE via `ParseStreamChunks`.

### Step 10: Parsing Response
For non-stream:
- Deserialize `ChatCompletionResponseDto`.
- Extract `choices[0].message.content` (string or aggregated parts).
- Map usage (`prompt_tokens`, `completion_tokens`, `total_tokens`).
- Compute cost via `_costCalculator` using model registry pricing.
- If structured: call `ValidateAndDeserializeStructured<T>`.

For stream:
- SSE events often with `data:` prefix; ignore `[DONE]` sentinel.
- Accumulate partial JSON fragments; yield `ChatStreamChunk` with delta text or structured partial.
- On completion aggregate entire structured output and validate if applicable.

### Step 11: Structured Response Validation
`JsonSchemaDescriptor` holds: `Name`, `SchemaObject` (e.g., strongly typed C# POCO -> schema), maybe `SystemPromptAugmentation`.
Validation steps:
1. Raw string -> `JsonDocument`.
2. `_schemaValidator.Validate(schema, document)` returns detailed error list.
3. If errors and `Strict=true`, throw `OpenRouterSchemaValidationException` with path details.
4. Deserialize into `T` using `JsonSerializer.Deserialize<T>(raw)`. Optionally add `SourceJson` property.

### Step 12: Token & Cost Estimation
Before sending: `EstimateTokenUsage` approximates input tokens (use model-specific encoding metadata). Provide conservative buffer for `MaxTokens`. After response: finalize actual usage.
Cost calculation: pricing map (per 1K tokens input/output). Compute `inputCost = prompt_tokens/1000 * inPrice`, `outputCost = completion_tokens/1000 * outPrice`.

### Step 13: Model Registry
`OpenRouterModelRegistry` loaded either statically via config or dynamic fetch from `models` endpoint. Includes: `Name`, `MaxContextTokens`, `SupportsStructured`, `SupportsVision`, `InputPricePer1K`, `OutputPricePer1K`.
Refresh in background on TTL expiry (fire-and-forget safe exception handling).

### Step 14: Logging & Metrics
Use structured logging:
- Info: model, duration, tokens, cost, streaming vs non-stream.
- Debug: truncated prompt (first N chars) after redaction.
- Error: exception + `ErrorContext`.
Metrics counters: `openrouter_requests_total`, histogram `openrouter_latency_seconds`, `openrouter_tokens_total`, `openrouter_cost_usd_total`, `openrouter_errors_total{type=...}`.

### Step 15: Example Usages (Numbered)
1. System Message Configuration:
```csharp
var session = service.CreateSession("meta-llama/llama-3.1-70b-instruct");
service.SetSystemMessage(session, "You are a concise assistant helping with Polish flashcards.");
```
2. User Message:
```csharp
service.AddUserMessage(session, "Generate 5 flashcards for Polish verbs in JSON.");
```
3. Structured Response Format (Schema):
```csharp
var schema = new JsonSchemaDescriptor(
    Name: "flashcards_response",
    SchemaObject: new {
        type = "object",
        properties = new {
            flashcards = new {
                type = "array",
                items = new {
                    type = "object",
                    properties = new {
                        term = new { type = "string" },
                        definition = new { type = "string" },
                        example = new { type = "string" }
                    },
                    required = new [] { "term", "definition" }
                }
            }
        },
        required = new [] { "flashcards" }
    }
);
var options = service.CreateOptions(o => {
    o.Temperature = 0.4;
    o.MaxTokens = 800;
    o.StructuredSchema = schema;
});
var result = await service.CompleteStructuredAsync<FlashcardSet>(session, schema, options);
```
4. Model Name Selection:
```csharp
var modelInfo = await service.GetModelInfoAsync("meta-llama/llama-3.1-70b-instruct");
if (!modelInfo.SupportsStructured) throw new InvalidOperationException("Model lacks structured output support.");
```
5. Model Parameters (Temperature + Penalties):
```csharp
var options2 = service.CreateOptions(o => {
    o.Temperature = 0.2;
    o.PresencePenalty = 0.1;
    o.FrequencyPenalty = 0.0;
    o.MaxTokens = 600;
});
var response = await service.CompleteAsync(session, options2);
```
6. Streaming:
```csharp
var streamOptions = service.CreateOptions(o => {
    o.Stream = true;
    o.Temperature = 0.5;
});
await foreach (var chunk in service.StreamAsync(session, streamOptions)) {
    Console.Write(chunk.TextDelta);
}
```
7. Response Format Raw Shape (for clarity):
```json
{
  "response_format": {
    "type": "json_schema",
    "json_schema": {
      "name": "flashcards_response",
      "strict": true,
      "schema": {
        "type": "object",
        "properties": {
          "flashcards": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "term": { "type": "string" },
                "definition": { "type": "string" },
                "example": { "type": "string" }
              },
              "required": ["term", "definition"]
            }
          }
        },
        "required": ["flashcards"]
      }
    }
  }
}
```
8. Cost Estimation:
```csharp
var usageEstimate = service.EstimateTokenUsage(session, options);
var costEstimate = service.EstimateCost(session, options);
_logger.LogInformation("Estimated tokens {total} cost ${cost}", usageEstimate.TotalTokens, costEstimate.TotalUsd);
```

### Step 16: Testing Strategy
- Unit Tests: message normalization, schema validation success/fail, retry logic (mock transient error), cost calculation.
- Integration Tests: mock OpenRouter API via `HttpMessageHandler` test double; streaming SSE simulation.
- Load Tests: concurrency limit respected; rate limiter tokens consumed.
- Security Tests: ensure API key not logged; redaction patterns applied.

### Step 17: Dependency Injection Registration
```csharp
services.AddHttpClient<OpenRouterChatService>((sp, client) => {
    var opts = sp.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
    client.BaseAddress = opts.BaseUri;
    client.Timeout = opts.RequestTimeout;
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler {
    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});
services.AddSingleton<OpenRouterModelRegistry>();
services.AddSingleton<ITokenEstimator, TokenEstimator>();
services.AddSingleton<ICostCalculator, CostCalculator>();
services.AddSingleton<IJsonSchemaValidator, SystemTextJsonSchemaValidator>();
services.AddSingleton<IOpenRouterRetryPolicyProvider, PollyRetryPolicyProvider>();
services.AddSingleton<IOpenRouterRateLimiter, SemaphoreRateLimiter>();
services.AddScoped<IOpenRouterChatService, OpenRouterChatService>();
```

### Step 18: Observability Integration
- Add logging scopes: `using (_logger.BeginScope(new { Model = options.Model, CorrelationId = corrId })) { ... }`
- Emit metrics after completion.
- Include Activity tracing: start `Activity` named `OpenRouter.ChatCompletion`.

### Step 19: Validation Hooks
Before send:
- `ValidateSession` -> domain-level.
- If structured: `ValidateSchemaAsync` ensures schema meets minimal invariants (`type=object`, has `required`).
- Check `MaxTokens` <= model `MaxContextTokens - promptEstimatedTokens`.

### Step 20: Failure Fallbacks
- On streaming failure mid-way: attempt to finalize partial text by concatenating deltas; mark result `IsPartial=true`.
- On schema validation failure: return exception OR if `Strict=false` (future flag) degrade to raw text.
- On model unsupported structured attempt: fallback to unstructured with warning log.

### Step 21: Versioning & Backwards Compatibility
Expose service version via constant. If OpenRouter changes API fields, maintain adapter layer; keep public DTOs stable, use internal mappers.

### Step 22: Documentation & Samples
Add README examples (some provided above) and XML doc comments on interface methods including parameter semantics and exceptions thrown.

---
This guide provides a full blueprint for implementing a robust, secure, and maintainable OpenRouter integration aligned with .NET best practices.

