using System.Text.Json.Serialization;

namespace Fiszki.Services.Models.OpenRouter.Dtos;

/// <summary>
/// DTO for chat message in OpenRouter API format
/// </summary>
public sealed record ChatMessageDto(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] object Content
);

/// <summary>
/// DTO for response format configuration with JSON schema support
/// </summary>
public sealed record ResponseFormatDto(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("json_schema")] JsonSchemaEnvelope JsonSchema
);

/// <summary>
/// Envelope for JSON schema configuration in structured responses
/// </summary>
public sealed record JsonSchemaEnvelope(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("strict")] bool Strict,
    [property: JsonPropertyName("schema")] object Schema
);

/// <summary>
/// DTO for chat completion request to OpenRouter API
/// </summary>
public sealed record ChatCompletionRequestDto(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessageDto> Messages,
    [property: JsonPropertyName("stream")] bool? Stream = null,
    [property: JsonPropertyName("temperature")] double? Temperature = null,
    [property: JsonPropertyName("top_p")] double? TopP = null,
    [property: JsonPropertyName("max_tokens")] int? MaxTokens = null,
    [property: JsonPropertyName("presence_penalty")] double? PresencePenalty = null,
    [property: JsonPropertyName("frequency_penalty")] double? FrequencyPenalty = null,
    [property: JsonPropertyName("seed")] string? Seed = null,
    [property: JsonPropertyName("response_format")] ResponseFormatDto? ResponseFormat = null
);

/// <summary>
/// DTO for token usage information in API responses
/// </summary>
public sealed record TokenUsageDto(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens
);

/// <summary>
/// DTO for chat completion choice in API response
/// </summary>
public sealed record ChatCompletionChoiceDto(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] ChatMessageDto Message,
    [property: JsonPropertyName("finish_reason")] string? FinishReason
);

/// <summary>
/// DTO for chat completion response from OpenRouter API
/// </summary>
public sealed record ChatCompletionResponseDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("choices")] IReadOnlyList<ChatCompletionChoiceDto> Choices,
    [property: JsonPropertyName("usage")] TokenUsageDto? Usage
);

/// <summary>
/// DTO for streaming chunk delta content
/// </summary>
public sealed record StreamDeltaDto(
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("role")] string? Role
);

/// <summary>
/// DTO for streaming chunk choice
/// </summary>
public sealed record StreamChoiceDto(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("delta")] StreamDeltaDto Delta,
    [property: JsonPropertyName("finish_reason")] string? FinishReason
);

/// <summary>
/// DTO for streaming chat completion chunk
/// </summary>
public sealed record ChatStreamChunkDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("choices")] IReadOnlyList<StreamChoiceDto> Choices
);
