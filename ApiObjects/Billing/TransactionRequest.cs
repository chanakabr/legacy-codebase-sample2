using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class TransactionRequest
    {
        public PaymentGateway paymentGateway;
        public string chargeId;
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
    }
}
