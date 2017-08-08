using ApiObjects.Billing;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Managers
{
    public class UnifiedBillingCycleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 3;

        #region SingleTon

        private static object locker = new object();
        private static UnifiedBillingCycleManager instance;

        public static UnifiedBillingCycleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new UnifiedBillingCycleManager();
                        }
                    }
                }

                return instance;
            }
        }
        
        #endregion  

        #region Public Methods
        public UnifiedBillingCycle GetDomainUnifiedBillingCycle(int domainId, long billingCycle)
        {
            UnifiedBillingCycle unifiedBillingCycle = null;
            try
            {
                bool result = DAL.BillingDAL.GetDomainUnifiedBillingCycle(domainId, billingCycle, out unifiedBillingCycle);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in 'GetDomainUnifiedBillingCycle' for domainId = {0}, billingCycle = {1}", domainId, billingCycle), ex);
                return new UnifiedBillingCycle();
            }

            return unifiedBillingCycle;
        }

             

        public bool SetDomainUnifiedBillingCycle(long domainId, long billingCycle, long endDate, List<int> paymentGateway)
        {            
            bool result = false;
            UnifiedBillingCycle unifiedBillingCycle = new UnifiedBillingCycle()
            {
                endDate = endDate,
                paymentGatewayIds = paymentGateway
            };

            result = DAL.BillingDAL.SetDomainUnifiedBillingCycle(domainId, billingCycle, unifiedBillingCycle);
          
            return result;
        }
        #endregion

        #region Private Methods

        public bool DeleteDomainUnifiedBillingCycle(int domainId, long billingCycle)
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
