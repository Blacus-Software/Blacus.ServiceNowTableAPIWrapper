namespace Blacus.ServiceNowTableAPIWrapper.Stateless.Internal;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Parser for RFC 5988 Link headers and X-Total-Count headers returned by ServiceNow Table API pagination.
/// </summary>
internal static class LinkHeaderParser
{
    private static readonly Regex LinkHeaderRegex = new(
        @"<(?<url>[^>]+)>;\s*rel=""?(?<rel>[a-zA-Z0-9_-]+)""?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses pagination links from the HTTP response headers.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>A <see cref="PaginationLinks"/> instance containing parsed URLs.</returns>
    public static PaginationLinks ParseLinks(HttpResponseMessage response)
    {
        if (response is null)
        {
            return PaginationLinks.Empty;
        }

        IEnumerable<string>? headerValues = null;
        if (response.Headers.TryGetValues("Link", out IEnumerable<string>? values))
        {
            headerValues = values;
        }
        else if (response.Content.Headers.TryGetValues("Link", out IEnumerable<string>? contentValues))
        {
            headerValues = contentValues;
        }

        if (headerValues is null)
        {
            return PaginationLinks.Empty;
        }

        Uri? first = null;
        Uri? prev = null;
        Uri? next = null;
        Uri? last = null;

        foreach (string headerValue in headerValues)
        {
            MatchCollection matches = LinkHeaderRegex.Matches(headerValue);
            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                string urlStr = match.Groups["url"].Value.Trim();
                string rel = match.Groups["rel"].Value.Trim().ToLowerInvariant();

                if (Uri.TryCreate(urlStr, UriKind.Absolute, out Uri? parsedUri))
                {
                    switch (rel)
                    {
                        case "first":
                            first = parsedUri;
                            break;
                        case "prev":
                        case "previous":
                            prev = parsedUri;
                            break;
                        case "next":
                            next = parsedUri;
                            break;
                        case "last":
                            last = parsedUri;
                            break;
                    }
                }
            }
        }

        return new PaginationLinks(first, prev, next, last);
    }

    /// <summary>
    /// Attempts to parse the total record count from the X-Total-Count header.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The total count if present and valid; otherwise, <see langword="null"/>.</returns>
    public static int? ParseTotalCount(HttpResponseMessage response)
    {
        if (response is null)
        {
            return null;
        }

        IEnumerable<string>? countValues = null;
        if (response.Headers.TryGetValues("X-Total-Count", out IEnumerable<string>? values))
        {
            countValues = values;
        }
        else if (response.Content.Headers.TryGetValues("X-Total-Count", out IEnumerable<string>? contentValues))
        {
            countValues = contentValues;
        }

        if (countValues is not null)
        {
            foreach (string val in countValues)
            {
                if (int.TryParse(val, out int count))
                {
                    return count;
                }
            }
        }

        return null;
    }
}
