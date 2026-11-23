namespace Blacus.ServiceNowTableAPIWrapper.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Models;

public interface IServiceNowQueryable<T> where T : class, IServiceNowRecord
{
    IServiceNowQueryable<T> Where(Expression<Func<T, bool>> predicate);
    IServiceNowQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    IServiceNowQueryable<T> SelectFields<TProjection>(Expression<Func<T, TProjection>> fieldSelector);
    IServiceNowQueryable<T> Take(int count);
    IServiceNowQueryable<T> Skip(int count);
    IServiceNowQueryable<T> WithOptions(TableRequestOptions options);

    ServiceNowResult<TableRequestOptions> ToRequestOptions();

    Task<ServiceNowPageResult<T>> ToPageAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<IReadOnlyList<T>>> ToListAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<T?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowPageResult<T>> StreamPagesAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowResult<T>> StreamRecordsAsync(CancellationToken cancellationToken = default);
}

public interface IServiceNowDynamicQueryable
{
    IServiceNowDynamicQueryable Where(Expression<Func<ServiceNowRow, bool>> predicate);
    IServiceNowDynamicQueryable OrderBy(string fieldName);
    IServiceNowDynamicQueryable OrderByDescending(string fieldName);
    IServiceNowDynamicQueryable ThenBy(string fieldName);
    IServiceNowDynamicQueryable ThenByDescending(string fieldName);
    IServiceNowDynamicQueryable SelectFields(params string[] fields);
    IServiceNowDynamicQueryable Take(int count);
    IServiceNowDynamicQueryable Skip(int count);
    IServiceNowDynamicQueryable WithOptions(TableRequestOptions options);

    ServiceNowResult<TableRequestOptions> ToRequestOptions();

    Task<ServiceNowPageResult<dynamic>> ToPageAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<IReadOnlyList<dynamic>>> ToListAsync(CancellationToken cancellationToken = default);
    Task<ServiceNowResult<dynamic?>> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowPageResult<dynamic>> StreamPagesAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ServiceNowResult<dynamic>> StreamRecordsAsync(CancellationToken cancellationToken = default);
}
