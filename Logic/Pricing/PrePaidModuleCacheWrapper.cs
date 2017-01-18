using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
    * 1. This class uses a decorator in order to wrap the BasePrePaidModule class. Understand Decorator Design Pattern before you change anything.
    * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
    * 3. Methods not called by CAS do not cache their results right now (September 2014).
    * 
    */
    public class PrePaidModuleCacheWrapper : BasePrePaidModuleDecorator
    {

        protected static readonly string PP_MODULE_CACHE_WRAPPER_LOG_FILE = "PrePaidModuleCacheWrapper";
        protected static readonly string PP_DATA_CACHE_NAME = "pp_data";

        public PrePaidModuleCacheWrapper(BasePrePaidModule originalBasePrePaidModule)
            : base(originalBasePrePaidModule)
        {

        }

        private string GetPPDataCacheKey(int prePaidModuleCode) 
        {
            return String.Concat(originalBasePrePaidModule.GroupID, "_", PP_DATA_CACHE_NAME, "_", prePaidModuleCode);
        }

        public override PrePaidModule GetPrePaidModuleData(int nPrePaidModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PrePaidModule res = null;
            if (nPrePaidModuleCode > 0)
            {
                string cacheKey = GetPPDataCacheKey(nPrePaidModuleCode);
                PrePaidModule temp = null;
                if (PricingCache.TryGetPrePaidModule(cacheKey, out temp) && temp != null)
                    return temp;
                res = originalBasePrePaidModule.GetPrePaidModuleData(nPrePaidModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (res != null)
                {
                    if (!PricingCache.TryAddPrePaidModule(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetPrePaidModuleData",
                            PP_MODULE_CACHE_WRAPPER_LOG_FILE);
                    }
                }
            }

            return res;
        }
    }
}
