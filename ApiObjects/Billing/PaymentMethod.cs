using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentMethod
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string PaymentMethodType { get; set; }
        public int Selected { get; set; }
    }
}
