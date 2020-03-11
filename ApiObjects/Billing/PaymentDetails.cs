using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Billing
{
    public class PaymentDetails
    {
        public int PaymentGatewayId { get; set; }
        public int PaymentMethodId { get; set; }
        public string TransactionId { get; set; }
        public string BillingGuid { get; set; }
        public bool isRenewDetails { get; set; }

        public PaymentDetails()
        {
        }
    }
}
