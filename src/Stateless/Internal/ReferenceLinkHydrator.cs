namespace Blacus.ServiceNowTableAPIWrapper.Stateless.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Blacus.ServiceNowTableAPIWrapper.Abstractions;
using Blacus.ServiceNowTableAPIWrapper.Authentication;
using Blacus.ServiceNowTableAPIWrapper.Configuration;
using Blacus.ServiceNowTableAPIWrapper.Metadata;
using Blacus.ServiceNowTableAPIWrapper.Models;

/// <summary>
/// Hydrates ReferenceLink instances on deserialized records by attaching client context and batch-loading [ServiceNowEagerLoad] properties.
/// </summary>
internal sealed class ReferenceLinkHydrator
{
    private static readonly ConcurrentDictionary<Type, IEagerLoader> EagerLoaderCache = new();

    private interface IEagerLoader
    {
        Task LoadBatchAsync(
            IServiceNowStatelessClient client,
            ServiceNowConnection connection,
            IReadOnlyList<ReferenceLink> links,
            int nextDepth,
            int maxDepth,
            CancellationToken ct);
    }

    private sealed class EagerLoader<TTarget> : IEagerLoader where TTarget : class, IServiceNowRecord
    {
        public async Task LoadBatchAsync(
            IServiceNowStatelessClient client,
            ServiceNowConnection connection,
            IReadOnlyList<ReferenceLink> links,
            int nextDepth,
            int maxDepth,
            CancellationToken ct)
        {
            var targetLinks = new List<ReferenceLink<TTarget>>();
            foreach (ReferenceLink link in links)
            {
                if (link is ReferenceLink<TTarget> typedLink && typedLink.Record is null && !string.IsNullOrWhiteSpace(typedLink.Value))
                {
                    targetLinks.Add(typedLink);
                }
            }

            if (targetLinks.Count == 0)
            {
                return;
            }

            var distinctSysIds = targetLinks
                .Select(l => l.Value!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            int remainingDepth = Math.Max(0, maxDepth - nextDepth);
            var queryOptions = new TableRequestOptions
            {
                Query = $"sys_idIN{string.Join(",", distinctSysIds)}",
                Limit = distinctSysIds.Count,
                MaxEagerReferenceDepth = remainingDepth
            };

            ServiceNowPageResult<TTarget> result = await client.ListPageAsync<TTarget>(
                connection,
                queryOptions,
                ct).ConfigureAwait(false);

            if (!result.IsSuccess || result.Items is null || result.Items.Count == 0)
            {
                return;
            }

            var recordMap = new Dictionary<string, TTarget>(StringComparer.OrdinalIgnoreCase);
            foreach (TTarget item in result.Items)
            {
                if (!string.IsNullOrEmpty(item.SysId))
                {
                    recordMap[item.SysId] = item;
                }
            }

            foreach (ReferenceLink<TTarget> link in targetLinks)
            {
                if (link.Value is not null && recordMap.TryGetValue(link.Value, out TTarget? matched))
                {
                    link.Record = matched;
                }
            }
        }
    }

    /// <summary>
    /// Hydrates a single strongly typed record.
    /// </summary>
    public async Task HydrateRecordAsync<T>(
        T? record,
        IServiceNowStatelessClient client,
        ServiceNowConnection connection,
        TableRequestOptions? options,
        ServiceNowTransportOptions transportOptions,
        int currentDepth,
        CancellationToken ct)
        where T : class, IServiceNowRecord
    {
        if (record is null)
        {
            return;
        }

        await HydrateListAsync(new[] { record }, client, connection, options, transportOptions, currentDepth, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Hydrates a list of strongly typed records.
    /// </summary>
    public async Task HydrateListAsync<T>(
        IReadOnlyList<T>? records,
        IServiceNowStatelessClient client,
        ServiceNowConnection connection,
        TableRequestOptions? options,
        ServiceNowTransportOptions transportOptions,
        int currentDepth,
        CancellationToken ct)
        where T : class, IServiceNowRecord
    {
        if (records is null || records.Count == 0)
        {
            return;
        }

        var clientContext = new ServiceNowClientContext(client, connection);
        IReadOnlyList<ReferencePropertyMetadata> refProps = TableMetadata<T>.ReferenceProperties;
        if (refProps.Count == 0)
        {
            return;
        }

        int maxDepth = options?.MaxEagerReferenceDepth ?? transportOptions.MaxEagerReferenceDepth;

        // 1. Attach client context to all reference link properties
        foreach (T record in records)
        {
            foreach (ReferencePropertyMetadata refProp in refProps)
            {
                object? val = refProp.Property.GetValue(record);
                if (val is ReferenceLink refLink)
                {
                    refLink.ClientContext = clientContext;
                }
            }
        }

        // 2. If depth limit reached, skip batch eager loading
        if (currentDepth >= maxDepth || maxDepth <= 0)
        {
            return;
        }

        // 3. Batch eager loading for [ServiceNowEagerLoad] properties
        foreach (ReferencePropertyMetadata refProp in refProps)
        {
            if (!refProp.IsEagerLoad || refProp.TargetRecordType is null)
            {
                continue;
            }

            var linksToLoad = new List<ReferenceLink>();
            foreach (T record in records)
            {
                object? val = refProp.Property.GetValue(record);
                if (val is ReferenceLink refLink && !string.IsNullOrWhiteSpace(refLink.Value))
                {
                    linksToLoad.Add(refLink);
                }
            }

            if (linksToLoad.Count == 0)
            {
                continue;
            }

            IEagerLoader loader = EagerLoaderCache.GetOrAdd(
                refProp.TargetRecordType,
                t => (IEagerLoader)Activator.CreateInstance(typeof(EagerLoader<>).MakeGenericType(t))!);

            await loader.LoadBatchAsync(client, connection, linksToLoad, currentDepth + 1, maxDepth, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Hydrates a dynamic record (ExpandoObject, dictionary, etc.) by attaching client context to any ReferenceLink values.
    /// </summary>
    public void HydrateDynamic(
        object? record,
        IServiceNowStatelessClient client,
        ServiceNowConnection connection)
    {
        if (record is null)
        {
            return;
        }

        var clientContext = new ServiceNowClientContext(client, connection);
        AttachContextDynamic(record, clientContext);
    }

    /// <summary>
    /// Hydrates a collection of dynamic records by attaching client context.
    /// </summary>
    public void HydrateDynamicList(
        IEnumerable<object?>? records,
        IServiceNowStatelessClient client,
        ServiceNowConnection connection)
    {
        if (records is null)
        {
            return;
        }

        var clientContext = new ServiceNowClientContext(client, connection);
        foreach (object? record in records)
        {
            AttachContextDynamic(record, clientContext);
        }
    }

    private static void AttachContextDynamic(object? record, ServiceNowClientContext clientContext)
    {
        if (record is null)
        {
            return;
        }

        if (record is ReferenceLink refLink)
        {
            refLink.ClientContext = clientContext;
            return;
        }

        if (record is IDictionary<string, object?> dict)
        {
            foreach (KeyValuePair<string, object?> kvp in dict)
            {
                if (kvp.Value is ReferenceLink link)
                {
                    link.ClientContext = clientContext;
                }
                else if (kvp.Value is IDictionary<string, object?> nestedDict)
                {
                    AttachContextDynamic(nestedDict, clientContext);
                }
                else if (kvp.Value is IEnumerable<object?> list && !(kvp.Value is string))
                {
                    foreach (object? item in list)
                    {
                        AttachContextDynamic(item, clientContext);
                    }
                }
            }
        }
    }
}
