using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewaySettings
    {
        public string key { get; set; }
        public string value { get; set; }       

        public PaymentGatewaySettings()
        {
        }

        public PaymentGatewaySettings(string key, string value)
        {            
            this.key = key;
            this.value = value;
        }
    }
}
