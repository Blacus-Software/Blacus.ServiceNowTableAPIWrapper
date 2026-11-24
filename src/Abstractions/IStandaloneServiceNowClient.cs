namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using System;

/// <summary>
/// Represents a standalone, disposable ServiceNow stateless client.
/// </summary>
public interface IStandaloneServiceNowStatelessClient : IServiceNowStatelessClient, IDisposable
{
}

/// <summary>
/// Represents a standalone, disposable bound ServiceNow client.
/// </summary>
public interface IStandaloneServiceNowClient : IServiceNowClient, IDisposable
{
}
