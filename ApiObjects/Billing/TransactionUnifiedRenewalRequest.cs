using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Billing
{
    public class TransactionUnifiedRenewalRequest
    {
        public int groupId;
        public PaymentGateway paymentGateway;
        public string chargeId;
        public string paymentMethodExternalId;

        public string userIP;
        public long householdId;

        public double totalPrice;
        public string currency;
        public List<TransactionUnifiedRenewalDetails> renewRequests { get; set; }
    }
}
