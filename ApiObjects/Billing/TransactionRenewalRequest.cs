using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class TransactionRenewalRequest : TransactionRequest
    {
        public string ExternalTransactionId { get; set; }
        public int GracePeriodMinutes { get; set; }
    }
}
