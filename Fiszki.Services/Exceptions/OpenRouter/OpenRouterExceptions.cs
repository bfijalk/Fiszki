namespace Fiszki.Services.Exceptions.OpenRouter;

/// <summary>
/// Base exception for all OpenRouter-related errors
/// </summary>
public abstract class OpenRouterException : Exception
{
    public string? RequestId { get; }
    public string? Model { get; }
    public TimeSpan? Duration { get; }

    protected OpenRouterException(string message, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message)
    {
        RequestId = requestId;
        Model = model;
        Duration = duration;
    }

    protected OpenRouterException(string message, Exception innerException, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message, innerException)
    {
        RequestId = requestId;
        Model = model;
        Duration = duration;
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class OpenRouterAuthException : OpenRouterException
{
    public OpenRouterAuthException(string message, string? requestId = null) 
        : base(message, requestId) { }
}

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class OpenRouterRateLimitException : OpenRouterException
{
    public TimeSpan? RetryAfter { get; }

    public OpenRouterRateLimitException(string message, TimeSpan? retryAfter = null, string? requestId = null) 
        : base(message, requestId)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Exception thrown when model is not found
/// </summary>
public class OpenRouterModelNotFoundException : OpenRouterException
{
    public OpenRouterModelNotFoundException(string message, string? model = null, string? requestId = null) 
        : base(message, requestId, model) { }
}

/// <summary>
/// Exception thrown when JSON schema validation fails
/// </summary>
public class OpenRouterSchemaValidationException : OpenRouterException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public OpenRouterSchemaValidationException(string message, IReadOnlyList<string> validationErrors, string? requestId = null) 
        : base(message, requestId)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown for transient errors that can be retried
/// </summary>
public class OpenRouterTransientException : OpenRouterException
{
    public OpenRouterTransientException(string message, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message, requestId, model, duration) { }

    public OpenRouterTransientException(string message, Exception innerException, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message, innerException, requestId, model, duration) { }
}

/// <summary>
/// Exception thrown for fatal errors that should not be retried
/// </summary>
public class OpenRouterFatalException : OpenRouterException
{
    public OpenRouterFatalException(string message, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message, requestId, model, duration) { }

    public OpenRouterFatalException(string message, Exception innerException, string? requestId = null, string? model = null, TimeSpan? duration = null) 
        : base(message, innerException, requestId, model, duration) { }
}
