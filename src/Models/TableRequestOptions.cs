namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System.Collections.Generic;

public sealed record TableRequestOptions
{

    public string? TableName { get; init; }
    public string? Query { get; init; }                      // sysparm_query
    public string? QueryCategory { get; init; }              // sysparm_query_category
    public string? View { get; init; }                       // sysparm_view
    
    public bool? DisableAutoFields { get; init; }            // Suppresses auto-projected fields on <T>
    public bool? ExcludeReferenceLink { get; init; }         // sysparm_exclude_reference_link
    public bool? NoCount { get; init; }                      // sysparm_no_count
    public bool? QueryNoDomain { get; init; }                // sysparm_query_no_domain
    public bool? SuppressPaginationHeader { get; init; }     // sysparm_suppress_pagination_header
    
    public int? Limit { get; init; }                         // sysparm_limit (default 1000 on <T> list queries)
    public int? Offset { get; init; }                        // sysparm_offset
    public int? MaxEagerReferenceDepth { get; init; }        // Overrides transport default depth (0 disables eager load)

    public IReadOnlyList<string>? Fields { get; init; }      // sysparm_fields

    public DisplayValueMode? DisplayValue { get; init; }     // sysparm_display_value (false | true | all)

    /// <summary>
    /// Merges the current options with specified override options. Non-null properties in <paramref name="overrides"/> take precedence.
    /// </summary>
    /// <param name="overrides">The options to override current values with.</param>
    /// <returns>A new <see cref="TableRequestOptions"/> instance with merged values.</returns>
    public TableRequestOptions MergeWith(TableRequestOptions? overrides)
    {

        if (overrides is null)
        {
            return this;
        }

        return new TableRequestOptions
        {
            TableName = overrides.TableName ?? this.TableName,
            Query = overrides.Query ?? this.Query,
            Fields = overrides.Fields ?? this.Fields,
            DisableAutoFields = overrides.DisableAutoFields ?? this.DisableAutoFields,
            DisplayValue = overrides.DisplayValue ?? this.DisplayValue,
            ExcludeReferenceLink = overrides.ExcludeReferenceLink ?? this.ExcludeReferenceLink,
            View = overrides.View ?? this.View,
            Limit = overrides.Limit ?? this.Limit,
            Offset = overrides.Offset ?? this.Offset,
            NoCount = overrides.NoCount ?? this.NoCount,
            QueryCategory = overrides.QueryCategory ?? this.QueryCategory,
            QueryNoDomain = overrides.QueryNoDomain ?? this.QueryNoDomain,
            SuppressPaginationHeader = overrides.SuppressPaginationHeader ?? this.SuppressPaginationHeader,
            MaxEagerReferenceDepth = overrides.MaxEagerReferenceDepth ?? this.MaxEagerReferenceDepth
        };

    }

}
