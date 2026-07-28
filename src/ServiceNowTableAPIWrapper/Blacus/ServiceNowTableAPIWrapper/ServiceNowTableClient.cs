using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    public partial class ServiceNowTableClient : IServiceNowTableService
    {
        private static readonly HttpClient defaultClient = new(new HttpClientHandler
        {
            UseCookies = false
        });

        private readonly HttpClient _httpClient;

        public static HttpClient HttpClient => defaultClient;

        public IServiceNowCredential Credentials { get; set; }

        public ServiceNowTableClient(IServiceNowCredential credentials)
            : this(credentials, defaultClient)
        {
        }

        public ServiceNowTableClient(IServiceNowCredential credentials, HttpClient httpClient)
        {
            this.Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            this._httpClient = httpClient ?? defaultClient;
        }

        public async Task<List<T>> GetWithCacheAsync<T>(
            IServiceNowCacheService serviceNowCacheService,
            QueryParameters? queryParameterOverride = null,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (serviceNowCacheService == null)
                throw new ArgumentNullException(nameof(serviceNowCacheService));

            var options = JsonSerializerOptionsBuilder.Singleton;
            Type t = typeof(T);
            List<T> toReturn = new();

            var sysIdOnlyQueryParameters = new QueryParameters<T>().GetQueryParameters(queryParameterOverride);
            sysIdOnlyQueryParameters.Fields = string.Join(',', serviceNowCacheService.CacheFields);

            var sysIdList = await GetAsync<T>(sysIdOnlyQueryParameters, cancellationToken);

            if (sysIdList?.Records == null)
                return toReturn;

            foreach (var _record in sysIdList.Records)
            {
                T? returnRecord = default;
                IServiceNowTableModel? cachedRecord = null;
                var sysIdRecord = (IServiceNowTableModel)_record;

                string cacheId = $"{t.FullName}:{sysIdRecord.SysId}";
                string? json = serviceNowCacheService.CacheGet?.Invoke(cacheId);

                if (json != null)
                {
                    returnRecord = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
                    cachedRecord = returnRecord;
                }

                if (serviceNowCacheService.Compare == null || !serviceNowCacheService.Compare(cachedRecord, sysIdRecord))
                {
                    var singleRecordQuery = queryParameterOverride != null
                        ? queryParameterOverride.Clone()
                        : new QueryParameters();
                    singleRecordQuery.Query = null;

                    RecordGetResponse<T> record = await GetAsync<T>(sysIdRecord.SysId!, singleRecordQuery, cancellationToken);
                    returnRecord = record.Record;

                    if (returnRecord != null)
                    {
                        serviceNowCacheService.PostProcessing?.Invoke(returnRecord);
                        string outputJson = System.Text.Json.JsonSerializer.Serialize<T>(returnRecord, options);
                        serviceNowCacheService.CacheUpdate?.Invoke(cacheId, outputJson);
                    }
                }

                if (returnRecord != null)
                {
                    toReturn.Add(returnRecord);
                }
            }

            return toReturn;
        }

        public async Task<RecordGetResponse<T>> PostAsync<T>(
            IServiceNowTableModel record,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var queryParameters = new QueryParameters<T>();
            var query = queryParameters.GetQueryString();
            var tableName = queryParameters.TableName;

            string url = UrlHelper.BuildTableUrl(Credentials, tableName, null, query.ToString());

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Post, new Uri(url), Credentials);

            string jsonText = System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);
            request.Content = new StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            return JSONDeserialize<T>(responseString);
        }

        private RecordGetResponse<T> JSONDeserialize<T>(string json)
        {
            var options = JsonSerializerOptionsBuilder.Singleton;
            var toReturn = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(json, options);
            if (toReturn != null)
            {
                toReturn.SourceJSON = json;
            }
            return toReturn!;
        }

        public async Task<RecordGetResponse<T>> PatchAsync<T>(
            IServiceNowTableModel record,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (record.SysId == null)
                throw new KeyNotFoundException("The field sys_id is null");

            var queryParameters = new QueryParameters<T>();
            var query = queryParameters.GetQueryString();
            var tableName = queryParameters.TableName;

            string url = UrlHelper.BuildTableUrl(Credentials, tableName, record.SysId, query.ToString());

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(HttpMethod.Patch, new Uri(url), Credentials);

            string jsonText = System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);
            request.Content = new StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            return JSONDeserialize<T>(responseString);
        }

        public async Task<RecordGetResponse<T>> UpsertAsync<T>(
            IServiceNowTableModel record,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var queryParameters = new QueryParameters<T>();
            var query = queryParameters.GetQueryString();
            var tableName = queryParameters.TableName;

            HttpMethod method = record.SysId == null ? HttpMethod.Post : HttpMethod.Patch;
            string url = UrlHelper.BuildTableUrl(Credentials, tableName, record.SysId, query.ToString());

            using var request = ServiceNowHttpRequestMessage.NewHttpRequestMessage(method, new Uri(url), Credentials);

            string jsonText = System.Text.Json.JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);
            request.Content = new StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            return JSONDeserialize<T>(responseString);
        }

        public async Task<List<T>> ListAllRecordsAsync<T>(
            QueryParameters queryParameters,
            CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            var toReturn = new List<T>();

            var getResponse = await GetAsync<T>(queryParameters, cancellationToken);
            if (getResponse?.Records != null)
            {
                toReturn.AddRange(getResponse.Records);
            }

            while (getResponse != null && getResponse.HasNext && !string.IsNullOrEmpty(getResponse.NextLink))
            {
                getResponse = await GetTableByURLAsync<T>(new Uri(getResponse.NextLink), cancellationToken: cancellationToken);
                if (getResponse?.Records != null)
                {
                    toReturn.AddRange(getResponse.Records);
                }
            }

            return toReturn;
        }
    }
}
