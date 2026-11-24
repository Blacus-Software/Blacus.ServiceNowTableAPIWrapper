namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Defines the Tier 1 stateless ServiceNow Table API client contract. Holds zero credential or instance state;
/// every operation requires an explicit <see cref="ServiceNowConnection"/>.
/// </summary>
public interface IServiceNowStatelessClient
{
    /// <summary>
    /// Creates a strongly typed LINQ query builder targeting the table associated with <typeparamref name="T"/>.
    /// </summary>
    IServiceNowQueryable<T> Query<T>(ServiceNowConnection connection, TableRequestOptions? options = null) where T : class, IServiceNowRecord;

    /// <summary>
    /// Creates a dynamic non-generic LINQ query builder for the table specified in <paramref name="options"/>.
    /// </summary>
    IServiceNowDynamicQueryable Query(ServiceNowConnection connection, TableRequestOptions options);

    /// <summary>
    /// Retrieves a record by its sys_id as <typeparamref name="T"/>.
    /// </summary>
    Task<ServiceNowResult<T>> GetByIdAsync<T>(ServiceNowConnection connection, string sysId, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves a record by its sys_id as a dynamic object.
    /// </summary>
    Task<ServiceNowResult<dynamic>> GetByIdAsync(ServiceNowConnection connection, string sysId, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a referenced record using its <see cref="ReferenceLink{T}"/>.
    /// </summary>
    Task<ServiceNowResult<T>> GetByLinkAsync<T>(ServiceNowConnection connection, ReferenceLink<T> link, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves a referenced record using its <see cref="ReferenceLink"/>.
    /// </summary>
    Task<ServiceNowResult<dynamic>> GetByLinkAsync(ServiceNowConnection connection, ReferenceLink link, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a record by its direct REST API URI as <typeparamref name="T"/>.
    /// </summary>
    Task<ServiceNowResult<T>> GetByUriAsync<T>(ServiceNowConnection connection, Uri recordUri, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves a record by its direct REST API URI as a dynamic object.
    /// </summary>
    Task<ServiceNowResult<dynamic>> GetByUriAsync(ServiceNowConnection connection, Uri recordUri, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single page of records as <typeparamref name="T"/>.
    /// </summary>
    Task<ServiceNowPageResult<T>> ListPageAsync<T>(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves a single page of dynamic records.
    /// </summary>
    Task<ServiceNowPageResult<dynamic>> ListPageAsync(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single page of records following a pagination URI as <typeparamref name="T"/>.
    /// </summary>
    Task<ServiceNowPageResult<T>> ListPageByUriAsync<T>(ServiceNowConnection connection, Uri pageUri, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves a single page of dynamic records following a pagination URI.
    /// </summary>
    Task<ServiceNowPageResult<dynamic>> ListPageByUriAsync(ServiceNowConnection connection, Uri pageUri, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all records matching the query options as <typeparamref name="T"/>, automatically traversing pages.
    /// </summary>
    Task<ServiceNowResult<IReadOnlyList<T>>> ListAllAsync<T>(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Retrieves all dynamic records matching the query options, automatically traversing pages.
    /// </summary>
    Task<ServiceNowResult<IReadOnlyList<dynamic>>> ListAllAsync(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams pages of records asynchronously as <typeparamref name="T"/>.
    /// </summary>
    IAsyncEnumerable<ServiceNowPageResult<T>> StreamPagesAsync<T>(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Streams pages of dynamic records asynchronously.
    /// </summary>
    IAsyncEnumerable<ServiceNowPageResult<dynamic>> StreamPagesAsync(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams individual records asynchronously as <typeparamref name="T"/>.
    /// </summary>
    IAsyncEnumerable<ServiceNowResult<T>> StreamRecordsAsync<T>(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Streams individual dynamic records asynchronously.
    /// </summary>
    IAsyncEnumerable<ServiceNowResult<dynamic>> StreamRecordsAsync(ServiceNowConnection connection, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new record from a strongly typed model instance.
    /// </summary>
    Task<ServiceNowResult<T>> CreateAsync<T>(ServiceNowConnection connection, T record, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Creates a new record from an anonymous, dictionary, or dynamic payload.
    /// </summary>
    Task<ServiceNowResult<dynamic>> CreateAsync(ServiceNowConnection connection, dynamic record, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies partial updates to a record by sys_id using an anonymous, dictionary, or object payload.
    /// </summary>
    Task<ServiceNowResult<T>> PatchAsync<T>(ServiceNowConnection connection, string sysId, object partialPayload, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Applies partial updates to a record by sys_id using an anonymous, dictionary, or dynamic payload.
    /// </summary>
    Task<ServiceNowResult<dynamic>> PatchAsync(ServiceNowConnection connection, string sysId, object partialPayload, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies partial updates to an existing strongly typed record identified by its <see cref="IServiceNowRecord.SysId"/>.
    /// </summary>
    Task<ServiceNowResult<T>> PatchAsync<T>(ServiceNowConnection connection, T record, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Applies partial updates to an existing dynamic record identified by its sys_id.
    /// </summary>
    Task<ServiceNowResult<dynamic>> PatchAsync(ServiceNowConnection connection, dynamic record, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fully replaces a record (PUT) from a strongly typed model instance.
    /// </summary>
    Task<ServiceNowResult<T>> ReplaceAsync<T>(ServiceNowConnection connection, T record, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Fully replaces a record (PUT) from an anonymous, dictionary, or dynamic payload.
    /// </summary>
    Task<ServiceNowResult<dynamic>> ReplaceAsync(ServiceNowConnection connection, dynamic record, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a record based on whether its sys_id is present.
    /// </summary>
    Task<ServiceNowResult<T>> UpsertAsync<T>(ServiceNowConnection connection, T record, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Inserts or updates a dynamic record based on whether its sys_id is present.
    /// </summary>
    Task<ServiceNowResult<dynamic>> UpsertAsync(ServiceNowConnection connection, dynamic record, TableRequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a record by sys_id from the table associated with <typeparamref name="T"/>.
    /// </summary>
    Task<ServiceNowDeleteResult> DeleteAsync<T>(ServiceNowConnection connection, string sysId, TableRequestOptions? options = null, CancellationToken cancellationToken = default) where T : class, IServiceNowRecord;

    /// <summary>
    /// Deletes a record by sys_id from the table specified in <paramref name="options"/>.
    /// </summary>
    Task<ServiceNowDeleteResult> DeleteAsync(ServiceNowConnection connection, string sysId, TableRequestOptions? options = null, CancellationToken cancellationToken = default);
}
