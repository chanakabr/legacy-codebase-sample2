using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class RenewOneTransactionData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_one_transaction_renew_subscription";
                
        private long householdId;
        private int paymentgatewayId;
        private long endDate;
        private eSubscriptionRenewRequestType type;

        public RenewOneTransactionData(int groupId, long householdId, int paymentgatewayId, long endDate, DateTime nextRenewDate,
            eSubscriptionRenewRequestType type = eSubscriptionRenewRequestType.Renew) :
                base(// id = household+paymentgateway+date
                     string.Format("h{0}_p{1}_d{2}", householdId, paymentgatewayId, endDate),
                     // task = const
                     TASK)
            {
            // Basic member initialization
            this.GroupId = groupId;
            this.ETA = nextRenewDate;
            this.householdId = householdId;
            this.paymentgatewayId = paymentgatewayId;
            this.endDate = endDate;
            this.type = type;

            this.args = new List<object>()
            {
                groupId,
                householdId,
                paymentgatewayId,
                endDate,
                type,
                base.RequestId
            };
        }
    }
}
