using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PendingTransactionRequest
    {
        public PaymentGateway paymentGateway;
        public int productId;
        public eTransactionType productType;
        public string siteGuid;
    }
}
