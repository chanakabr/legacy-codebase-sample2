using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayTransaction
    {
        public int ID { get; set; }
        public int PaymentGatewayID { get; set; }
        public string ExternalTransactionId { get; set; }
        public string ExternalStatus { get; set; }
        public int ProductType { get; set; }
        public int ProductId { get; set; }
        public string BillingGuid { get; set; }
        public int ContentId { get; set; }        
        public string Message { get; set; }
        public int State { get; set; }
        public int FailReason { get; set; }        
        public int PaymentMethodId{ get; set; }

        public string PaymentDetails
        {
            get;
            set;
        }

        public string PaymentMethod
        {
            get;
            set;
        }

        public PaymentGatewayTransaction() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        public PaymentGatewayTransaction(PaymentGatewayTransaction original)
        {
            this.ID = original.ID;
            this.PaymentGatewayID = original.PaymentGatewayID;
            this.ExternalTransactionId = original.ExternalTransactionId;
            this.ExternalStatus = original.ExternalStatus;
            this.ProductId = original.ProductId;
            this.ProductType = original.ProductType;
            this.BillingGuid = original.BillingGuid;
            this.ContentId = original.ContentId;            
            this.Message = original.Message;
            this.State = original.State;
            this.FailReason = original.FailReason;
            this.PaymentMethodId = original.PaymentMethodId;
        }
    }
}
