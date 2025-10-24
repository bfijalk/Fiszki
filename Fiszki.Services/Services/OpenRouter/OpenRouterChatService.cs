using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fiszki.Services.Interfaces.OpenRouter;
using Fiszki.Services.Models.OpenRouter;
using Fiszki.Services.Models.OpenRouter.Dtos;
using Fiszki.Services.Exceptions.OpenRouter;

namespace Fiszki.Services.Services.OpenRouter;

/// <summary>
/// OpenRouter chat completion service implementation (MVP version)
/// </summary>
public class OpenRouterChatService : IOpenRouterChatService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterChatService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpenRouterChatService(
        HttpClient httpClient,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterChatService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HTTP client
        ConfigureHttpClient();

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Validate critical options
        ValidateOptions();
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatSession session, 
        ChatCompletionOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        ValidateSession(session);
        options ??= new ChatCompletionOptions();

        var requestPayload = BuildRequestPayload(session, options);
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting OpenRouter completion for model {Model}", session.Model);

            var response = await SendRequestAsync(requestPayload, cancellationToken);
            var responseDto = await DeserializeResponseAsync<ChatCompletionResponseDto>(response, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            var result = MapToCompletionResult(responseDto, duration);

            _logger.LogInformation("OpenRouter completion completed in {Duration}ms. Tokens: {Tokens}", 
                duration.TotalMilliseconds, result.Usage.TotalTokens);

            return result;
        }
        catch (Exception ex) when (!(ex is OpenRouterException))
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "OpenRouter completion failed after {Duration}ms", duration.TotalMilliseconds);
            throw new OpenRouterTransientException("Failed to complete chat request", ex, duration: duration);
        }
    }

    public async Task<StructuredResult<T>> CompleteStructuredAsync<T>(
        ChatSession session, 
        JsonSchemaDescriptor schema, 
        ChatCompletionOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        ValidateSession(session);
        options ??= new ChatCompletionOptions();
        options.StructuredSchema = schema;

        var baseResult = await CompleteAsync(session, options, cancellationToken);

        try
        {
            var deserializedData = JsonSerializer.Deserialize<T>(baseResult.Content, _jsonOptions);
            if (deserializedData == null)
            {
                throw new OpenRouterSchemaValidationException(
                    "Failed to deserialize structured response", 
                    new[] { "Deserialized data is null" },
                    baseResult.RequestId);
            }

            return new StructuredResult<T>
            {
                Data = deserializedData,
                RawJson = baseResult.Content,
                BaseResult = baseResult
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize structured response for schema {SchemaName}", schema.Name);
            throw new OpenRouterSchemaValidationException(
                $"Failed to deserialize response according to schema '{schema.Name}'", 
                new[] { ex.Message },
                baseResult.RequestId);
        }
    }

    public ChatSession CreateSession(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be null or empty", nameof(model));

        return new ChatSession
        {
            Model = model,
            SessionId = Guid.NewGuid().ToString()
        };
    }

    public void SetSystemMessage(ChatSession session, string content)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));

        session.SetSystemMessage(content);
        _logger.LogDebug("System message set for session {SessionId}", session.SessionId);
    }

    public void AddUserMessage(ChatSession session, string content, MessageMetadata? metadata = null)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));

        session.AddMessage(new ChatMessage
        {
            Role = "user",
            Content = content,
            Metadata = metadata ?? new MessageMetadata()
        });

        _logger.LogDebug("User message added to session {SessionId}", session.SessionId);
    }

    public void AddAssistantMessage(ChatSession session, string content, MessageMetadata? metadata = null)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));

        session.AddMessage(new ChatMessage
        {
            Role = "assistant",
            Content = content,
            Metadata = metadata ?? new MessageMetadata()
        });

        _logger.LogDebug("Assistant message added to session {SessionId}", session.SessionId);
    }

    public ChatCompletionOptions CreateOptions(Action<ChatCompletionOptionsBuilder> build)
    {
        if (build == null) throw new ArgumentNullException(nameof(build));

        var builder = new ChatCompletionOptionsBuilder();
        build(builder);
        return builder.Build();
    }

    private void ConfigureHttpClient()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = _options.BaseUri;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FiszkiOpenRouterClient/1.0");

        if (!string.IsNullOrWhiteSpace(_options.Referer))
        {
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", new[] { _options.Referer });
        }

        if (!string.IsNullOrWhiteSpace(_options.Title))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Title", new[] { _options.Title });
        }

        _httpClient.Timeout = _options.RequestTimeout;
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ArgumentException("OpenRouter API key is required", nameof(_options.ApiKey));
        }

        if (_options.RequestTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentException("Request timeout must be positive", nameof(_options.RequestTimeout));
        }
    }

    private void ValidateSession(ChatSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(session.Model))
            throw new ArgumentException("Session model cannot be null or empty");

        if (!session.Messages.Any())
            throw new ArgumentException("Session must contain at least one message");
    }

    private ChatCompletionRequestDto BuildRequestPayload(ChatSession session, ChatCompletionOptions options)
    {
        var messages = session.Messages.Select(m => new ChatMessageDto(m.Role, m.Content)).ToList();

        ResponseFormatDto? responseFormat = null;
        if (options.StructuredSchema != null)
        {
            responseFormat = new ResponseFormatDto(
                "json_schema",
                new JsonSchemaEnvelope(
                    options.StructuredSchema.Name,
                    true,
                    options.StructuredSchema.SchemaObject
                )
            );
        }

        return new ChatCompletionRequestDto(
            session.Model,
            messages,
            options.Stream ? true : null,
            options.Temperature,
            options.TopP,
            options.MaxTokens,
            options.PresencePenalty,
            options.FrequencyPenalty,
            options.Seed,
            responseFormat
        );
    }

    private async Task<HttpResponseMessage> SendRequestAsync(ChatCompletionRequestDto payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var correlationId = Guid.NewGuid().ToString();
        content.Headers.Add("X-Correlation-Id", correlationId);

        _logger.LogDebug("Sending OpenRouter request with correlation ID {CorrelationId}", correlationId);

        var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, correlationId);
        }

        return response;
    }

    private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            if (result == null)
            {
                throw new OpenRouterFatalException("Received null response from OpenRouter API");
            }
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize OpenRouter response: {Response}", responseContent);
            throw new OpenRouterFatalException("Failed to parse OpenRouter API response", ex);
        }
    }

    private async Task HandleErrorResponseAsync(HttpResponseMessage response, string correlationId)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;

        _logger.LogError("OpenRouter API error {StatusCode}: {Error} (Correlation: {CorrelationId})", 
            statusCode, errorContent, correlationId);

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
            case System.Net.HttpStatusCode.Forbidden:
                throw new OpenRouterAuthException($"Authentication failed: {errorContent}", correlationId);

            case System.Net.HttpStatusCode.NotFound:
                throw new OpenRouterModelNotFoundException($"Model not found: {errorContent}", requestId: correlationId);

            case System.Net.HttpStatusCode.TooManyRequests:
                var retryAfter = response.Headers.RetryAfter?.Delta;
                throw new OpenRouterRateLimitException($"Rate limit exceeded: {errorContent}", retryAfter, correlationId);

            case System.Net.HttpStatusCode.BadRequest:
                throw new OpenRouterFatalException($"Bad request: {errorContent}", requestId: correlationId);

            case System.Net.HttpStatusCode.InternalServerError:
            case System.Net.HttpStatusCode.BadGateway:
            case System.Net.HttpStatusCode.ServiceUnavailable:
            case System.Net.HttpStatusCode.GatewayTimeout:
                throw new OpenRouterTransientException($"Server error: {errorContent}", requestId: correlationId);

            default:
                throw new OpenRouterFatalException($"Unexpected error {statusCode}: {errorContent}", requestId: correlationId);
        }
    }

    private static ChatCompletionResult MapToCompletionResult(ChatCompletionResponseDto response, TimeSpan duration)
    {
        var firstChoice = response.Choices.FirstOrDefault();
        if (firstChoice == null)
        {
            throw new OpenRouterFatalException("No choices returned in response");
        }

        var content = firstChoice.Message.Content?.ToString() ?? string.Empty;
        var usage = response.Usage != null 
            ? new TokenUsage
            {
                PromptTokens = response.Usage.PromptTokens,
                CompletionTokens = response.Usage.CompletionTokens,
                TotalTokens = response.Usage.TotalTokens
            }
            : new TokenUsage();

        return new ChatCompletionResult
        {
            Content = content,
            Model = response.Model,
            Usage = usage,
            RequestId = response.Id,
            FinishReason = firstChoice.FinishReason,
            Duration = duration,
            Cost = 0 // TODO: Calculate cost based on model pricing
        };
    }
}
