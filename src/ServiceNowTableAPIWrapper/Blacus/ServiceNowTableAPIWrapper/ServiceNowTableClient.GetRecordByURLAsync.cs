using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public partial class ServiceNowTableClient
    {
        public async Task<RecordGetResponse<T>> GetRecordByURLAsync<T>(
            Uri link,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
            => await GetRecordByURLAsync<T>(Credentials, _httpClient, link, cancellationToken);

        public static async Task<RecordGetResponse<T>> GetRecordByURLAsync<T>(
            IServiceNowCredential credential,
            Uri link,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
            => await GetRecordByURLAsync<T>(credential, HttpClient, link, cancellationToken);

        public static async Task<RecordGetResponse<T>> GetRecordByURLAsync<T>(
            IServiceNowCredential credential,
            HttpClient httpClient,
            Uri link,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var queryParameters = new QueryParameters<T>();
            var query = queryParameters.GetQueryString();

            string url = link.Query.Length == 0 ? $"{link}?{query}" : link.ToString();

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, new Uri(url), credential);
            using var response = await httpClient.SendAsync(request, cancellationToken);

            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = JsonSerializerOptionsBuilder.Singleton;
            RecordGetResponse<T>? result = JsonSerializer.Deserialize<RecordGetResponse<T>>(responseString, options);

            if (result != null)
            {
                result.SourceJSON = responseString;
            }

            return result!;
        }
    }
}
