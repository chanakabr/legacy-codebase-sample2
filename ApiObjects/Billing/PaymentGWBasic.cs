using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWBasic
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public PaymentGWBasic()
        {
        }

        public PaymentGWBasic(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
