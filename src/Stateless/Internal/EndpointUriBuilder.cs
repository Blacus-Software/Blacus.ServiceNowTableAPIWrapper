namespace Blacus.ServiceNowTableAPIWrapper.Stateless.Internal;

using System;
using System.Collections.Generic;
using System.Text;
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Constructs and formats REST API endpoint URIs for ServiceNow Table API requests.
/// </summary>
internal static class EndpointUriBuilder
{
    /// <summary>
    /// Builds the URI targeting a ServiceNow table record collection for read operations.
    /// </summary>
    /// <param name="connection">The ServiceNow connection providing the base URI.</param>
    /// <param name="tableName">The name of the target table.</param>
    /// <param name="options">Optional table request options.</param>
    /// <returns>An absolute <see cref="Uri"/> targeting the table collection.</returns>
    public static Uri BuildTableUri(ServiceNowConnection connection, string tableName, TableRequestOptions? options)
    {
        string relativePath = $"/api/now/table/{Uri.EscapeDataString(tableName)}";
        string queryString = BuildQueryString(options, shapeOnly: false);
        return CombineUri(connection.BaseUri, relativePath, queryString);
    }

    /// <summary>
    /// Builds the URI targeting a single ServiceNow record for read operations.
    /// </summary>
    /// <param name="connection">The ServiceNow connection providing the base URI.</param>
    /// <param name="tableName">The name of the target table.</param>
    /// <param name="sysId">The sys_id of the target record.</param>
    /// <param name="options">Optional table request options.</param>
    /// <returns>An absolute <see cref="Uri"/> targeting the single record.</returns>
    public static Uri BuildRecordUri(ServiceNowConnection connection, string tableName, string sysId, TableRequestOptions? options)
    {
        string relativePath = $"/api/now/table/{Uri.EscapeDataString(tableName)}/{Uri.EscapeDataString(sysId)}";
        string queryString = BuildQueryString(options, shapeOnly: false);
        return CombineUri(connection.BaseUri, relativePath, queryString);
    }

    /// <summary>
    /// Builds the URI targeting a write operation (POST, PUT, PATCH). Always appends resolved shape-controlling parameters.
    /// </summary>
    /// <param name="connection">The ServiceNow connection providing the base URI.</param>
    /// <param name="tableName">The name of the target table.</param>
    /// <param name="sysId">Optional sys_id for update/replace operations. If null, targets the table collection (create).</param>
    /// <param name="options">Optional table request options supplying shape-controlling settings.</param>
    /// <returns>An absolute <see cref="Uri"/> for the write request.</returns>
    public static Uri BuildWriteUri(ServiceNowConnection connection, string tableName, string? sysId, TableRequestOptions? options)
    {
        string relativePath = string.IsNullOrWhiteSpace(sysId)
            ? $"/api/now/table/{Uri.EscapeDataString(tableName)}"
            : $"/api/now/table/{Uri.EscapeDataString(tableName)}/{Uri.EscapeDataString(sysId)}";

        string queryString = BuildQueryString(options, shapeOnly: true);
        return CombineUri(connection.BaseUri, relativePath, queryString);
    }

    /// <summary>
    /// Builds an absolute URI from a caller-provided URI (such as a pagination link or record link), merging any additional options.
    /// </summary>
    /// <param name="connection">The ServiceNow connection providing the base URI if <paramref name="uri"/> is relative.</param>
    /// <param name="uri">The absolute or relative URI.</param>
    /// <param name="options">Optional table request options.</param>
    /// <param name="shapeOnly">Whether to append shape-controlling parameters only.</param>
    /// <returns>An absolute <see cref="Uri"/>.</returns>
    public static Uri BuildUriFromExisting(ServiceNowConnection connection, Uri uri, TableRequestOptions? options, bool shapeOnly = false)
    {
        Uri absoluteUri = uri.IsAbsoluteUri ? uri : new Uri(connection.BaseUri, uri);

        if (options is null)
        {
            return absoluteUri;
        }

        string additionalQuery = BuildQueryString(options, shapeOnly);
        if (string.IsNullOrEmpty(additionalQuery))
        {
            return absoluteUri;
        }

        string existingQuery = absoluteUri.Query;
        string combinedQuery;
        if (string.IsNullOrEmpty(existingQuery) || existingQuery == "?")
        {
            combinedQuery = additionalQuery;
        }
        else
        {
            combinedQuery = existingQuery + "&" + additionalQuery.TrimStart('?');
        }

        var uriBuilder = new UriBuilder(absoluteUri)
        {
            Query = combinedQuery.TrimStart('?')
        };

        return uriBuilder.Uri;
    }

    private static Uri CombineUri(Uri baseUri, string relativePath, string queryString)
    {
        string baseStr = baseUri.ToString().TrimEnd('/');
        string relStr = relativePath.TrimStart('/');
        string full = $"{baseStr}/{relStr}{queryString}";
        return new Uri(full, UriKind.Absolute);
    }

    private static string BuildQueryString(TableRequestOptions? options, bool shapeOnly)
    {
        if (options is null)
        {
            return string.Empty;
        }

        var pairs = new List<string>();

        // Shape-controlling parameters (applied on both read and write)
        if (options.DisplayValue.HasValue)
        {
            string modeStr = options.DisplayValue.Value switch
            {
                DisplayValueMode.True => "true",
                DisplayValueMode.All => "all",
                _ => "false"
            };
            pairs.Add($"sysparm_display_value={Uri.EscapeDataString(modeStr)}");
        }

        if (options.ExcludeReferenceLink.HasValue)
        {
            pairs.Add($"sysparm_exclude_reference_link={(options.ExcludeReferenceLink.Value ? "true" : "false")}");
        }

        if (options.Fields is not null && options.Fields.Count > 0)
        {
            pairs.Add($"sysparm_fields={Uri.EscapeDataString(string.Join(",", options.Fields))}");
        }

        if (!string.IsNullOrWhiteSpace(options.View))
        {
            pairs.Add($"sysparm_view={Uri.EscapeDataString(options.View)}");
        }

        if (!shapeOnly)
        {
            if (!string.IsNullOrWhiteSpace(options.Query))
            {
                pairs.Add($"sysparm_query={Uri.EscapeDataString(options.Query)}");
            }

            if (options.Limit.HasValue)
            {
                pairs.Add($"sysparm_limit={options.Limit.Value}");
            }

            if (options.Offset.HasValue)
            {
                pairs.Add($"sysparm_offset={options.Offset.Value}");
            }

            if (options.NoCount.HasValue)
            {
                pairs.Add($"sysparm_no_count={(options.NoCount.Value ? "true" : "false")}");
            }

            if (!string.IsNullOrWhiteSpace(options.QueryCategory))
            {
                pairs.Add($"sysparm_query_category={Uri.EscapeDataString(options.QueryCategory)}");
            }

            if (options.QueryNoDomain.HasValue)
            {
                pairs.Add($"sysparm_query_no_domain={(options.QueryNoDomain.Value ? "true" : "false")}");
            }

            if (options.SuppressPaginationHeader.HasValue)
            {
                pairs.Add($"sysparm_suppress_pagination_header={(options.SuppressPaginationHeader.Value ? "true" : "false")}");
            }
        }

        return pairs.Count > 0 ? "?" + string.Join("&", pairs) : string.Empty;
    }
}
