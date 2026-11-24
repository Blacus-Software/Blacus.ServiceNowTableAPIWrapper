namespace Blacus.ServiceNowTableAPIWrapper.Metadata;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Attributes;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Describes a property representing a ServiceNow reference link on a model.
/// </summary>
public sealed class ReferencePropertyMetadata
{
    /// <summary>Gets the property reflection info.</summary>
    public PropertyInfo Property { get; init; } = null!;

    /// <summary>Gets the ServiceNow column name (from JsonPropertyName or property name).</summary>
    public string ColumnName { get; init; } = null!;

    /// <summary>Gets a value indicating whether this property is decorated with [ServiceNowEagerLoad(true)].</summary>
    public bool IsEagerLoad { get; init; }

    /// <summary>Gets the referenced record type if the property is ReferenceLink{T}, otherwise null.</summary>
    public Type? TargetRecordType { get; init; }
}

/// <summary>
/// Static metadata cache for strongly typed ServiceNow record models. Extracted once per closed generic type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The record model type.</typeparam>
public static class TableMetadata<T> where T : class, IServiceNowRecord
{
    static TableMetadata()
    {
        Type type = typeof(T);

        // 1. Table name
        ServiceNowTableAttribute? tableAttr = type.GetCustomAttribute<ServiceNowTableAttribute>(true);
        TableName = tableAttr?.TableName;

        // 2. Annotation-based default TableRequestOptions
        SysparmQueryAttribute? queryAttr = type.GetCustomAttribute<SysparmQueryAttribute>(true);
        SysparmFieldsAttribute? fieldsAttr = type.GetCustomAttribute<SysparmFieldsAttribute>(true);
        SysparmDisableAutoFieldsAttribute? disableAutoAttr = type.GetCustomAttribute<SysparmDisableAutoFieldsAttribute>(true);
        SysparmDisplayValueAttribute? displayValueAttr = type.GetCustomAttribute<SysparmDisplayValueAttribute>(true);
        SysparmExcludeReferenceLinkAttribute? excludeLinkAttr = type.GetCustomAttribute<SysparmExcludeReferenceLinkAttribute>(true);
        SysparmViewAttribute? viewAttr = type.GetCustomAttribute<SysparmViewAttribute>(true);
        SysparmLimitAttribute? limitAttr = type.GetCustomAttribute<SysparmLimitAttribute>(true);
        SysparmOffsetAttribute? offsetAttr = type.GetCustomAttribute<SysparmOffsetAttribute>(true);
        SysparmNoCountAttribute? noCountAttr = type.GetCustomAttribute<SysparmNoCountAttribute>(true);
        SysparmQueryCategoryAttribute? categoryAttr = type.GetCustomAttribute<SysparmQueryCategoryAttribute>(true);
        SysparmQueryNoDomainAttribute? noDomainAttr = type.GetCustomAttribute<SysparmQueryNoDomainAttribute>(true);
        SysparmSuppressPaginationHeaderAttribute? suppressLinkAttr = type.GetCustomAttribute<SysparmSuppressPaginationHeaderAttribute>(true);

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var fieldNames = new List<string>();
        var readOnlyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var refProperties = new List<ReferencePropertyMetadata>();
        var propByCol = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        var colByProp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (PropertyInfo prop in properties)
        {
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
            {
                continue;
            }

            JsonPropertyNameAttribute? jsonProp = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            string columnName = jsonProp?.Name ?? prop.Name;
            fieldNames.Add(columnName);
            propByCol[columnName] = prop;
            colByProp[prop.Name] = columnName;

            if (prop.GetCustomAttribute<ServiceNowReadOnlyAttribute>() is not null)
            {
                readOnlyNames.Add(columnName);
            }

            if (typeof(ReferenceLink).IsAssignableFrom(prop.PropertyType))
            {
                ServiceNowEagerLoadAttribute? eager = prop.GetCustomAttribute<ServiceNowEagerLoadAttribute>();
                Type? targetType = null;
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(ReferenceLink<>))
                {
                    targetType = prop.PropertyType.GetGenericArguments()[0];
                }

                refProperties.Add(new ReferencePropertyMetadata
                {
                    Property = prop,
                    ColumnName = columnName,
                    IsEagerLoad = eager?.Enabled ?? false,
                    TargetRecordType = targetType
                });
            }
        }

        AllFields = fieldNames.AsReadOnly();
        ReadOnlyFields = readOnlyNames;
        ReferenceProperties = refProperties.AsReadOnly();
        PropertyByColumnName = propByCol;
        ColumnNameByPropertyName = colByProp;

        IReadOnlyList<string>? effectiveFields = fieldsAttr?.Fields;
        if (effectiveFields is null || effectiveFields.Count == 0)
        {
            if (disableAutoAttr is null)
            {
                effectiveFields = AllFields;
            }
        }

        DefaultOptions = new TableRequestOptions
        {
            TableName = TableName,
            Query = queryAttr?.Query,
            Fields = effectiveFields,
            DisableAutoFields = disableAutoAttr is not null,
            DisplayValue = displayValueAttr?.Mode,
            ExcludeReferenceLink = excludeLinkAttr?.Exclude,
            View = viewAttr?.View,
            Limit = limitAttr?.Limit,
            Offset = offsetAttr?.Offset,
            NoCount = noCountAttr?.NoCount,
            QueryCategory = categoryAttr?.Category,
            QueryNoDomain = noDomainAttr?.NoDomain,
            SuppressPaginationHeader = suppressLinkAttr?.Suppress
        };
    }

    /// <summary>Gets the default ServiceNow table name from [ServiceNowTable].</summary>
    public static string? TableName { get; }

    /// <summary>Gets the default request options synthesized from class annotations.</summary>
    public static TableRequestOptions DefaultOptions { get; }

    /// <summary>Gets all auto-projected JSON column names on <typeparamref name="T"/>.</summary>
    public static IReadOnlyList<string> AllFields { get; }

    /// <summary>Gets column names marked as read-only via [ServiceNowReadOnly].</summary>
    public static IReadOnlySet<string> ReadOnlyFields { get; }

    /// <summary>Gets all reference link properties defined on <typeparamref name="T"/>.</summary>
    public static IReadOnlyList<ReferencePropertyMetadata> ReferenceProperties { get; }

    /// <summary>Maps ServiceNow column names to CLR properties.</summary>
    public static IReadOnlyDictionary<string, PropertyInfo> PropertyByColumnName { get; }

    /// <summary>Maps CLR property names to ServiceNow column names.</summary>
    public static IReadOnlyDictionary<string, string> ColumnNameByPropertyName { get; }
}
