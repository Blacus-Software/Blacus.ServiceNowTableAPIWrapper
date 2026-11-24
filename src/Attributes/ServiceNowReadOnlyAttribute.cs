namespace Blacus.ServiceNowTableAPIWrapper.Attributes;

using System;

/// <summary>
/// Marks a property as read-only. The field is included during GET auto-projections but omitted when serializing write (POST, PUT, PATCH) payloads.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ServiceNowReadOnlyAttribute : Attribute
{
}
