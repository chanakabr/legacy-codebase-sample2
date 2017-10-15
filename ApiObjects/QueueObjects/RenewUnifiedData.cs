using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class RenewUnifiedData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_unified_renew_subscription";
                
        private long householdId;
        private long processId;
        private long endDate;
        private eSubscriptionRenewRequestType type;

        public RenewUnifiedData(int groupId, long householdId, long processId, long endDate, DateTime nextRenewDate,
            eSubscriptionRenewRequestType type = eSubscriptionRenewRequestType.RenewUnifiedTransaction) :
                base(// id = household+processPurchasesId+date
                     string.Format("h{0}_p{1}_d{2}", householdId, processId, endDate),
                     // task = const
                     TASK)
            {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = nextRenewDate;
            this.householdId = householdId;
            this.processId = processId;
            this.endDate = endDate;
            this.type = type;

            this.args = new List<object>()
            {
                groupId,
                householdId,
                processId,
                endDate,
                type,
                base.RequestId
            };
        }
    }
}
