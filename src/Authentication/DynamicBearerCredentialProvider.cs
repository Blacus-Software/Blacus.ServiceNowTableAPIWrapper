namespace Blacus.ServiceNowTableAPIWrapper.Authentication;

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;

/// <summary>
/// Provides HTTP Bearer token authentication headers dynamically using an asynchronous token retrieval delegate.
/// </summary>
public sealed class DynamicBearerCredentialProvider : IServiceNowCredentialProvider
{

    private readonly Func<CancellationToken, ValueTask<string?>> _tokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicBearerCredentialProvider"/> class using a <see cref="ValueTask{TResult}"/> delegate.
    /// </summary>
    /// <param name="tokenProvider">A delegate that asynchronously retrieves the current bearer token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenProvider"/> is <see langword="null"/>.</exception>
    public DynamicBearerCredentialProvider(Func<CancellationToken, ValueTask<string?>> tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicBearerCredentialProvider"/> class using a <see cref="Task{TResult}"/> delegate.
    /// </summary>
    /// <param name="tokenProvider">A delegate that asynchronously retrieves the current bearer token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenProvider"/> is <see langword="null"/>.</exception>
    public DynamicBearerCredentialProvider(Func<CancellationToken, Task<string?>> tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        _tokenProvider = async ct => await tokenProvider(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<AuthenticationHeaderValue?> GetAuthorizationHeaderAsync(CancellationToken ct = default)
    {
        string? token = await _tokenProvider(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", token);
    }
    
}
