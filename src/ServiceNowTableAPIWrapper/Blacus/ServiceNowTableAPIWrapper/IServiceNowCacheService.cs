using System;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Defines a contract for caching ServiceNow table records.
    /// </summary>
    public interface IServiceNowCacheService
    {
        /// <summary>
        /// Gets or sets the function to retrieve cached JSON data by record key.
        /// </summary>
        Func<string, string>? CacheGet { get; set; }

        /// <summary>
        /// Gets or sets the action to update cached JSON data for a key.
        /// </summary>
        Action<string, string>? CacheUpdate { get; set; }

        /// <summary>
        /// Gets or sets an optional post-processing delegate invoked after record retrieval.
        /// </summary>
        Action<IServiceNowTableModel>? PostProcessing { get; set; }

        /// <summary>
        /// Gets or sets a comparison delegate to determine if a cached record is up-to-date with a live record.
        /// </summary>
        Func<IServiceNowTableModel?, IServiceNowTableModel, bool>? Compare { get; set; }

        /// <summary>
        /// Gets or sets the fields requested when performing sys_id metadata queries for caching.
        /// </summary>
        string[] CacheFields { get; set; }

        /// <summary>
        /// Default comparison implementation for cached records.
        /// </summary>
        /// <param name="cacheRecord">The cached record instance.</param>
        /// <param name="liveRecord">The live ServiceNow record instance.</param>
        /// <returns><c>false</c> by default.</returns>
        public static bool CompareDefault(IServiceNowTableModel? cacheRecord, IServiceNowTableModel liveRecord)
        {
            return false;
        }
    }
}
