using System;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public static class UrlHelper
    {
        public static string GetBaseUrl(IServiceNowCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            return GetBaseUrl(credential.Url);
        }

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
