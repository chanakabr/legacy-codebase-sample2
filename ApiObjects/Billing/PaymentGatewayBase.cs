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

        public PaymentGatewayBase()
        {
        }

        public PaymentGatewayBase(PaymentGatewayBase paymentGatewayBase)
        {
            this.ID = paymentGatewayBase.ID;
            this.Name = paymentGatewayBase.Name;
            this.IsDefault = paymentGatewayBase.IsDefault;
        }

        public PaymentGatewayBase(int id, string name, bool isDefault)
        {
            this.ID = id;
            this.Name = name;
            this.IsDefault = isDefault;
        }
    }
}
