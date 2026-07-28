using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public partial class ServiceNowTableClient
    {
        public static bool UseStreaming { get; set; } = false;

        public Task<RecordGetResponse<T>> GetAsync<T>(
            string sys_id,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, sys_id, queryParameterOverride, default);

        public Task<RecordGetResponse<T>> GetAsync<T>(
            string sys_id,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, sys_id, queryParameterOverride, cancellationToken);

        public Task<TableGetResponse<T>> GetAsync<T>(
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, queryParameterOverride, default);

        public Task<TableGetResponse<T>> GetAsync<T>(
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(Credentials, _httpClient, queryParameterOverride, cancellationToken);

        public static Task<RecordGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            string sys_id,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, sys_id, queryParameterOverride, default);

        public static Task<RecordGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            string sys_id,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, sys_id, queryParameterOverride, cancellationToken);

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

        public static Task<TableGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, queryParameterOverride, default);

        public static Task<TableGetResponse<T>> GetAsync<T>(
            IServiceNowCredential credential,
            QueryParameters? queryParameterOverride,
            CancellationToken cancellationToken) where T : IServiceNowTableModel
            => GetAsync<T>(credential, HttpClient, queryParameterOverride, cancellationToken);

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
