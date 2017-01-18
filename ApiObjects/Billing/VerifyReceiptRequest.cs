using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class VerifyReceiptRequest : TransactionRequest
    {
        public string purchaseToken;
        public string paymentGatewayType;
        public string productCode;
    }
}
