using System;

namespace ApiObjects.Billing
{
    public class PaymentGWPending
    {
        public int ID { get; set; }
        public int PaymentGWTransactionId { get; set; }
        public DateTime NextRetryDate { get; set; }
        public int AdapterRetryCount { get; set; }

        public PaymentGWPending() { }

        public PaymentGWPending(PaymentGWPending paymentGWPending)
        {
            this.AdapterRetryCount = paymentGWPending.AdapterRetryCount;
            this.ID = paymentGWPending.ID;
            this.NextRetryDate = paymentGWPending.NextRetryDate;
        }
    }
}
