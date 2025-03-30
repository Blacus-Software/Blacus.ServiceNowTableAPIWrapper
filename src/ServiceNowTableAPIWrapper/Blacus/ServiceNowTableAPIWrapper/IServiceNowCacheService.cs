using System;

namespace Blacus.ServiceNowTableAPIWrapper
{

    public interface IServiceNowCacheService
    {

        public Func<string, string> CacheGet { get; set; }
        public Action<string, string> CacheUpdate { get; set; }
        public Action<IServiceNowTableModel> PostProcessing { get; set; }
        public Func<IServiceNowTableModel, IServiceNowTableModel, bool> Compare { get; set; }
        public string[] CacheFields { get; set; }

        public static bool CompareDefault(IServiceNowTableModel cacheRecord, IServiceNowTableModel liveRecord)
        {

            return false;

            //try
            //{
            //    if (cacheRecord == null || cacheRecord.SysUpdatedOn == null || cacheRecord.SysUpdatedOn != liveRecord.SysUpdatedOn)
            //        return false;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}

            //return true;

        }

    }

}
