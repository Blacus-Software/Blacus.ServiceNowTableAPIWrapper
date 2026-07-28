using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public partial class ServiceNowTableClient
    {
        /// <summary>
        /// Asynchronously retrieves table records from a direct URL link.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="link">The direct URI endpoint.</param>
        /// <param name="queryParameters">Optional query parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public async Task<TableGetResponse<T>> GetTableByURLAsync<T>(
            Uri link,
            QueryParameters? queryParameters = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
            => await GetTableByURLAsync<T>(Credentials, _httpClient, link, queryParameters, JsonSerializerOptions, cancellationToken);

        /// <summary>
        /// Static method to asynchronously retrieve table records from a direct URL.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="url">The direct URI endpoint.</param>
        /// <param name="queryParameterOverride">Optional query parameters override.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public static async Task<TableGetResponse<T>> GetTableByURLAsync<T>(
            IServiceNowCredential credential,
            Uri url,
            QueryParameters? queryParameterOverride = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
            => await GetTableByURLAsync<T>(credential, HttpClient, url, queryParameterOverride, null, cancellationToken);

        /// <summary>
        /// Static method to asynchronously retrieve table records from a direct URL using custom HTTP client and JSON options.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="httpClient">HTTP client instance.</param>
        /// <param name="url">The direct URI endpoint.</param>
        /// <param name="queryParameterOverride">Optional query parameters override.</param>
        /// <param name="jsonSerializerOptions">Optional JSON serializer options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public static async Task<TableGetResponse<T>> GetTableByURLAsync<T>(
            IServiceNowCredential credential,
            HttpClient httpClient,
            Uri url,
            QueryParameters? queryParameterOverride = null,
            JsonSerializerOptions? jsonSerializerOptions = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (url == null) throw new ArgumentNullException(nameof(url));

            Uri requestUri = url;

            if (url.Query.Length == 0)
            {
                var queryParameters = new QueryParameters<T>();
                var query = queryParameters.GetQueryString(queryParameterOverride);
                requestUri = new Uri($"{url}?{query}");
            }

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, requestUri, credential);
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"ServiceNow API error (Status {response.StatusCode}): {responseString}");

            var options = jsonSerializerOptions ?? JsonSerializerOptionsBuilder.Singleton;

            TableGetResponse<T>? result = System.Text.Json.JsonSerializer.Deserialize<TableGetResponse<T>>(responseString, options);
            if (result != null)
            {
                result.OriginalJSON = responseString;

                if (response.Headers.TryGetValues("Link", out IEnumerable<string>? linkHeaderValues))
                {
                    string? linkHeaderValue = linkHeaderValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(linkHeaderValue))
                    {
                        var linkHeader = LinkHeader.LinksFromHeader(linkHeaderValue);
                        result.NextLink = linkHeader?.NextLink;
                    }
                }
            }

            return result!;
        }
    }
}
