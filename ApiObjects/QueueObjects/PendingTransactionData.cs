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

        public const string TASK = "distributed_tasks.check_pending_transactions";
        
        #endregion

        #region Data Members

        private long pendingTransactionId;
        private int numberOfRetries;
        private string billingGuid;
        private long paymengGatewayTransactionId;

        #endregion

        public PendingTransactionData(int groupId, PaymentGatewayPending pendingTransaction, 
            string siteGuid, long productId, int productType)
            : base(
                // id = guid
                Guid.NewGuid().ToString(),
                // task = const
                TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.pendingTransactionId = pendingTransaction.ID;
            this.numberOfRetries = pendingTransaction.AdapterRetryCount;
            this.ETA = pendingTransaction.NextRetryDate;
            this.billingGuid = pendingTransaction.BillingGuid;
            this.paymengGatewayTransactionId = pendingTransaction.PaymentGatewayTransactionId;

            this.args = new List<object>()
            {
                pendingTransactionId,
                numberOfRetries,
                productId,
                productType,
                billingGuid,
                paymengGatewayTransactionId
            };
        }
    }
}
