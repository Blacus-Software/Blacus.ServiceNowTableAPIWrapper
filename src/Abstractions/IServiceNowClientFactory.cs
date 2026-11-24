namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using Blacus.ServiceNowTableAPIWrapper.Authentication;

/// <summary>
/// Factory for creating bound <see cref="IServiceNowClient"/> instances backed by the registered stateless client.
/// </summary>
public interface IServiceNowClientFactory
{
    /// <summary>
    /// Creates a bound <see cref="IServiceNowClient"/> instance for the specified connection.
    /// </summary>
    /// <param name="connection">The connection details for the target ServiceNow instance.</param>
    /// <returns>A bound <see cref="IServiceNowClient"/> instance.</returns>
    IServiceNowClient CreateClient(ServiceNowConnection connection);
}
