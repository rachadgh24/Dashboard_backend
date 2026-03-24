using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace task1.Application.Resilience;

/// <summary>
/// Centralized Polly resilience pipeline for database operations.
/// Retries only transient failures; does not retry validation or business logic errors.
/// </summary>
public interface IDatabaseResiliencePipeline
{
    /// <summary>Executes an async operation with retry and circuit breaker.</summary>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>Executes an async operation with retry and circuit breaker.</summary>
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}

public sealed class DatabaseResiliencePipeline : IDatabaseResiliencePipeline
{
    private readonly ResiliencePipeline _pipeline;

    public DatabaseResiliencePipeline(ILogger<DatabaseResiliencePipeline> logger)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = args => args.Outcome.Exception is { } ex && TransientExceptionPredicate.IsTransient(ex)
                ? new ValueTask<bool>(true)
                : new ValueTask<bool>(false),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(1),
            UseJitter = true,
            OnRetry = args =>
            {
                logger.LogWarning(
                    args.Outcome.Exception,
                    "Retry attempt {Attempt} after transient failure. Next delay: {Delay}ms",
                    args.AttemptNumber + 1,
                    args.RetryDelay.TotalMilliseconds);
                return default;
            }
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions
        {
            ShouldHandle = args => args.Outcome.Exception is { } ex && TransientExceptionPredicate.IsTransient(ex)
                ? new ValueTask<bool>(true)
                : new ValueTask<bool>(false),
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
            SamplingDuration = TimeSpan.FromSeconds(10),
            OnOpened = args =>
            {
                logger.LogError(args.Outcome.Exception, "Circuit breaker opened. Duration: {Duration}", args.BreakDuration);
                return default;
            },
            OnClosed = args =>
            {
                logger.LogInformation("Circuit breaker closed and reset.");
                return default;
            }
        };

        // Circuit breaker outermost (fail fast when open), retry innermost (retries transient failures)
        _pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(circuitBreakerOptions)
            .AddRetry(retryOptions)
            .Build();
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(async ct => await action().ConfigureAwait(false), cancellationToken);
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(async ct => await action().ConfigureAwait(false), cancellationToken);
    }
}
