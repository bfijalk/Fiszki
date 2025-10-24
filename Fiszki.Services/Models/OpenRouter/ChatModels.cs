namespace Fiszki.Services.Models.OpenRouter;

/// <summary>
/// Metadata associated with a chat message
/// </summary>
public sealed class MessageMetadata
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? MessageId { get; init; }
    public Dictionary<string, object>? Properties { get; init; }
}

/// <summary>
/// Represents a single message in a chat conversation
/// </summary>
public sealed class ChatMessage
{
    public required string Role { get; init; }
    public required object Content { get; init; }
    public MessageMetadata? Metadata { get; init; }
}

/// <summary>
/// Represents a chat session with a collection of messages
/// </summary>
public sealed class ChatSession
{
    private readonly List<ChatMessage> _messages = new();

    public required string Model { get; init; }
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? SessionId { get; init; }

    internal void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
    }

    internal void SetSystemMessage(string content)
    {
        // Remove existing system message if present
        var existingSystemIndex = _messages.FindIndex(m => m.Role == "system");
        if (existingSystemIndex >= 0)
        {
            _messages.RemoveAt(existingSystemIndex);
        }

        // Insert system message at the beginning
        _messages.Insert(0, new ChatMessage
        {
            Role = "system",
            Content = content,
            Metadata = new MessageMetadata()
        });
    }
}

/// <summary>
/// Options for configuring chat completion requests
/// </summary>
public sealed class ChatCompletionOptions
{
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public int? MaxTokens { get; set; }
    public double? PresencePenalty { get; set; }
    public double? FrequencyPenalty { get; set; }
    public string? Seed { get; set; }
    public bool Stream { get; set; }
    public JsonSchemaDescriptor? StructuredSchema { get; set; }
}

/// <summary>
/// Builder for fluent configuration of chat completion options
/// </summary>
public sealed class ChatCompletionOptionsBuilder
{
    private readonly ChatCompletionOptions _options = new();

    public ChatCompletionOptionsBuilder WithTemperature(double temperature)
    {
        _options.Temperature = temperature;
        return this;
    }

    public ChatCompletionOptionsBuilder WithTopP(double topP)
    {
        _options.TopP = topP;
        return this;
    }

    public ChatCompletionOptionsBuilder WithMaxTokens(int maxTokens)
    {
        _options.MaxTokens = maxTokens;
        return this;
    }

    public ChatCompletionOptionsBuilder WithPresencePenalty(double presencePenalty)
    {
        _options.PresencePenalty = presencePenalty;
        return this;
    }

    public ChatCompletionOptionsBuilder WithFrequencyPenalty(double frequencyPenalty)
    {
        _options.FrequencyPenalty = frequencyPenalty;
        return this;
    }

    public ChatCompletionOptionsBuilder WithSeed(string seed)
    {
        _options.Seed = seed;
        return this;
    }

    public ChatCompletionOptionsBuilder WithStreaming(bool stream = true)
    {
        _options.Stream = stream;
        return this;
    }

    public ChatCompletionOptionsBuilder WithStructuredSchema(JsonSchemaDescriptor schema)
    {
        _options.StructuredSchema = schema;
        return this;
    }

    public ChatCompletionOptions Build() => _options;
}

/// <summary>
/// Descriptor for JSON schema used in structured responses
/// </summary>
public sealed class JsonSchemaDescriptor
{
    public required string Name { get; init; }
    public required object SchemaObject { get; init; }
    public string? SystemPromptAugmentation { get; init; }
}

/// <summary>
/// Result of a chat completion request
/// </summary>
public sealed class ChatCompletionResult
{
    public required string Content { get; init; }
    public required string Model { get; init; }
    public required TokenUsage Usage { get; init; }
    public required string RequestId { get; init; }
    public string? FinishReason { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
    public TimeSpan Duration { get; init; }
    public decimal Cost { get; init; }
}

/// <summary>
/// Structured result with validated JSON content
/// </summary>
public sealed class StructuredResult<T>
{
    public required T Data { get; init; }
    public required string RawJson { get; init; }
    public required ChatCompletionResult BaseResult { get; init; }
}

/// <summary>
/// Streaming chunk containing incremental content
/// </summary>
public sealed class ChatStreamChunk
{
    public string? TextDelta { get; init; }
    public string? Role { get; init; }
    public bool IsComplete { get; init; }
    public string? FinishReason { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Token usage information
/// </summary>
public sealed class TokenUsage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
}

/// <summary>
/// Estimate of token usage before making a request
/// </summary>
public sealed class TokenUsageEstimate
{
    public int EstimatedPromptTokens { get; init; }
    public int EstimatedMaxCompletionTokens { get; init; }
    public int TotalTokens => EstimatedPromptTokens + EstimatedMaxCompletionTokens;
    public bool IsApproximate { get; init; } = true;
}

/// <summary>
/// Cost estimate for a chat completion request
/// </summary>
public sealed class CostEstimate
{
    public decimal InputCost { get; init; }
    public decimal OutputCost { get; init; }
    public decimal TotalUsd => InputCost + OutputCost;
    public string Model { get; init; } = string.Empty;
    public bool IsEstimate { get; init; } = true;
}

/// <summary>
/// Information about a model's capabilities
/// </summary>
public sealed class ModelInfo
{
    public required string Name { get; init; }
    public int MaxContextTokens { get; init; }
    public bool SupportsStructured { get; init; }
    public bool SupportsVision { get; init; }
    public bool SupportsStreaming { get; init; }
    public decimal InputPricePer1K { get; init; }
    public decimal OutputPricePer1K { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a tool call in a conversation
/// </summary>
public sealed class ToolCallResult
{
    public required string ToolCallId { get; init; }
    public required string Content { get; init; }
    public bool IsError { get; init; }
}
