using ApiObjects.Billing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Managers
{
    public static class UnifiedBillingCycleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 3;

        #region Public Methods
        public static UnifiedBillingCycle GetDomainUnifiedBillingCycle(int domainId, long billingCycle)
        {
            UnifiedBillingCycle unifiedBillingCycle = null;
            try
            {
                string key = DAL.UtilsDal.GetDomainUnifiedBillingCycleKey(domainId, billingCycle);
                bool isReadAction = LayeredCache.Instance.IsReadAction();
                if (isReadAction && LayeredCache.Instance.TryGetKeyFromSession<UnifiedBillingCycle>(key, ref unifiedBillingCycle))
                {
                    if (unifiedBillingCycle.endDate == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return unifiedBillingCycle;
                    }
                }
                else
                {
                    DAL.BillingDAL.GetDomainUnifiedBillingCycle(domainId, billingCycle, out unifiedBillingCycle);
                }

                if (isReadAction)
                {
                    Dictionary<string, UnifiedBillingCycle> resultsToAdd = new Dictionary<string, UnifiedBillingCycle>();
                    resultsToAdd.Add(key, unifiedBillingCycle != null ? unifiedBillingCycle : new UnifiedBillingCycle() { endDate = 0 });
                    LayeredCache.Instance.InsertResultsToSession<UnifiedBillingCycle>(resultsToAdd);

                    if (unifiedBillingCycle.endDate == 0)
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in 'GetDomainUnifiedBillingCycle' for domainId = {0}, billingCycle = {1}", domainId, billingCycle), ex);
                return new UnifiedBillingCycle();
            }

            return unifiedBillingCycle;
        }

        public static bool SetDomainUnifiedBillingCycle(long domainId, long billingCycle, long endDate)
        {            
            bool result = false;
            UnifiedBillingCycle unifiedBillingCycle = new UnifiedBillingCycle()
            {
                endDate = endDate,
            };

            result = DAL.BillingDAL.SetDomainUnifiedBillingCycle(domainId, billingCycle, unifiedBillingCycle);
          
            return result;
        }
      
        public static bool DeleteDomainUnifiedBillingCycle(int domainId, long billingCycle)
        {
            bool result = false;
            try
            {
                result = DAL.BillingDAL.DeleteDomainUnifiedBillingCycle(domainId, billingCycle);
            }
            catch (Exception ex)
            {
                result = false;
                log.Error(string.Format("Error in 'DeleteDomainUnifiedBillingCycle' for domainId = {0}, billingCycle = {1}", domainId, billingCycle), ex);
            }

            return result;
        }

        #endregion

    }
}
