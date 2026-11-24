namespace Blacus.ServiceNowTableAPIWrapper.Serialization.Converters;

using System;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Blacus.ServiceNowTableAPIWrapper.Attributes;

/// <summary>
/// A type-info modifier for System.Text.Json that suppresses serialization of properties decorated with <see cref="ServiceNowReadOnlyAttribute"/>.
/// </summary>
public static class ReadOnlyPropertyModifier
{
    /// <summary>
    /// Type-info modifier action to attach to a <see cref="DefaultJsonTypeInfoResolver"/>.
    /// </summary>
    /// <param name="typeInfo">The JSON type info being configured.</param>
    public static void Modify(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.AttributeProvider?.GetCustomAttributes(typeof(ServiceNowReadOnlyAttribute), true).Length > 0)
            {
                property.ShouldSerialize = static (_, _) => false;
            }
        }
    }
}
