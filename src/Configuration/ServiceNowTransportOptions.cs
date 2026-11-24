namespace Blacus.ServiceNowTableAPIWrapper.Configuration;

using System;

/// <summary>
/// Configures HTTP transport, resilience, and serialization defaults for ServiceNow client operations.
/// </summary>
public sealed class ServiceNowTransportOptions
{

    /// <summary>
    /// Gets or sets the overall HTTP request timeout. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets a value indicating whether raw JSON response strings are captured on result envelopes.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool CaptureRawJson { get; set; }

    /// <summary>
    /// Gets or sets the default maximum recursion depth for batch hydrating eager-loaded reference links.
    /// Set to 0 to disable automatic eager loading. Defaults to 2.
    /// </summary>
    public int MaxEagerReferenceDepth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the default page size limit applied to list requests when no explicit limit is specified.
    /// Defaults to 1000.
    /// </summary>
    public int DefaultPageSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether transient HTTP resilience (retry with backoff on 429/503) is enabled.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool EnableResilience { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient HTTP status codes when resilience is enabled.
    /// Defaults to 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retry attempts when resilience is enabled.
    /// Defaults to 2 seconds.
    /// </summary>
    public TimeSpan RetryBackoffDelay { get; set; } = TimeSpan.FromSeconds(2);
    
}
