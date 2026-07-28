using System;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Helper utilities for normalizing ServiceNow instance URLs and building API request endpoints.
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>
        /// Extracts and normalizes the base instance URL from credentials.
        /// </summary>
        /// <param name="credential">The ServiceNow credential object.</param>
        /// <returns>A normalized base URL starting with <c>https://</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="credential"/> is null.</exception>
        public static string GetBaseUrl(IServiceNowCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            return GetBaseUrl(credential.Url);
        }

        /// <summary>
        /// Normalizes a raw URL string ensuring proper scheme and no trailing slash.
        /// </summary>
        /// <param name="rawUrl">The raw URL input.</param>
        /// <returns>A normalized URL string.</returns>
        public static string GetBaseUrl(string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return string.Empty;

            string url = rawUrl.Trim().TrimEnd('/');
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }
            return url;
        }

        /// <summary>
        /// Builds a full ServiceNow Table API request endpoint URL.
        /// </summary>
        /// <param name="credential">The ServiceNow credential.</param>
        /// <param name="tableName">The target ServiceNow table name.</param>
        /// <param name="sysId">Optional <c>sys_id</c> of a specific record.</param>
        /// <param name="queryString">Optional query string parameters.</param>
        /// <returns>A fully qualified ServiceNow API URL.</returns>
        public static string BuildTableUrl(IServiceNowCredential credential, string tableName, string? sysId = null, string? queryString = null)
        {
            string baseUrl = GetBaseUrl(credential);
            string url = string.IsNullOrEmpty(sysId)
                ? $"{baseUrl}/api/now/table/{tableName}"
                : $"{baseUrl}/api/now/table/{tableName}/{sysId}";

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                url += "?" + queryString;
            }

            return url;
        }
    }
}
