namespace Blacus.ServiceNowTableAPIWrapper.Attributes;

using System;

/// <summary>
/// Instructs the client to batch-load referenced records upon parent retrieval within the configured eager depth.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ServiceNowEagerLoadAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowEagerLoadAttribute"/> class.
    /// </summary>
    /// <param name="enabled">Whether eager loading is enabled for this property. Defaults to <see langword="true"/>.</param>
    public ServiceNowEagerLoadAttribute(bool enabled = true)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Gets a value indicating whether eager loading is enabled.
    /// </summary>
    public bool Enabled { get; }
}
