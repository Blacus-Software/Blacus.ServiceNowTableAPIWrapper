namespace Blacus.ServiceNowTableAPIWrapper.Stateless;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Configuration;
using Blacus.ServiceNowTableAPIWrapper.Linq;
using Blacus.ServiceNowTableAPIWrapper.Metadata;
using Blacus.ServiceNowTableAPIWrapper.Models;
using Blacus.ServiceNowTableAPIWrapper.Serialization;
using Blacus.ServiceNowTableAPIWrapper.Stateless.Internal;
using Microsoft.Extensions.Options;

/// <summary>
/// Implements the Tier 1 stateless ServiceNow Table API client. Holds zero instance or credential state;
/// every operation requires an explicit <see cref="ServiceNowConnection"/>.
/// </summary>
public sealed class ServiceNowStatelessClient : IServiceNowStatelessClient
{
    private readonly HttpClient _httpClient;
    private readonly ServiceNowTransportOptions _transportOptions;
    private readonly ReferenceLinkHydrator _hydrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowStatelessClient"/> class with an HTTP client and optional transport options.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for all requests.</param>
    /// <param name="transportOptions">Optional transport configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is null.</exception>
    public ServiceNowStatelessClient(HttpClient httpClient, ServiceNowTransportOptions? transportOptions = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _transportOptions = transportOptions ?? new ServiceNowTransportOptions();
        _hydrator = new ReferenceLinkHydrator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowStatelessClient"/> class with an HTTP client and options monitor.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for all requests.</param>
    /// <param name="options">Options containing transport configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is null.</exception>
    public ServiceNowStatelessClient(HttpClient httpClient, IOptions<ServiceNowTransportOptions>? options)
        : this(httpClient, options?.Value)
    {
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> Query<T>(ServiceNowConnection connection, TableRequestOptions? options = null)
        where T : class, IServiceNowRecord
    {
        TableRequestOptions resolved = ResolveOptions<T>(options);
        return new ServiceNowQueryable<T>(this, connection, resolved);
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable Query(ServiceNowConnection connection, TableRequestOptions options)
    {
        return new ServiceNowDynamicQueryable(this, connection, options);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T>> GetByIdAsync<T>(
        ServiceNowConnection connection,
        string sysId,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options.");
        }

        Uri uri = EndpointUriBuilder.BuildRecordUri(connection, resolved.TableName, sysId, resolved);
        return await ExecuteGetSingleAsync<T>(connection, uri, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic>> GetByIdAsync(
        ServiceNowConnection connection,
        string sysId,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls.");
        }

        Uri uri = EndpointUriBuilder.BuildRecordUri(connection, options.TableName, sysId, options);
        return await ExecuteGetSingleDynamicAsync(connection, uri, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<T>> GetByLinkAsync<T>(
        ServiceNowConnection connection,
        ReferenceLink<T> link,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null."));
        }

        if (link is null)
        {
            return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "ReferenceLink must not be null."));
        }

        if (link.Link is not null)
        {
            return GetByUriAsync<T>(connection, link.Link, options, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(link.Value))
        {
            return GetByIdAsync<T>(connection, link.Value!, options, cancellationToken);
        }

        return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "ReferenceLink has neither a valid Link URI nor a Value (sys_id)."));
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<dynamic>> GetByLinkAsync(
        ServiceNowConnection connection,
        ReferenceLink link,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null."));
        }

        if (link is null)
        {
            return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "ReferenceLink must not be null."));
        }

        if (link.Link is not null)
        {
            return GetByUriAsync(connection, link.Link, options, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(link.Value))
        {
            return GetByIdAsync(connection, link.Value!, options, cancellationToken);
        }

        return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "ReferenceLink has neither a valid Link URI nor a Value (sys_id)."));
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T>> GetByUriAsync<T>(
        ServiceNowConnection connection,
        Uri recordUri,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (recordUri is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "recordUri must not be null.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        Uri uri = EndpointUriBuilder.BuildUriFromExisting(connection, recordUri, resolved, shapeOnly: false);
        return await ExecuteGetSingleAsync<T>(connection, uri, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic>> GetByUriAsync(
        ServiceNowConnection connection,
        Uri recordUri,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (recordUri is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "recordUri must not be null.");
        }

        Uri uri = EndpointUriBuilder.BuildUriFromExisting(connection, recordUri, options, shapeOnly: false);
        return await ExecuteGetSingleDynamicAsync(connection, uri, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowPageResult<T>> ListPageAsync<T>(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return CreateFailedPageResult<T>(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return CreateFailedPageResult<T>(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options.");
        }

        Uri uri = EndpointUriBuilder.BuildTableUri(connection, resolved.TableName, resolved);
        return await ExecuteGetPageAsync<T>(connection, uri, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowPageResult<dynamic>> ListPageAsync(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return CreateFailedPageResult<dynamic>(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return CreateFailedPageResult<dynamic>(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls.");
        }

        Uri uri = EndpointUriBuilder.BuildTableUri(connection, options.TableName, options);
        return await ExecuteGetPageDynamicAsync(connection, uri, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowPageResult<T>> ListPageByUriAsync<T>(
        ServiceNowConnection connection,
        Uri pageUri,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return CreateFailedPageResult<T>(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (pageUri is null)
        {
            return CreateFailedPageResult<T>(ServiceNowErrorKind.InvalidRequest, "pageUri must not be null.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        Uri uri = EndpointUriBuilder.BuildUriFromExisting(connection, pageUri, resolved, shapeOnly: false);
        return await ExecuteGetPageAsync<T>(connection, uri, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowPageResult<dynamic>> ListPageByUriAsync(
        ServiceNowConnection connection,
        Uri pageUri,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return CreateFailedPageResult<dynamic>(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (pageUri is null)
        {
            return CreateFailedPageResult<dynamic>(ServiceNowErrorKind.InvalidRequest, "pageUri must not be null.");
        }

        Uri uri = EndpointUriBuilder.BuildUriFromExisting(connection, pageUri, options, shapeOnly: false);
        return await ExecuteGetPageDynamicAsync(connection, uri, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<IReadOnlyList<T>>> ListAllAsync<T>(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        var allItems = new List<T>();
        ServiceNowPageResult<T> page = await ListPageAsync<T>(connection, options, cancellationToken).ConfigureAwait(false);

        if (!page.IsSuccess)
        {
            return ServiceNowResult<IReadOnlyList<T>>.Failure(
                page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                page.Error?.Message,
                page.Error?.Detail,
                page.StatusCode,
                page.Error?.ServerStatus,
                page.RawJson);
        }

        allItems.AddRange(page.Items);

        while (page.HasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            page = await ListPageByUriAsync<T>(connection, page.Links.Next!, options, cancellationToken).ConfigureAwait(false);

            if (!page.IsSuccess)
            {
                return ServiceNowResult<IReadOnlyList<T>>.Failure(
                    page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                    page.Error?.Message,
                    page.Error?.Detail,
                    page.StatusCode,
                    page.Error?.ServerStatus,
                    page.RawJson);
            }

            allItems.AddRange(page.Items);
        }

        return ServiceNowResult<IReadOnlyList<T>>.Success(allItems.AsReadOnly(), HttpStatusCode.OK);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<IReadOnlyList<dynamic>>> ListAllAsync(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var allItems = new List<dynamic>();
        ServiceNowPageResult<dynamic> page = await ListPageAsync(connection, options, cancellationToken).ConfigureAwait(false);

        if (!page.IsSuccess)
        {
            return ServiceNowResult<IReadOnlyList<dynamic>>.Failure(
                page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                page.Error?.Message,
                page.Error?.Detail,
                page.StatusCode,
                page.Error?.ServerStatus,
                page.RawJson);
        }

        allItems.AddRange(page.Items);

        while (page.HasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            page = await ListPageByUriAsync(connection, page.Links.Next!, options, cancellationToken).ConfigureAwait(false);

            if (!page.IsSuccess)
            {
                return ServiceNowResult<IReadOnlyList<dynamic>>.Failure(
                    page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                    page.Error?.Message,
                    page.Error?.Detail,
                    page.StatusCode,
                    page.Error?.ServerStatus,
                    page.RawJson);
            }

            allItems.AddRange(page.Items);
        }

        return ServiceNowResult<IReadOnlyList<dynamic>>.Success(allItems.AsReadOnly(), HttpStatusCode.OK);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceNowPageResult<T>> StreamPagesAsync<T>(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        ServiceNowPageResult<T> page = await ListPageAsync<T>(connection, options, cancellationToken).ConfigureAwait(false);
        yield return page;

        if (!page.IsSuccess)
        {
            yield break;
        }

        while (page.HasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            page = await ListPageByUriAsync<T>(connection, page.Links.Next!, options, cancellationToken).ConfigureAwait(false);
            yield return page;

            if (!page.IsSuccess)
            {
                yield break;
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceNowPageResult<dynamic>> StreamPagesAsync(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ServiceNowPageResult<dynamic> page = await ListPageAsync(connection, options, cancellationToken).ConfigureAwait(false);
        yield return page;

        if (!page.IsSuccess)
        {
            yield break;
        }

        while (page.HasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();
            page = await ListPageByUriAsync(connection, page.Links.Next!, options, cancellationToken).ConfigureAwait(false);
            yield return page;

            if (!page.IsSuccess)
            {
                yield break;
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceNowResult<T>> StreamRecordsAsync<T>(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        await foreach (ServiceNowPageResult<T> page in StreamPagesAsync<T>(connection, options, cancellationToken).ConfigureAwait(false))
        {
            if (!page.IsSuccess)
            {
                yield return ServiceNowResult<T>.Failure(
                    page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                    page.Error?.Message,
                    page.Error?.Detail,
                    page.StatusCode,
                    page.Error?.ServerStatus,
                    page.RawJson);
                yield break;
            }

            foreach (T item in page.Items)
            {
                yield return ServiceNowResult<T>.Success(item, page.StatusCode ?? HttpStatusCode.OK, page.RawJson);
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ServiceNowResult<dynamic>> StreamRecordsAsync(
        ServiceNowConnection connection,
        TableRequestOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ServiceNowPageResult<dynamic> page in StreamPagesAsync(connection, options, cancellationToken).ConfigureAwait(false))
        {
            if (!page.IsSuccess)
            {
                yield return ServiceNowResult<dynamic>.Failure(
                    page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                    page.Error?.Message,
                    page.Error?.Detail,
                    page.StatusCode,
                    page.Error?.ServerStatus,
                    page.RawJson);
                yield break;
            }

            foreach (dynamic item in page.Items)
            {
                yield return ServiceNowResult<dynamic>.Success(item, page.StatusCode ?? HttpStatusCode.OK, page.RawJson);
            }
        }
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T>> CreateAsync<T>(
        ServiceNowConnection connection,
        T record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (record is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record payload must not be null.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, resolved.TableName, null, resolved);
        return await ExecuteWriteAsync<T>(connection, HttpMethod.Post, uri, record, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic>> CreateAsync(
        ServiceNowConnection connection,
        dynamic record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (record is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Record payload must not be null.");
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, options.TableName, null, options);
        return await ExecuteWriteDynamicAsync(connection, HttpMethod.Post, uri, record, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T>> PatchAsync<T>(
        ServiceNowConnection connection,
        string sysId,
        object partialPayload,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty.");
        }

        if (partialPayload is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "partialPayload must not be null.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, resolved.TableName, sysId, resolved);
        return await ExecuteWriteAsync<T>(connection, new HttpMethod("PATCH"), uri, partialPayload, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic>> PatchAsync(
        ServiceNowConnection connection,
        string sysId,
        object partialPayload,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty.");
        }

        if (partialPayload is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "partialPayload must not be null.");
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, options.TableName, sysId, options);
        return await ExecuteWriteDynamicAsync(connection, new HttpMethod("PATCH"), uri, partialPayload, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<T>> PatchAsync<T>(
        ServiceNowConnection connection,
        T record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (record is null)
        {
            return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record must not be null."));
        }

        if (string.IsNullOrWhiteSpace(record.SysId))
        {
            return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record sys_id must not be null or empty for patch operations."));
        }

        return PatchAsync<T>(connection, record.SysId!, record, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<dynamic>> PatchAsync(
        ServiceNowConnection connection,
        dynamic record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (record is null)
        {
            return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Record must not be null."));
        }

        string? sysId = DynamicPayloadInspector.TryGetSysId(record);
        if (string.IsNullOrWhiteSpace(sysId))
        {
            return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id could not be found on dynamic record payload for patch operations."));
        }

        return PatchAsync(connection, sysId!, (object)record, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T>> ReplaceAsync<T>(
        ServiceNowConnection connection,
        T record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (record is null)
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record payload must not be null.");
        }

        if (string.IsNullOrWhiteSpace(record.SysId))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record sys_id must not be null or empty for replace operations.");
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, resolved.TableName, record.SysId, resolved);
        return await ExecuteWriteAsync<T>(connection, HttpMethod.Put, uri, record, resolved, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic>> ReplaceAsync(
        ServiceNowConnection connection,
        dynamic record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Connection must not be null.");
        }

        if (record is null)
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Record payload must not be null.");
        }

        string? sysId = DynamicPayloadInspector.TryGetSysId(record);
        if (string.IsNullOrWhiteSpace(sysId))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "sys_id could not be found on dynamic record payload for replace operations.");
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls.");
        }

        Uri uri = EndpointUriBuilder.BuildWriteUri(connection, options.TableName, sysId, options);
        return await ExecuteWriteDynamicAsync(connection, HttpMethod.Put, uri, record, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<T>> UpsertAsync<T>(
        ServiceNowConnection connection,
        T record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (record is null)
        {
            return Task.FromResult(ServiceNowResult<T>.Failure(ServiceNowErrorKind.InvalidRequest, "Record must not be null."));
        }

        if (!string.IsNullOrWhiteSpace(record.SysId))
        {
            return PatchAsync<T>(connection, record, options, cancellationToken);
        }

        return CreateAsync<T>(connection, record, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<dynamic>> UpsertAsync(
        ServiceNowConnection connection,
        dynamic record,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (record is null)
        {
            return Task.FromResult(ServiceNowResult<dynamic>.Failure(ServiceNowErrorKind.InvalidRequest, "Record must not be null."));
        }

        string? sysId = DynamicPayloadInspector.TryGetSysId(record);
        if (!string.IsNullOrWhiteSpace(sysId))
        {
            return PatchAsync(connection, sysId!, (object)record, options, cancellationToken);
        }

        return CreateAsync(connection, (object)record, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceNowDeleteResult> DeleteAsync<T>(
        ServiceNowConnection connection,
        string sysId,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
        where T : class, IServiceNowRecord
    {
        if (connection is null)
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "Connection must not be null."));
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty."));
        }

        TableRequestOptions resolved = ResolveOptions<T>(options);
        if (string.IsNullOrWhiteSpace(resolved.TableName))
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "Table name was not specified in model annotation or request options."));
        }

        Uri uri = EndpointUriBuilder.BuildRecordUri(connection, resolved.TableName, sysId, null);
        return await ExecuteDeleteAsync(connection, uri, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceNowDeleteResult> DeleteAsync(
        ServiceNowConnection connection,
        string sysId,
        TableRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (connection is null)
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "Connection must not be null."));
        }

        if (string.IsNullOrWhiteSpace(sysId))
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "sys_id must not be null or empty."));
        }

        if (string.IsNullOrWhiteSpace(options?.TableName))
        {
            return new ServiceNowDeleteResult(false, null, new ServiceNowError(ServiceNowErrorKind.InvalidRequest, "TableName must be specified in TableRequestOptions for non-generic calls."));
        }

        Uri uri = EndpointUriBuilder.BuildRecordUri(connection, options.TableName, sysId, null);
        return await ExecuteDeleteAsync(connection, uri, cancellationToken).ConfigureAwait(false);
    }

    private static TableRequestOptions ResolveOptions<T>(TableRequestOptions? explicitOptions) where T : class, IServiceNowRecord
    {
        TableRequestOptions defaults = TableMetadata<T>.DefaultOptions;
        if (explicitOptions is null)
        {
            return defaults;
        }

        return defaults.MergeWith(explicitOptions);
    }

    private static ServiceNowPageResult<TItem> CreateFailedPageResult<TItem>(ServiceNowErrorKind kind, string message)
    {
        return new ServiceNowPageResult<TItem>(
            false,
            null,
            Array.Empty<TItem>(),
            null,
            PaginationLinks.Empty,
            new ServiceNowError(kind, message));
    }

    private async Task<ServiceNowResult<T>> ExecuteGetSingleAsync<T>(
        ServiceNowConnection connection,
        Uri uri,
        TableRequestOptions options,
        CancellationToken cancellationToken)
        where T : class, IServiceNowRecord
    {
        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, HttpMethod.Get, uri, null, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return ServiceNowResult<T>.Failure(error.Kind, error.Message, error.Detail, response?.StatusCode, error.ServerStatus, rawJson);
        }

        SingleWireEnvelope<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<SingleWireEnvelope<T>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return ServiceNowResult<T>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Failed to deserialize JSON response to target type.",
                ex.Message,
                response?.StatusCode,
                null,
                rawJson);
        }

        if (envelope is null || envelope.Result is null)
        {
            return ServiceNowResult<T>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Response JSON did not contain a valid result envelope.",
                null,
                response?.StatusCode,
                null,
                rawJson);
        }

        await _hydrator.HydrateRecordAsync(envelope.Result, this, connection, options, _transportOptions, 0, cancellationToken).ConfigureAwait(false);
        return ServiceNowResult<T>.Success(envelope.Result, response!.StatusCode, rawJson);
    }

    private async Task<ServiceNowResult<dynamic>> ExecuteGetSingleDynamicAsync(
        ServiceNowConnection connection,
        Uri uri,
        CancellationToken cancellationToken)
    {
        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, HttpMethod.Get, uri, null, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return ServiceNowResult<dynamic>.Failure(error.Kind, error.Message, error.Detail, response?.StatusCode, error.ServerStatus, rawJson);
        }

        SingleWireEnvelope<dynamic>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<SingleWireEnvelope<dynamic>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return ServiceNowResult<dynamic>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Failed to deserialize JSON response to dynamic object.",
                ex.Message,
                response?.StatusCode,
                null,
                rawJson);
        }

        if (envelope is null || envelope.Result is null)
        {
            return ServiceNowResult<dynamic>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Response JSON did not contain a valid result envelope.",
                null,
                response?.StatusCode,
                null,
                rawJson);
        }

        _hydrator.HydrateDynamic(envelope.Result, this, connection);
        return ServiceNowResult<dynamic>.Success(envelope.Result, response!.StatusCode, rawJson);
    }

    private async Task<ServiceNowPageResult<T>> ExecuteGetPageAsync<T>(
        ServiceNowConnection connection,
        Uri uri,
        TableRequestOptions options,
        CancellationToken cancellationToken)
        where T : class, IServiceNowRecord
    {
        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, HttpMethod.Get, uri, null, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return new ServiceNowPageResult<T>(
                false,
                response?.StatusCode,
                Array.Empty<T>(),
                null,
                PaginationLinks.Empty,
                error,
                rawJson);
        }

        ListWireEnvelope<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<ListWireEnvelope<T>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return new ServiceNowPageResult<T>(
                false,
                response?.StatusCode,
                Array.Empty<T>(),
                null,
                PaginationLinks.Empty,
                new ServiceNowError(ServiceNowErrorKind.DeserializationError, "Failed to deserialize list response.", ex.Message),
                rawJson);
        }

        IReadOnlyList<T> items = envelope?.Result ?? (IReadOnlyList<T>)Array.Empty<T>();
        PaginationLinks links = LinkHeaderParser.ParseLinks(response!);
        int? totalCount = LinkHeaderParser.ParseTotalCount(response!);

        await _hydrator.HydrateListAsync(items, this, connection, options, _transportOptions, 0, cancellationToken).ConfigureAwait(false);
        return new ServiceNowPageResult<T>(true, response!.StatusCode, items, totalCount, links, null, rawJson);
    }

    private async Task<ServiceNowPageResult<dynamic>> ExecuteGetPageDynamicAsync(
        ServiceNowConnection connection,
        Uri uri,
        TableRequestOptions? options,
        CancellationToken cancellationToken)
    {
        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, HttpMethod.Get, uri, null, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return new ServiceNowPageResult<dynamic>(
                false,
                response?.StatusCode,
                Array.Empty<dynamic>(),
                null,
                PaginationLinks.Empty,
                error,
                rawJson);
        }

        ListWireEnvelope<dynamic>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<ListWireEnvelope<dynamic>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return new ServiceNowPageResult<dynamic>(
                false,
                response?.StatusCode,
                Array.Empty<dynamic>(),
                null,
                PaginationLinks.Empty,
                new ServiceNowError(ServiceNowErrorKind.DeserializationError, "Failed to deserialize dynamic list response.", ex.Message),
                rawJson);
        }

        IReadOnlyList<dynamic> items = envelope?.Result ?? (IReadOnlyList<dynamic>)Array.Empty<dynamic>();
        PaginationLinks links = LinkHeaderParser.ParseLinks(response!);
        int? totalCount = LinkHeaderParser.ParseTotalCount(response!);

        _hydrator.HydrateDynamicList(items, this, connection);
        return new ServiceNowPageResult<dynamic>(true, response!.StatusCode, items, totalCount, links, null, rawJson);
    }

    private async Task<ServiceNowResult<T>> ExecuteWriteAsync<T>(
        ServiceNowConnection connection,
        HttpMethod method,
        Uri uri,
        object payload,
        TableRequestOptions options,
        CancellationToken cancellationToken)
        where T : class, IServiceNowRecord
    {
        string json = JsonSerializer.Serialize(payload, payload.GetType(), ServiceNowJsonOptions.Write);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, method, uri, content, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return ServiceNowResult<T>.Failure(error.Kind, error.Message, error.Detail, response?.StatusCode, error.ServerStatus, rawJson);
        }

        SingleWireEnvelope<T>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<SingleWireEnvelope<T>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return ServiceNowResult<T>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Failed to deserialize write response to target type.",
                ex.Message,
                response?.StatusCode,
                null,
                rawJson);
        }

        if (envelope is null || envelope.Result is null)
        {
            return ServiceNowResult<T>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Write response JSON did not contain a valid result envelope.",
                null,
                response?.StatusCode,
                null,
                rawJson);
        }

        await _hydrator.HydrateRecordAsync(envelope.Result, this, connection, options, _transportOptions, 0, cancellationToken).ConfigureAwait(false);
        return ServiceNowResult<T>.Success(envelope.Result, response!.StatusCode, rawJson);
    }

    private async Task<ServiceNowResult<dynamic>> ExecuteWriteDynamicAsync(
        ServiceNowConnection connection,
        HttpMethod method,
        Uri uri,
        object payload,
        CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(payload, payload.GetType(), ServiceNowJsonOptions.Write);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, method, uri, content, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return ServiceNowResult<dynamic>.Failure(error.Kind, error.Message, error.Detail, response?.StatusCode, error.ServerStatus, rawJson);
        }

        SingleWireEnvelope<dynamic>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<SingleWireEnvelope<dynamic>>(responseBody!, ServiceNowJsonOptions.Default);
        }
        catch (Exception ex)
        {
            return ServiceNowResult<dynamic>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Failed to deserialize write response to dynamic object.",
                ex.Message,
                response?.StatusCode,
                null,
                rawJson);
        }

        if (envelope is null || envelope.Result is null)
        {
            return ServiceNowResult<dynamic>.Failure(
                ServiceNowErrorKind.DeserializationError,
                "Write response JSON did not contain a valid result envelope.",
                null,
                response?.StatusCode,
                null,
                rawJson);
        }

        _hydrator.HydrateDynamic(envelope.Result, this, connection);
        return ServiceNowResult<dynamic>.Success(envelope.Result, response!.StatusCode, rawJson);
    }

    private async Task<ServiceNowDeleteResult> ExecuteDeleteAsync(
        ServiceNowConnection connection,
        Uri uri,
        CancellationToken cancellationToken)
    {
        (HttpResponseMessage? response, string? responseBody, ServiceNowError? error) =
            await SendRequestAsync(connection, HttpMethod.Delete, uri, null, cancellationToken).ConfigureAwait(false);

        string? rawJson = _transportOptions.CaptureRawJson ? responseBody : null;

        if (error is not null)
        {
            return new ServiceNowDeleteResult(false, response?.StatusCode, error, rawJson);
        }

        return new ServiceNowDeleteResult(true, response!.StatusCode, null, rawJson);
    }

    private async Task<(HttpResponseMessage? Response, string? ResponseBody, ServiceNowError? Error)> SendRequestAsync(
        ServiceNowConnection connection,
        HttpMethod method,
        Uri uri,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (content is not null)
        {
            request.Content = content;
        }

        if (connection.CredentialProvider is not null)
        {
            AuthenticationHeaderValue? auth = await connection.CredentialProvider.GetAuthorizationHeaderAsync(cancellationToken).ConfigureAwait(false);
            if (auth is not null)
            {
                request.Headers.Authorization = auth;
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            return (null, null, new ServiceNowError(ServiceNowErrorKind.TransportError, "The HTTP request timed out.", ex.Message));
        }
        catch (HttpRequestException ex)
        {
            return (null, null, new ServiceNowError(ServiceNowErrorKind.TransportError, ex.Message));
        }
        catch (Exception ex)
        {
            return (null, null, new ServiceNowError(ServiceNowErrorKind.TransportError, ex.Message));
        }

        string responseBody = string.Empty;
        if (response.Content is not null)
        {
            try
            {
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return (response, null, new ServiceNowError(ServiceNowErrorKind.TransportError, "Failed reading response body.", ex.Message));
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            ServiceNowError apiError = ParseError(response.StatusCode, responseBody);
            return (response, responseBody, apiError);
        }

        return (response, responseBody, null);
    }

    private static ServiceNowError ParseError(HttpStatusCode statusCode, string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new ServiceNowError(ServiceNowErrorKind.HttpProtocolError, $"HTTP {(int)statusCode} {statusCode} with empty body.");
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                string? message = null;
                string? detail = null;
                string? status = null;

                if (root.TryGetProperty("status", out JsonElement statusElem) && statusElem.ValueKind == JsonValueKind.String)
                {
                    status = statusElem.GetString();
                }

                if (root.TryGetProperty("error", out JsonElement errorElem))
                {
                    if (errorElem.ValueKind == JsonValueKind.Object)
                    {
                        if (errorElem.TryGetProperty("message", out JsonElement msgElem) && msgElem.ValueKind == JsonValueKind.String)
                        {
                            message = msgElem.GetString();
                        }

                        if (errorElem.TryGetProperty("detail", out JsonElement detailElem) && detailElem.ValueKind == JsonValueKind.String)
                        {
                            detail = detailElem.GetString();
                        }
                    }
                    else if (errorElem.ValueKind == JsonValueKind.String)
                    {
                        message = errorElem.GetString();
                    }
                }

                if (message is not null || detail is not null || status is not null)
                {
                    return new ServiceNowError(
                        ServiceNowErrorKind.ApiError,
                        message ?? $"HTTP {(int)statusCode} {statusCode}",
                        detail,
                        status);
                }
            }
        }
        catch
        {
            // Not valid JSON - proxy error or HTML hibernation page
        }

        return new ServiceNowError(
            ServiceNowErrorKind.HttpProtocolError,
            $"HTTP {(int)statusCode} {statusCode}: Non-JSON response received.",
            responseBody);
    }
}
