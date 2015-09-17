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

        public RenewTransactionData(int groupId, string siteGuid, long purchaseId, string billingGuid, DateTime nextRenewDate) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = nextRenewDate;
            this.billingGuid = billingGuid;
            this.siteGuid = siteGuid;
            this.purchaseId = purchaseId;

            this.args = new List<object>()
            {
                groupId,
                billingGuid,
                siteGuid,
                purchaseId
            };
        }
    }
}
