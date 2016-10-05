using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class RenewTransactionData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_renew_subscription";

        private long purchaseId;
        private string billingGuid;
        private string siteGuid;
        private long endDate;

        public RenewTransactionData(int groupId, string siteGuid, long purchaseId, string billingGuid, long endDate, DateTime nextRenewDate) :
            base(// id = guid
                 string.Format("p{0}_d{1}", purchaseId, endDate),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = nextRenewDate;
            this.billingGuid = billingGuid;
            this.siteGuid = siteGuid;
            this.purchaseId = purchaseId;
            this.endDate = endDate;

            this.args = new List<object>()
            {
                groupId,
                billingGuid,
                siteGuid,
                purchaseId,
                endDate
            };
        }
    }
}
