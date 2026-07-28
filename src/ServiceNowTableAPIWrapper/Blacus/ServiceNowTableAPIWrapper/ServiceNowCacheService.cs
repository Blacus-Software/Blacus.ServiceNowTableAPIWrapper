namespace Blacus.ServiceNowTableAPIWrapper
{
    using System;

    public class ServiceNowCacheService : IServiceNowCacheService
    {
        public Func<string, string>? CacheGet { get; set; } = null;
        public Action<string, string>? CacheUpdate { get; set; } = null;
        public Action<IServiceNowTableModel>? PostProcessing { get; set; } = (record) => { };
        public Func<IServiceNowTableModel?, IServiceNowTableModel, bool>? Compare { get; set; } =
            (cacheRecord, liveRecord) => IServiceNowCacheService.CompareDefault(cacheRecord, liveRecord);

        public string[] CacheFields { get; set; } = { "sys_id", "sys_updated_on" };
    }
}
