using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    /// <summary>
    /// Object that defines performing a transaction via adapters controller
    /// </summary>
    public class TransactionRequest
    {
        public int groupId;
        public PaymentGateway paymentGateway;
        public string chargeId;
        public string paymentMethodExternalId;
        public double price;
        public string currency;
        public string userIP;
        public int productId;
        public eTransactionType productType;
        public int contentId;
        public string siteGuid;
        public long householdID;
        public string billingGuid;
        public string customData;
        public string productCode;
        public string adapterData;
    }
}
