using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

namespace Blacus.ServiceNowTableAPIWrapper
{

    public partial class ServiceNowTableClient : IServiceNowTableService
    {

        public IServiceNowCacheService _serviceNowCacheService = null;
        public IServiceNowCacheService ServiceNowCacheService
        {
            get
            {
                _serviceNowCacheService ??= new ServiceNowCacheService();
                return _serviceNowCacheService;
            }
            set => _serviceNowCacheService = value;
        }

        private static HttpClient client = new();
        public static HttpClient Client => client;

        public ServiceNowCredential Credentials { get; set; }

        public ServiceNowTableClient(ServiceNowCredential credentials)
        {
            this.Credentials = credentials;
        }

        public async Task<List<T>> GetWithCacheAsync<T>(QueryParameters? queryParameterOverride = null) where T : IServiceNowTableModel
        {

            var options = JsonSerializerOptionsBuilder.Singleton;

            Type t = typeof(T);

            List<T> toReturn = new();

            var sysIdOnlyQueryParameters = new QueryParameters<T>().GetQueryParameters(queryParameterOverride);
            sysIdOnlyQueryParameters.Fields = string.Join(',', ServiceNowCacheService.CacheFields);

            var sysIdList = await GetAsync<T>(sysIdOnlyQueryParameters);

            foreach (var _record in sysIdList.Records)
            {

                T returnRecord = default;
                IServiceNowTableModel cachedRecord = null;

                var sysIdRecord = (IServiceNowTableModel)_record;

                string chacheId = $"{t.FullName}:{sysIdRecord.SysId}";

                string json = ServiceNowCacheService.CacheGet(chacheId);

                if (json != null)
                {
                    returnRecord = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
                    cachedRecord = (IServiceNowTableModel)returnRecord;
                }

                if (!ServiceNowCacheService.Compare(cachedRecord, sysIdRecord))
                {

                    queryParameterOverride.Query = null;
                    RecordGetResponse<T> record = await GetAsync<T>(sysIdRecord.SysId, queryParameterOverride);
                    returnRecord = record.Record;
                    ServiceNowCacheService.PostProcessing((IServiceNowTableModel)returnRecord);
                    string outputJson = System.Text.Json.JsonSerializer.Serialize<T>(returnRecord, options);
                    ServiceNowCacheService.CacheUpdate(chacheId, outputJson);

                }

                toReturn.Add(returnRecord);

            }

            return toReturn;

        }


        public async Task<RecordGetResponse<T>> PostAsync<T>(IServiceNowTableModel record) where T : IServiceNowTableModel
        {

            Type recordType = record.GetType();

            var queryParameters = new QueryParameters<T>();

            var query = queryParameters.GetQueryString();
            var tableName = queryParameters.TableName;

            string url = string.Empty;

            HttpRequestMessage request = null;

            url = $"https://{Credentials.Url}/api/now/table/{tableName}?{query.ToString()}";

            request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Post, new Uri(url), Credentials);

            string jsonText =
                System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);

            var content = new StringContent(jsonText, null, "application/json");

            request.Content = content;
            var response = client.Send(request);

            string responseString = await response.Content.ReadAsStringAsync();
             
            return JSONDeserialize<T>(responseString);

        }

        private RecordGetResponse<T> JSONDeserialize<T>(string json)
        {
            var options = JsonSerializerOptionsBuilder.Singleton;
            var toReturn = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(json, options);
            toReturn.SourceJSON = json;
            return toReturn;
        }

        public async Task<RecordGetResponse<T>> PatchAsync<T>(IServiceNowTableModel record) where T : IServiceNowTableModel
        {

            if (record.SysId == null)
                throw new KeyNotFoundException("The field sys_id is null");

            var queryParameters = new QueryParameters<T>();

            var query = queryParameters.GetQueryString;
            var tableName = queryParameters.TableName;

            Uri url = new($"https://{Credentials.Url}/api/now/table/{tableName}/{record.SysId}?{query.ToString()}");

            HttpRequestMessage request = null;

            request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Patch, url, Credentials);

            string jsonText =
                System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);

            var content = new StringContent(jsonText, null, "application/json");

            request.Content = content;
            var response = client.Send(request);

            string responseString = await response.Content.ReadAsStringAsync();

            return JSONDeserialize<T>(responseString);

        }

        public async Task<RecordGetResponse<T>> UpsertAsync<T>(IServiceNowTableModel record) where T : IServiceNowTableModel
        {

            var queryParameters = new QueryParameters<T>();

            var query = queryParameters.GetQueryString();
            var tableName = queryParameters.TableName;

            string url = null;

            HttpRequestMessage request = null;

            if (record.SysId == null)
            {
                url = $"https://{Credentials.Url}/api/now/table/{tableName}?{query.ToString()}";
                request = new HttpRequestMessage(HttpMethod.Post, url);
            }
            else
            {
                url = $"https://{Credentials.Url}/api/now/table/{tableName}/{record.SysId}?{query.ToString()}";
                request = new HttpRequestMessage(HttpMethod.Patch, url);
            }

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Credentials.BearerString);

            string jsonText =
                System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);

            var content = new StringContent(jsonText, null, "application/json");

            request.Content = content;
            var response = client.Send(request);

            string responseString = await response.Content.ReadAsStringAsync();

            return JSONDeserialize<T>(responseString);

        }

    }

}
