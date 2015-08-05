using System;

namespace ApiObjects.Billing
{
    public class PaymentGatewayPending
    {
        public int ID { get; set; }
        public int PaymentGatewayTransactionId { get; set; }
        public DateTime NextRetryDate { get; set; }
        public int AdapterRetryCount { get; set; }
        public string BillingGuid { get; set; }

        public PaymentGatewayPending() { }

        public PaymentGatewayPending(PaymentGatewayPending paymentGWPending)
        {
            this.AdapterRetryCount = paymentGWPending.AdapterRetryCount;
            this.ID = paymentGWPending.ID;
            this.NextRetryDate = paymentGWPending.NextRetryDate;
            this.BillingGuid = paymentGWPending.BillingGuid;
        }
    }
}
