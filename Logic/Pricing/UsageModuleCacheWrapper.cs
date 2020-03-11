using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
    * 1. This class uses a decorator in order to wrap the BaseUsageModule class. Understand Decorator Design Pattern before you change anything.
    * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
    * 3. Methods not called by CAS do not cache their results right now (September 2014).
    * 
    */
    public class UsageModuleCacheWrapper : BaseUsageModuleDecorator
    {

        protected static readonly string USAGE_MODULE_CACHE_NAME = "um_data";
        protected static readonly string UM_CACHE_WRAPPER_LOG_FILE = "UsageModuleCacheWrapper";

        public UsageModuleCacheWrapper(BaseUsageModule originalBaseUsageModule)
            : base(originalBaseUsageModule)
        {

        }

        private string GetUsageModuleCacheKey(string usageModuleCode)
        {
            return String.Concat(originalBaseUsageModule.GroupID, "_", USAGE_MODULE_CACHE_NAME, "_", usageModuleCode);
        }

        public override UsageModule GetUsageModuleData(string sUsageModuleCode)
        {
            UsageModule res = null;
            if (!string.IsNullOrEmpty(sUsageModuleCode))
            {
                UsageModule temp = null;
                string cacheKey = GetUsageModuleCacheKey(sUsageModuleCode);
                if (PricingCache.TryGetUsageModule(cacheKey, out temp) && temp != null)
                    return temp;
                res = originalBaseUsageModule.GetUsageModuleData(sUsageModuleCode);
                if (res != null)
                {
                    if (!PricingCache.TryAddUsageModule(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetUsageModuleData",
                            UM_CACHE_WRAPPER_LOG_FILE);
                    }
                }

            }

            return res;
        }

        public override UsageModule GetOfflineUsageModuleData()
        {
            return this.originalBaseUsageModule.GetOfflineUsageModuleData();
        }

        public override UsageModule[] GetUsageModuleList()
        {
            return this.originalBaseUsageModule.GetUsageModuleList();
        }

        public override UsageModule[] GetSubscriptionUsageModuleList(string nSubscitionnSubscriptionCode)
        {
            return this.originalBaseUsageModule.GetSubscriptionUsageModuleList(nSubscitionnSubscriptionCode);
        }

    }
}
