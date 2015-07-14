using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWSettings
    {
        public string key { get; set; }
        public string value { get; set; }       

        public PaymentGWSettings()
        {
        }

        public PaymentGWSettings(string key, string value)
        {            
            this.key = key;
            this.value = value;
        }
    }
}
