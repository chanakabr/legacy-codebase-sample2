using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGW
    {
        public int id { get; set; }
        public string name { get; set; }

        public PaymentGW()
        {
        }

        public PaymentGW(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
