using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public partial class ServiceNowTableClient
    {
        /// <summary>
        /// Gets or sets a value indicating whether streaming deserialization is used for GET operations.
        /// </summary>
        public static bool UseStreaming { get; set; } = false;

        /// <summary>
        /// Asynchronously retrieves a single record by <c>sys_id</c>.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="sys_id">The ServiceNow <c>sys_id</c> of the record.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <returns>A <see cref="RecordGetResponse{T}"/> containing the record.</returns>
        public Task<RecordGetResponse<T>> GetAsync<T>(
            string sys_id,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, sys_id, queryParameterOverride, default);

        /// <summary>
        /// Asynchronously retrieves a single record by <c>sys_id</c> with cancellation support.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="sys_id">The ServiceNow <c>sys_id</c> of the record.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="RecordGetResponse{T}"/> containing the record.</returns>
        public Task<RecordGetResponse<T>> GetAsync<T>(
            string sys_id,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, sys_id, queryParameterOverride, cancellationToken);

        /// <summary>
        /// Asynchronously retrieves a page of records matching the table model query parameters.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing matching records.</returns>
        public Task<TableGetResponse<T>> GetAsync<T>(
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, queryParameterOverride, default);

        /// <summary>
        /// Asynchronously retrieves a page of records matching the table model query parameters with cancellation support.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing matching records.</returns>
        public Task<TableGetResponse<T>> GetAsync<T>(
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, queryParameterOverride, cancellationToken);

        /// <summary>
        /// Static method to asynchronously retrieve a single record by <c>sys_id</c>.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="sys_id">The ServiceNow <c>sys_id</c>.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <returns>A <see cref="RecordGetResponse{T}"/> containing the record.</returns>
        public static Task<RecordGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            string sys_id,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, sys_id, queryParameterOverride, default);

        /// <summary>
        /// Static method to asynchronously retrieve a single record by <c>sys_id</c> with cancellation support.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="sys_id">The ServiceNow <c>sys_id</c>.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="RecordGetResponse{T}"/> containing the record.</returns>
        public static Task<RecordGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            string sys_id,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, sys_id, queryParameterOverride, cancellationToken);

        /// <summary>
        /// Static method to asynchronously retrieve a single record using custom HTTP client.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="httpClient">HTTP client instance.</param>
        /// <param name="sys_id">The ServiceNow <c>sys_id</c>.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="RecordGetResponse{T}"/> containing the record.</returns>
        public static async Task<RecordGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            HttpClient httpClient,
            string sys_id,
            QueryParameters? queryParameterOverride = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            var queryParameters = new QueryParameters<T>();
            var query = queryParameters.GetQueryString(queryParameterOverride);
            string url = UrlHelper.BuildTableUrl(credential, queryParameters.TableName, sys_id, query.ToString());

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, new Uri(url), credential);
            using var response = await httpClient.SendAsync(request, cancellationToken);

            RecordGetResponse<T>? result;
            var options = JsonSerializerOptionsBuilder.Singleton;

            if (UseStreaming)
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                result = await JsonSerializer.DeserializeAsync<RecordGetResponse<T>>(stream, options, cancellationToken);
            }
            else
            {
                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                result = JsonSerializer.Deserialize<RecordGetResponse<T>>(responseString, options);

                if (result != null)
                {
                    result.SourceJSON = responseString;

                    if (result.Record != null)
                    {
                        var resultType = result.Record.GetType();
                        var originalJSON = resultType.GetProperty("OriginalJSON");
                        if (originalJSON != null && originalJSON.CanWrite)
                        {
                            originalJSON.SetValue(result.Record, responseString);
                        }
                    }
                }
            }

            return result!;
        }

        /// <summary>
        /// Static method to asynchronously retrieve table records matching model parameters.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public static Task<TableGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, queryParameterOverride, default);

        /// <summary>
        /// Static method to asynchronously retrieve table records with cancellation support.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public static Task<TableGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, queryParameterOverride, cancellationToken);

        /// <summary>
        /// Static method to asynchronously retrieve table records using custom HTTP client.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="credential">ServiceNow credential.</param>
        /// <param name="httpClient">HTTP client instance.</param>
        /// <param name="queryParameterOverride">Optional query parameter overrides.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="TableGetResponse{T}"/> containing table records.</returns>
        public static async Task<TableGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            HttpClient httpClient,
            QueryParameters? queryParameterOverride = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            QueryParameters queryParameters = new QueryParameters<T>().GetQueryParameters(queryParameterOverride);
            string url = UrlHelper.BuildTableUrl(credential, queryParameters.TableName ?? string.Empty, null, null);

            return await GetTableByURLAsync<T>(credential, httpClient, new Uri(url), queryParameterOverride, cancellationToken);
        }
    }
}
