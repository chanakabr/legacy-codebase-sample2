using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PendingTransactionData :  BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_check_pending_transaction";
        
        #endregion

        #region Data Members

        private long paymentGatewayPendingId;
        private int numberOfRetries;
        private string billingGuid;
        private long paymentGatewayTransactionId;
        private string siteGuid;

        #endregion

        public PendingTransactionData(int groupId, PaymentGatewayPending paymentGatewayPending, 
            string siteGuid, long productId, int productType)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.paymentGatewayPendingId = paymentGatewayPending.ID;
            this.numberOfRetries = paymentGatewayPending.AdapterRetryCount;
            this.ETA = paymentGatewayPending.NextRetryDate;
            this.billingGuid = paymentGatewayPending.BillingGuid;
            this.paymentGatewayTransactionId = paymentGatewayPending.PaymentGatewayTransactionId;
            this.siteGuid = siteGuid;

            this.args = new List<object>()
            {
                groupId,
                paymentGatewayPendingId,
                numberOfRetries,
                billingGuid,
                paymentGatewayTransactionId,
                siteGuid,
                productId,
                productType
            };
        }
    }
}
