using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fiszki.Services.Models.OpenRouter;

namespace Fiszki.Services.Services.OpenRouter;

/// <summary>
/// Simple semaphore-based rate limiter for OpenRouter API calls (MVP version)
/// </summary>
public sealed class SemaphoreRateLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<SemaphoreRateLimiter> _logger;
    private readonly OpenRouterOptions _options;
    private volatile bool _disposed;

    public SemaphoreRateLimiter(
        IOptions<OpenRouterOptions> options,
        ILogger<SemaphoreRateLimiter> logger)
    {
        _options = options.Value;
        _logger = logger;
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
    }

    public int AvailableTokens => _semaphore.CurrentCount;

    public async Task<bool> TryAcquireAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        try
        {
            var acquired = await _semaphore.WaitAsync(0, cancellationToken);
            if (acquired)
            {
                _logger.LogDebug("Rate limit token acquired. Available: {Available}/{Total}",
                    AvailableTokens, _options.MaxConcurrentRequests);
            }
            else
            {
                _logger.LogWarning("Rate limit exceeded. No tokens available ({Available}/{Total})",
                    AvailableTokens, _options.MaxConcurrentRequests);
            }
            return acquired;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Rate limit acquisition cancelled");
            return false;
        }
    }

    public void Release()
    {
        ThrowIfDisposed();
        
        try
        {
            _semaphore.Release();
            _logger.LogDebug("Rate limit token released. Available: {Available}/{Total}",
                AvailableTokens, _options.MaxConcurrentRequests);
        }
        catch (SemaphoreFullException)
        {
            _logger.LogError("Attempted to release more rate limit tokens than were acquired");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SemaphoreRateLimiter));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
