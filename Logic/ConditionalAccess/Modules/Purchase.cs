using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess.Modules
{
    public abstract class Purchase : CoreObject
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());        

        #region members

        public string siteGuid { get; set; }
        public string currency { get; set; }
        public string customData { get; set; }
        public string country { get; set; }
        public string deviceName { get; set; }
        public string billingGuid { get; set; }
        
        public double price { get; set; }

        public long billingTransactionId { get; set; }
        public long houseHoldId { get; set; }
        public long purchaseId { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        public abstract eTransactionType type { get; }

        #endregion

        public Purchase(int groupId)
        {
            this.GroupId = groupId;
        }

        public abstract override CoreObject CoreClone();
    }
}
