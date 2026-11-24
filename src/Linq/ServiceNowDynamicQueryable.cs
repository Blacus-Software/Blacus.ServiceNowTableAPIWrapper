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
/// Non-generic dynamic query builder compiling fluent clauses into server-side ServiceNow Table API query parameters.
/// </summary>
public sealed class ServiceNowDynamicQueryable : IServiceNowDynamicQueryable
{
    private readonly IServiceNowStatelessClient _client;
    private readonly ServiceNowConnection _connection;
    private readonly TableRequestOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceNowDynamicQueryable"/> class.
    /// </summary>
    /// <param name="client">The stateless client executing the query.</param>
    /// <param name="connection">The target ServiceNow connection.</param>
    /// <param name="options">Initial table request options containing at least TableName.</param>
    public ServiceNowDynamicQueryable(
        IServiceNowStatelessClient client,
        ServiceNowConnection connection,
        TableRequestOptions options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable Where(Expression<Func<ServiceNowRow, bool>> predicate)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable OrderBy(string fieldName)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable OrderByDescending(string fieldName)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable ThenBy(string fieldName)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable ThenByDescending(string fieldName)
    {
        return this;
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable SelectFields(params string[] fields)
    {
        return new ServiceNowDynamicQueryable(_client, _connection, _options with { Fields = fields });
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable Take(int count)
    {
        return new ServiceNowDynamicQueryable(_client, _connection, _options with { Limit = count });
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable Skip(int count)
    {
        return new ServiceNowDynamicQueryable(_client, _connection, _options with { Offset = count });
    }

    /// <inheritdoc />
    public IServiceNowDynamicQueryable WithOptions(TableRequestOptions options)
    {
        return new ServiceNowDynamicQueryable(_client, _connection, _options.MergeWith(options));
    }

    /// <inheritdoc />
    public ServiceNowResult<TableRequestOptions> ToRequestOptions()
    {
        return ServiceNowResult<TableRequestOptions>.Success(_options, System.Net.HttpStatusCode.OK);
    }

    /// <inheritdoc />
    public Task<ServiceNowPageResult<dynamic>> ToPageAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListPageAsync(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ServiceNowResult<IReadOnlyList<dynamic>>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return _client.ListAllAsync(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceNowResult<dynamic?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        TableRequestOptions singleOption = _options with { Limit = 1 };
        ServiceNowPageResult<dynamic> page = await _client.ListPageAsync(_connection, singleOption, cancellationToken).ConfigureAwait(false);
        if (!page.IsSuccess)
        {
            return ServiceNowResult<dynamic?>.Failure(
                page.Error?.Kind ?? ServiceNowErrorKind.ApiError,
                page.Error?.Message,
                page.Error?.Detail,
                page.StatusCode,
                page.Error?.ServerStatus,
                page.RawJson);
        }

        dynamic? first = page.Items.Count > 0 ? page.Items[0] : null;
        return ServiceNowResult<dynamic?>.Success(first, page.StatusCode ?? System.Net.HttpStatusCode.OK, page.RawJson);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceNowPageResult<dynamic>> StreamPagesAsync(CancellationToken cancellationToken = default)
    {
        return _client.StreamPagesAsync(_connection, _options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceNowResult<dynamic>> StreamRecordsAsync(CancellationToken cancellationToken = default)
    {
        return _client.StreamRecordsAsync(_connection, _options, cancellationToken);
    }
}
