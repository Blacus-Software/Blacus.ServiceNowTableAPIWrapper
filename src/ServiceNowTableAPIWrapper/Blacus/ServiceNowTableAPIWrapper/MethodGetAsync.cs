#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Blacus.ServiceNowTableAPIWrapper
{

    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    public partial class ServiceNowTableClient
    {

        public static bool UseStreaming { get; set; } = false;

        public async Task<RecordGetResponse<T>> GetAsync<T>(string sys_id, QueryParameters? queryParameterOverride = null)
            where T : IServiceNowTableModel => await GetAsync<T>(Credentials, sys_id, queryParameterOverride);

        public async Task<TableGetResponse<T>> GetAsync<T>(QueryParameters? queryParameterOverride = null)
            where T : IServiceNowTableModel => await GetAsync<T>(Credentials, queryParameterOverride);

        public static async Task<RecordGetResponse<T>> GetAsync<T>(IServiceNowCredential credential, string sys_id, QueryParameters? queryParameterOverride = null)
        {

            JsonSerializerOptions options = null;
            RecordGetResponse<T> result = null;
            string responseString = null;

            Type t = typeof(T);

            var queryParameters = new QueryParameters<T>();

            var query = queryParameters.GetQueryString(queryParameterOverride);

            Uri url = new($"https://{credential.Url}/api/now/table/{queryParameters.TableName}/{sys_id}?{query.ToString()}");

            HttpResponseMessage response = null;

            var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Get, url, credential);

            response = ServiceNowTableClient.HttpClient.Send(request);

            if (UseStreaming)
            {
                options = JsonSerializerOptionsBuilder.Singleton;
                result = JsonSerializer.Deserialize<RecordGetResponse<T>>(await response.Content.ReadAsStreamAsync(), options);
            }
            else
            {
                responseString = await response.Content.ReadAsStringAsync();

                options = JsonSerializerOptionsBuilder.Singleton;
                result = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(responseString, options);

                if (result.Record != null)
                {

                    var resultType = result.Record.GetType();
                    var originalJSON = resultType.GetProperty("OriginalJSON");
                    if (originalJSON != null)
                    {
                        originalJSON.SetValue(result.Record, responseString);
                    }

                }
            }

            return result;

        }

        public static async Task<TableGetResponse<T>> GetAsync<T>(IServiceNowCredential credential, QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
        {

            QueryParameters queryParameters = new QueryParameters<T>().GetQueryParameters(queryParameterOverride);

            Uri url = new($"https://{credential.Url}/api/now/table/{((QueryParameters)queryParameters).TableName}");

            var toReturn = await ServiceNowTableClient.GetTableByURLAsync<T>(credential, url, queryParameterOverride);

            return toReturn;

        }

    }

}
