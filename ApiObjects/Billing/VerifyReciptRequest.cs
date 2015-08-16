using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class VerifyReciptRequest : TransactionRequest
    {
        public string purchaseToken;
        public string paymentGatewayType;
    }
}
