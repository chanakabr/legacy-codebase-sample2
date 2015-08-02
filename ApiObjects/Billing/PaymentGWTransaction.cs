using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWTransaction
    {
        public int ID { get; set; }
        public int PaymentGWId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string ExternalStatus { get; set; }
        public int ProductType { get; set; }
        public int ProductId { get; set; }
        public string BillingGuid { get; set; }
        public int ContentId { get; set; }
        public string AdapterMessage { get; set; }        
        public string Message { get; set; }        
                
        public PaymentGWTransaction() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentGWTransaction"></param>
        public PaymentGWTransaction(PaymentGWTransaction paymentGWTransaction)
        {
            this.ID = paymentGWTransaction.ID;
            this.PaymentGWId = paymentGWTransaction.PaymentGWId;
            this.ExternalTransactionId = paymentGWTransaction.ExternalTransactionId;
            this.ExternalStatus = paymentGWTransaction.ExternalStatus;
            this.ProductId = paymentGWTransaction.ProductId;
            this.ProductType = paymentGWTransaction.ProductType;
            this.BillingGuid = paymentGWTransaction.BillingGuid;
            this.ContentId = paymentGWTransaction.ContentId;
            this.AdapterMessage = paymentGWTransaction.AdapterMessage;
            this.Message = paymentGWTransaction.Message;
        }
    }
}
