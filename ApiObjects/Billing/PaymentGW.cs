using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGW
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int IsActive { get; set; }
        public string Url { get; set; }
        public string ExternalIdentifier { get; set; }
        public List<PaymentGWSettings> Settings { get; set; }

        public PaymentGW()
        {
        }

        public PaymentGW(PaymentGW pgw)
        {
            this.ID = pgw.ID;
            this.Name = pgw.Name;
            this.IsDefault = pgw.IsDefault;
            this.IsActive = pgw.IsActive;
            this.Url = pgw.Url;
            this.ExternalIdentifier = pgw.ExternalIdentifier;
            this.Settings = pgw.Settings;
        }
    }
}
