namespace Blacus.ServiceNowTableAPIWrapper.Authentication;

using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;

/// <summary>
/// Provides HTTP Basic authentication headers using a fixed username and password.
/// </summary>
public sealed class BasicCredentialProvider : IServiceNowCredentialProvider
{

    private readonly AuthenticationHeaderValue _header;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicCredentialProvider"/> class.
    /// </summary>
    /// <param name="username">The ServiceNow user name.</param>
    /// <param name="password">The ServiceNow user password.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is <see langword="null"/>.</exception>
    public BasicCredentialProvider(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        string rawCredentials = $"{username}:{password}";
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCredentials));
        _header = new AuthenticationHeaderValue("Basic", base64);
    }

    /// <inheritdoc />
    public ValueTask<AuthenticationHeaderValue?> GetAuthorizationHeaderAsync(CancellationToken ct = default)
    {
        return new ValueTask<AuthenticationHeaderValue?>(_header);
    }
    
}
