namespace Blacus.ServiceNowTableAPIWrapper.Attributes;

using System;

/// <summary>
/// Specifies the default ServiceNow table name associated with a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ServiceNowTableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowTableAttribute"/> class.
    /// </summary>
    /// <param name="tableName">The ServiceNow table name (e.g., "incident", "sys_user").</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tableName"/> is <see langword="null"/>.</exception>
    public ServiceNowTableAttribute(string tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        TableName = tableName;
    }

    /// <summary>
    /// Gets the ServiceNow table name.
    /// </summary>
    public string TableName { get; }
}
