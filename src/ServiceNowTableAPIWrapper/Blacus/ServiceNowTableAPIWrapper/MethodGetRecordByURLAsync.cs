
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Blacus.ServiceNowTableAPIWrapper
{

    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public partial class ServiceNowTableClient
    {

        public async Task<RecordGetResponse<T>> GetRecordByURLAsync<T>(Uri link)
             where T : IServiceNowTableModel => await GetRecordByURLAsync<T>(Credentials, link);

        public static async Task<RecordGetResponse<T>> GetRecordByURLAsync<T>(ServiceNowCredential credential, Uri link)
        {

            Type t = typeof(T);
            var queryParameters = new QueryParameters<T>();

            var query = queryParameters.GetQueryString();

            string url = $"{link.ToString()}?{query.ToString()}";
            string responseString = null;

            var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, new Uri(url), credential);

            var response = ServiceNowTableClient.HttpClient.Send(request);

            responseString = await response.Content.ReadAsStringAsync();

            var options = JsonSerializerOptionsBuilder.Singleton;
            RecordGetResponse<T> result = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(responseString, options);

            result.SourceJSON = responseString;

            return result;

        }

    }

}