namespace Blacus.ServiceNowTableAPIWrapper.Metadata;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Utility for inspecting dynamic, dictionary, or anonymous record payloads to extract metadata such as sys_id.
/// </summary>
public static class DynamicPayloadInspector
{
    /// <summary>
    /// Attempts to extract the sys_id string value from a dynamic, anonymous, dictionary, or POCO payload.
    /// </summary>
    /// <param name="payload">The object payload.</param>
    /// <returns>The sys_id string if found; otherwise, <see langword="null"/>.</returns>
    public static string? TryGetSysId(object? payload)
    {
        if (payload is null)
        {
            return null;
        }

        if (payload is IDictionary<string, object?> dict)
        {
            if (dict.TryGetValue("sys_id", out object? val) && val is not null)
            {
                return val.ToString();
            }

            if (dict.TryGetValue("SysId", out object? valUpper) && valUpper is not null)
            {
                return valUpper.ToString();
            }

            return null;
        }

        Type type = payload.GetType();
        PropertyInfo? prop = type.GetProperty("sys_id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop is not null && prop.CanRead)
        {
            object? val = prop.GetValue(payload);
            return val?.ToString();
        }

        return null;
    }
}
