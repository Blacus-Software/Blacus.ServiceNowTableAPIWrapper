namespace Blacus.ServiceNowTableAPIWrapper.Authentication;

using System;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;

/// <summary>
/// Encapsulates connection details for a ServiceNow instance, including the base URL and credential provider.
/// </summary>
public sealed class ServiceNowConnection
{
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowConnection"/> class.
    /// </summary>
    /// <param name="instanceUrl">The base URL of the ServiceNow instance.</param>
    /// <param name="credentialProvider">The credential provider used to authenticate requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instanceUrl"/> or <paramref name="credentialProvider"/> is <see langword="null"/>.</exception>
    public ServiceNowConnection(Uri instanceUrl, IServiceNowCredentialProvider credentialProvider)
    {
        ArgumentNullException.ThrowIfNull(instanceUrl);
        ArgumentNullException.ThrowIfNull(credentialProvider);

        if (!instanceUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("The instance URL must be an absolute URI.", nameof(instanceUrl));
        }

        InstanceUrl = instanceUrl;
        CredentialProvider = credentialProvider;
    }

    /// <summary>
    /// Gets the base URL of the ServiceNow instance.
    /// </summary>
    public Uri InstanceUrl { get; }

    /// <summary>
    /// Gets the base URL of the ServiceNow instance. Alias for <see cref="InstanceUrl"/>.
    /// </summary>
    public Uri BaseUri => InstanceUrl;

    /// <summary>
    /// Gets the credential provider used to authenticate requests to the ServiceNow instance.
    /// </summary>
    public IServiceNowCredentialProvider CredentialProvider { get; }

}
