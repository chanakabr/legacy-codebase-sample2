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

    public class PaymentMethodResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }

    public class PaymentMethodsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<PaymentMethod> PaymentMethods { get; set; }
    }
}
