using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Provides client methods for querying, inserting, updating, and caching ServiceNow table records.
    /// </summary>
    public partial class ServiceNowTableClient : IServiceNowTableService
    {
        private static readonly HttpClient defaultClient = new(new HttpClientHandler
        {
            UseCookies = false
        });

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets the default static <see cref="HttpClient"/> instance used when no custom client is provided.
        /// </summary>
        public static HttpClient HttpClient => defaultClient;

        /// <summary>
        /// Gets or sets the credentials used to authenticate requests to ServiceNow.
        /// </summary>
        public IServiceNowCredential Credentials { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowTableClient"/> class using default HTTP client settings.
        /// </summary>
        /// <param name="credentials">The ServiceNow credentials.</param>
        public ServiceNowTableClient(IServiceNowCredential credentials)
            : this(credentials, defaultClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNowTableClient"/> class using a custom <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="credentials">The ServiceNow credentials.</param>
        /// <param name="httpClient">The HTTP client instance.</param>
        public ServiceNowTableClient(IServiceNowCredential credentials, HttpClient httpClient)
        {
            this.Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            this._httpClient = httpClient ?? defaultClient;
        }

        /// <summary>
        /// Fetches records from ServiceNow utilizing the provided cache service.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="serviceNowCacheService">The cache implementation.</param>
        /// <param name="queryParameterOverride">Optional query parameters override.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of cached or freshly fetched records.</returns>
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

        /// <summary>
        /// Posts a new record to the ServiceNow table via HTTP POST.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="record">The record to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The creation response containing the inserted record.</returns>
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

            string jsonText = JsonSerializer.Serialize<object>(record, JsonSerializerOptionsBuilder.Singleton);
            request.Content = new StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            return JSONDeserialize<T>(responseString);
        }

        private RecordGetResponse<T> JSONDeserialize<T>(string json)
        {
            var toReturn = System.Text.Json.JsonSerializer.Deserialize<RecordGetResponse<T>>(json, JsonSerializerOptionsBuilder.Singleton);
            if (toReturn != null)
            {
                toReturn.SourceJSON = json;
            }
            return toReturn!;
        }

        /// <summary>
        /// Patches an existing record in the ServiceNow table via HTTP PATCH.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="record">The record to update (must have a valid <see cref="IServiceNowTableModel.SysId"/>).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The patch response containing the updated record.</returns>
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

        /// <summary>
        /// Performs an upsert operation (POST if <see cref="IServiceNowTableModel.SysId"/> is null; PATCH otherwise).
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="record">The record to insert or update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response containing the saved record.</returns>
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

        /// <summary>
        /// Fetches and returns all matching records across all paginated responses into a in-memory list.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="queryParameters">Query parameters specifying filters and limits.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list containing all retrieved records across all pages.</returns>
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

        /// <summary>
        /// Streams all matching records across paginated HTTP responses asynchronously one by one without buffering the entire dataset into memory.
        /// </summary>
        /// <typeparam name="T">The record model type implementing <see cref="IServiceNowTableModel"/>.</typeparam>
        /// <param name="queryParameters">Query parameters specifying filters and limits.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable sequence yielding records as they arrive from ServiceNow.</returns>
        public async IAsyncEnumerable<T> StreamAllRecordsAsync<T>(
            QueryParameters queryParameters,
            [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : IServiceNowTableModel
        {
            if (queryParameters == null)
                throw new ArgumentNullException(nameof(queryParameters));

            var getResponse = await GetAsync<T>(queryParameters, cancellationToken);
            if (getResponse?.Records != null)
            {
                foreach (var record in getResponse.Records)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return record;
                }
            }

            while (getResponse != null && getResponse.HasNext && !string.IsNullOrEmpty(getResponse.NextLink))
            {
                cancellationToken.ThrowIfCancellationRequested();
                getResponse = await GetTableByURLAsync<T>(new Uri(getResponse.NextLink), cancellationToken: cancellationToken);
                if (getResponse?.Records != null)
                {
                    foreach (var record in getResponse.Records)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return record;
                    }
                }
            }
        }
    }
}
