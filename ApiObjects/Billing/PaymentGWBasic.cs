using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWBasic
    {
        public int id { get; set; }
        public string name { get; set; }

        public PaymentGWBasic()
        {
        }

        public PaymentGWBasic(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
