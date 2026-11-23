namespace Blacus.ServiceNowTableAPIWrapper.Models;

using System.Collections.Generic;

public sealed record TableRequestOptions
{
    public string? TableName { get; init; }
    public string? Query { get; init; }                      // sysparm_query
    public IReadOnlyList<string>? Fields { get; init; }      // sysparm_fields
    public bool? DisableAutoFields { get; init; }            // Suppresses auto-projected fields on <T>
    public DisplayValueMode? DisplayValue { get; init; }     // sysparm_display_value (false | true | all)
    public bool? ExcludeReferenceLink { get; init; }         // sysparm_exclude_reference_link
    public string? View { get; init; }                       // sysparm_view
    public int? Limit { get; init; }                         // sysparm_limit (default 1000 on <T> list queries)
    public int? Offset { get; init; }                        // sysparm_offset
    public bool? NoCount { get; init; }                      // sysparm_no_count
    public string? QueryCategory { get; init; }              // sysparm_query_category
    public bool? QueryNoDomain { get; init; }                // sysparm_query_no_domain
    public bool? SuppressPaginationHeader { get; init; }     // sysparm_suppress_pagination_header
    public int? MaxEagerReferenceDepth { get; init; }        // Overrides transport default depth (0 disables eager load)

    public TableRequestOptions MergeWith(TableRequestOptions? overrides);
}
