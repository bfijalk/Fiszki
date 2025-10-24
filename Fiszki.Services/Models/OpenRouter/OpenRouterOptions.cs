namespace Fiszki.Services.Models.OpenRouter;

/// <summary>
/// Configuration options for the OpenRouter service
/// </summary>
public sealed class OpenRouterOptions
{
    /// <summary>
    /// API key for OpenRouter authentication
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Base URI for OpenRouter API
    /// </summary>
    public Uri? BaseUri { get; init; } = new("https://openrouter.ai/api/v1/");

    /// <summary>
    /// Optional HTTP referer header value
    /// </summary>
    public string? Referer { get; init; }

    /// <summary>
    /// Optional application title for X-Title header
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Request timeout duration
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Maximum number of concurrent requests
    /// </summary>
    public int MaxConcurrentRequests { get; init; } = 8;

    /// <summary>
    /// Whether streaming is enabled
    /// </summary>
    public bool EnableStreaming { get; init; } = true;

    /// <summary>
    /// Whether structured response validation is enabled
    /// </summary>
    public bool EnableStructuredValidation { get; init; } = true;

    /// <summary>
    /// Time-to-live for cached model information
    /// </summary>
    public TimeSpan ModelCacheTtl { get; init; } = TimeSpan.FromMinutes(30);
}
