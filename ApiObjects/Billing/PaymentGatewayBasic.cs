using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayBasic
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public PaymentGatewayBasic()
        {
        }

        public PaymentGatewayBasic(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
