using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWConfigs
    {
        public string key { get; set; }
        public string value { get; set; }       

        public PaymentGWConfigs()
        {
        }

        public PaymentGWConfigs(string key, string value)
        {            
            this.key = key;
            this.value = value;
        }
    }
}
