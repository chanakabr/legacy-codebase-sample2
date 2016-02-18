using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayBase
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool SupportPaymentMethod { get; set; }

        public PaymentGatewayBase()
        {
        }

        public PaymentGatewayBase(PaymentGatewayBase paymentGatewayBase)
        {
            this.ID = paymentGatewayBase.ID;
            this.Name = paymentGatewayBase.Name;
            this.IsDefault = paymentGatewayBase.IsDefault;
            this.SupportPaymentMethod = paymentGatewayBase.SupportPaymentMethod;
        }

        public PaymentGatewayBase(int id, string name, bool isDefault, bool supportPaymentMethod)
        {
            this.ID = id;
            this.Name = name;
            this.IsDefault = isDefault;
            this.SupportPaymentMethod = supportPaymentMethod;
        }
    }
}
