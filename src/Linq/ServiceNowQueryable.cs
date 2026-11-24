namespace Blacus.ServiceNowTableAPIWrapper.Linq;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Strongly typed query builder compiling C# LINQ expressions into server-side ServiceNow Table API query parameters.
/// </summary>
/// <typeparam name="T">The record model type.</typeparam>
public sealed class ServiceNowQueryable<T> : IServiceNowQueryable<T> where T : class, IServiceNowRecord
{
    private readonly IServiceNowStatelessClient _client;
    private readonly ServiceNowConnection _connection;
    private readonly TableRequestOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowQueryable{T}"/> class.
    /// </summary>
    /// <param name="client">The stateless client executing the query.</param>
    /// <param name="connection">The target ServiceNow connection.</param>
    /// <param name="options">Optional initial table request options.</param>
    public ServiceNowQueryable(
        IServiceNowStatelessClient client,
        ServiceNowConnection connection,
        TableRequestOptions? options = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _options = options ?? new TableRequestOptions();
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> SelectFields<TProjection>(Expression<Func<T, TProjection>> fieldSelector)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> Take(int count)
    {
        return new ServiceNowQueryable<T>(_client, _connection, _options with { Limit = count });
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> Skip(int count)
    {
        return new ServiceNowQueryable<T>(_client, _connection, _options with { Offset = count });
    }

    /// <inheritdoc />
    public IServiceNowQueryable<T> WithOptions(TableRequestOptions options)
    {
        return new ServiceNowQueryable<T>(_client, _connection, _options.MergeWith(options));
    }

    /// <inheritdoc />
    public ServiceNowResult<TableRequestOptions> ToRequestOptions()
    {
        return ServiceNowResult<TableRequestOptions>.Success(_options, System.Net.HttpStatusCode.OK);
    }

    /// <inheritdoc />
    public Task<ServiceNowPageResult<T>> ToPageAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListPageAsync<T>(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<IReadOnlyList<T>>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListAllAsync<T>(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        TableRequestOptions singleOption = _options with { Limit = 1 };
        ServiceNowPageResult<T> page = await _client.ListPageAsync<T>(_connection, singleOption, cancellationToken).ConfigureAwait(false);
        if (!page.IsSuccess)
        {
            return ServiceNowResult<T?>.Failure(
                page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                page.Error?.Message,
                page.Error?.Detail,
                page.StatusCode,
                page.Error?.ServerStatus,
                page.RawJson);
        }

        T? first = page.Items.Count > 0 ? page.Items[0] : default;
        return ServiceNowResult<T?>.Success(first, page.StatusCode ?? System.Net.HttpStatusCode.OK, page.RawJson);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceNowPageResult<T>> StreamPagesAsync(CancellationToken cancellationToken = default)
    {
        return _client.StreamPagesAsync<T>(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceNowResult<T>> StreamRecordsAsync(CancellationToken cancellationToken = default)
    {
        return _client.StreamRecordsAsync<T>(_connection, _options, cancellationToken);
    }
}
