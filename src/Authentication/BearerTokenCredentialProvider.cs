namespace Blacus.ServiceNowTableAPIWrapper.Authentication;

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;

/// <summary>
/// Provides HTTP Bearer token authentication headers using a static OAuth or access token.
/// </summary>
public sealed class BearerTokenCredentialProvider : IServiceNowCredentialProvider
{

    private readonly AuthenticationHeaderValue _header;

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenCredentialProvider"/> class.
    /// </summary>
    /// <param name="token">The static bearer token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is <see langword="null"/>.</exception>
    public BearerTokenCredentialProvider(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        _header = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <inheritdoc />
    public ValueTask<AuthenticationHeaderValue?> GetAuthorizationHeaderAsync(CancellationToken ct = default)
    {
        return new ValueTask<AuthenticationHeaderValue?>(_header);
    }
    
}
