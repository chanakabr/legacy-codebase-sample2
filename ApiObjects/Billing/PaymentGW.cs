using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiObjects.Billing
{
    public class PaymentGW
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string TransactUrl { get; set; }
        public string StatusUrl { get; set; }
        public string RenewUrl { get; set; }
        public string ExternalIdentifier { get; set; }
        public int PendingInterval { get; set; }
        public int PendingRetries { get; set; }
        public string SharedSecret { get; set; }
        [XmlIgnore]
        public int Status { get; set; }
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
            this.AdapterUrl = pgw.AdapterUrl;
            this.TransactUrl = pgw.TransactUrl;
            this.StatusUrl = pgw.StatusUrl;
            this.RenewUrl = pgw.RenewUrl;
            this.ExternalIdentifier = pgw.ExternalIdentifier;
            this.PendingInterval = pgw.PendingInterval;
            this.PendingRetries = pgw.PendingRetries;
            this.SharedSecret = pgw.SharedSecret;
            this.Status = pgw.Status;
            this.Settings = pgw.Settings;
        }
    }
}
