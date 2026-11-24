namespace Blacus.ServiceNowTableAPIWrapper.Attributes;

using System;
using System.Collections.Generic;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Specifies the default sysparm_query parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmQueryAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmQueryAttribute"/> class.</summary>
    /// <param name="query">The encoded query string.</param>
    public SysparmQueryAttribute(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        Query = query;
    }

    /// <summary>Gets the default query string.</summary>
    public string Query { get; }
}

/// <summary>
/// Specifies the default sysparm_fields projection for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmFieldsAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmFieldsAttribute"/> class.</summary>
    /// <param name="fields">The list of ServiceNow column names.</param>
    public SysparmFieldsAttribute(params string[] fields)
    {
        Fields = fields ?? Array.Empty<string>();
    }

    /// <summary>Gets the list of field names.</summary>
    public IReadOnlyList<string> Fields { get; }
}

/// <summary>
/// Suppresses automatic projection of fields derived from model properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmDisableAutoFieldsAttribute : Attribute
{
}

/// <summary>
/// Specifies the default sysparm_display_value mode for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmDisplayValueAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmDisplayValueAttribute"/> class.</summary>
    /// <param name="mode">The display value mode.</param>
    public SysparmDisplayValueAttribute(DisplayValueMode mode)
    {
        Mode = mode;
    }

    /// <summary>Gets the display value mode.</summary>
    public DisplayValueMode Mode { get; }
}

/// <summary>
/// Specifies the default sysparm_exclude_reference_link parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmExcludeReferenceLinkAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmExcludeReferenceLinkAttribute"/> class.</summary>
    /// <param name="exclude">Whether to exclude reference links.</param>
    public SysparmExcludeReferenceLinkAttribute(bool exclude = true)
    {
        Exclude = exclude;
    }

    /// <summary>Gets a value indicating whether reference links are excluded.</summary>
    public bool Exclude { get; }
}

/// <summary>
/// Specifies the default sysparm_view parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmViewAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmViewAttribute"/> class.</summary>
    /// <param name="view">The view name.</param>
    public SysparmViewAttribute(string view)
    {
        ArgumentNullException.ThrowIfNull(view);
        View = view;
    }

    /// <summary>Gets the view name.</summary>
    public string View { get; }
}

/// <summary>
/// Specifies the default sysparm_limit parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmLimitAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmLimitAttribute"/> class.</summary>
    /// <param name="limit">The maximum number of records to return.</param>
    public SysparmLimitAttribute(int limit)
    {
        Limit = limit;
    }

    /// <summary>Gets the record limit.</summary>
    public int Limit { get; }
}

/// <summary>
/// Specifies the default sysparm_offset parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmOffsetAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmOffsetAttribute"/> class.</summary>
    /// <param name="offset">The record offset.</param>
    public SysparmOffsetAttribute(int offset)
    {
        Offset = offset;
    }

    /// <summary>Gets the record offset.</summary>
    public int Offset { get; }
}

/// <summary>
/// Specifies the default sysparm_no_count parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmNoCountAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmNoCountAttribute"/> class.</summary>
    /// <param name="noCount">Whether to suppress total count calculation.</param>
    public SysparmNoCountAttribute(bool noCount = true)
    {
        NoCount = noCount;
    }

    /// <summary>Gets a value indicating whether total count is suppressed.</summary>
    public bool NoCount { get; }
}

/// <summary>
/// Specifies the default sysparm_query_category parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmQueryCategoryAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmQueryCategoryAttribute"/> class.</summary>
    /// <param name="category">The query category name.</param>
    public SysparmQueryCategoryAttribute(string category)
    {
        ArgumentNullException.ThrowIfNull(category);
        Category = category;
    }

    /// <summary>Gets the query category.</summary>
    public string Category { get; }
}

/// <summary>
/// Specifies the default sysparm_query_no_domain parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmQueryNoDomainAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmQueryNoDomainAttribute"/> class.</summary>
    /// <param name="noDomain">Whether to disable domain separation filtering.</param>
    public SysparmQueryNoDomainAttribute(bool noDomain = true)
    {
        NoDomain = noDomain;
    }

    /// <summary>Gets a value indicating whether domain separation filtering is disabled.</summary>
    public bool NoDomain { get; }
}

/// <summary>
/// Specifies the default sysparm_suppress_pagination_header parameter for a record class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SysparmSuppressPaginationHeaderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="SysparmSuppressPaginationHeaderAttribute"/> class.</summary>
    /// <param name="suppress">Whether to suppress Link headers.</param>
    public SysparmSuppressPaginationHeaderAttribute(bool suppress = true)
    {
        Suppress = suppress;
    }

    /// <summary>Gets a value indicating whether Link headers are suppressed.</summary>
    public bool Suppress { get; }
}
