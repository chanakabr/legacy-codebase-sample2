using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGWConfig
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool isDefault { get; set; }
        public int isActive { get; set; }
        public List<PaymentGWConfigs> configs { get; set; }

        public PaymentGWConfig()
        {
        }

        public PaymentGWConfig(PaymentGWConfig pgw)
        {
            this.id = pgw.id;
            this.name = pgw.name;
            this.isDefault = pgw.isDefault;
            this.isActive = pgw.isActive;
            this.configs = pgw.configs;
        }
    }
}
