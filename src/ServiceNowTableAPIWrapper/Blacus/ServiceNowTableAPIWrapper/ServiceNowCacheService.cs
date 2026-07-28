using System;

namespace Blacus.ServiceNowTableAPIWrapper
{
    /// <summary>
    /// Default implementation of <see cref="IServiceNowCacheService"/>.
    /// </summary>
    public class ServiceNowCacheService : IServiceNowCacheService
    {
        /// <summary>
        /// Gets or sets the function to retrieve cached JSON data by key.
        /// </summary>
        public Func<string, string>? CacheGet { get; set; } = null;

        /// <summary>
        /// Gets or sets the action to store updated JSON data by key.
        /// </summary>
        public Action<string, string>? CacheUpdate { get; set; } = null;

        /// <summary>
        /// Gets or sets optional post-processing applied to retrieved model instances.
        /// </summary>
        public Action<IServiceNowTableModel>? PostProcessing { get; set; } = (record) => { };

        /// <summary>
        /// Gets or sets the comparative function between a cached record and a live record.
        /// </summary>
        public Func<IServiceNowTableModel?, IServiceNowTableModel, bool>? Compare { get; set; } =
            (cacheRecord, liveRecord) => IServiceNowCacheService.CompareDefault(cacheRecord, liveRecord);

        /// <summary>
        /// Gets or sets the field names retrieved when querying record headers for cache synchronization.
        /// </summary>
        public string[] CacheFields { get; set; } = { "sys_id", "sys_updated_on" };
    }
}
