using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
    * 1. This class uses a decorator in order to wrap the BasePricing class. Understand Decorator Design Pattern before you change anything.
    * 2. Its main functionality is to add caching mechanism to Pricing methods uses by the Conditional Access module.
    * 3. Methods not called by CAS do not cache their results right now (September 2014).
    * 
    */
    public class PricingCacheWrapper : BasePricingDecorator
    {
        protected static readonly string PRICE_DATA_CACHE_NAME = "pc_data";
        protected static readonly string PRICING_CACHE_WRAPPER_LOG_FILE = "PricingCacheWrapper";

        public PricingCacheWrapper(BasePricing originalBasePricing)
            : base(originalBasePricing)
        {

        }

        public override PriceCode GetPriceCodeData(string sPC, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PriceCode res = null;
            if (!string.IsNullOrEmpty(sPC))
            {
                string cacheKey = GetPriceCodeDataCacheKey(sPC);
                PriceCode temp = null;
                if (PricingCache.TryGetPriceCode(cacheKey, out temp) && temp != null)
                    return temp;
                res = originalBasePricing.GetPriceCodeData(sPC, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (res != null)
                {
                    if (!PricingCache.TryAddPriceCode(cacheKey, res))
                    {
                        PricingCache.LogCachingError("Failed to insert entry into cache. ", cacheKey, res, "GetPriceCodeData",
                            PRICING_CACHE_WRAPPER_LOG_FILE);
                    }
                }
            }

            return res;
        }

        private string GetPriceCodeDataCacheKey(string priceCode)
        {
            return String.Concat(originalBasePricing.GroupID, "_", PRICE_DATA_CACHE_NAME, "_", priceCode);
        }

        public override PriceCode[] GetPriceCodeList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return originalBasePricing.GetPriceCodeList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }
    }
}
