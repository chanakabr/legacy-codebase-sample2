using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    /// <summary>
    /// Object that defines a check pending transaction request via the adapters controller
    /// </summary>
    public class PendingTransactionRequest
    {
        public int groupId;
        public PaymentGateway paymentGateway;
        public int productId;
        public eTransactionType productType;
        public string siteGuid;
        public long pendingTransactionId;
        public string pendingExternalTransactionId;
    }
}
