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

        public async Task<RecordGetResponse<T>> GetAsync<T>(string sys_id, QueryParameters? queryParameterOverride = null)
            where T : IServiceNowTableModel => await GetAsync<T>(Credentials, sys_id, queryParameterOverride);

        public async Task<TableGetResponse<T>> GetAsync<T>(QueryParameters? queryParameterOverride = null)
            where T : IServiceNowTableModel => await GetAsync<T>(Credentials, queryParameterOverride);

        public static async Task<RecordGetResponse<T>> GetAsync<T>(ServiceNowCredential credential, string sys_id, QueryParameters? queryParameterOverride = null)
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

            response = ServiceNowTableClient.Client.Send(request);

            responseString = await response.Content.ReadAsStringAsync();

            options = JsonSerializerOptionsBuilder.Singleton;
            result = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(responseString, options);

            if (result.Result != null)
            {

                var resultType = result.Result.GetType();
                var originalJSON = resultType.GetProperty("OriginalJSON");
                if (originalJSON != null)
                {
                    originalJSON.SetValue(result.Result, responseString);
                }

            }

            return result;

        }

        public static async Task<TableGetResponse<T>> GetAsync<T>(ServiceNowCredential credential, QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
        {

            QueryParameters queryParameters = new QueryParameters<T>().GetQueryParameters(queryParameterOverride); 

            var query = queryParameters.GetQueryString();

            Uri url = new($"https://{credential.Url}/api/now/table/{((QueryParameters)queryParameters).TableName}?{queryParameters.GetQueryString()}");

            var toReturn = await ServiceNowTableClient.GetTableByURLAsync<T>(credential, url);

            return toReturn;

        }

    }

}
