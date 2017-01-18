using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiObjects.Billing
{
    public class PaymentGateway : PaymentGatewayBase
    {
        public int IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string TransactUrl { get; set; }
        public string StatusUrl { get; set; }
        public string RenewUrl { get; set; }
        public string ExternalIdentifier { get; set; }
        public int PendingInterval { get; set; }
        public int PendingRetries { get; set; }
        public int RenewalIntervalMinutes { get; set; }
        public int RenewalStartMinutes { get; set; }

        public string SharedSecret { get; set; }
        [XmlIgnore]
        public int Status { get; set; }
        [XmlIgnore]
        public int Selected { get; set; }

        public bool SkipSettings { get; set; }
        public List<PaymentGatewaySettings> Settings { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; }

        public PaymentGateway()
        {
        }

        public PaymentGateway(PaymentGateway paymentGateway)
        {
            this.ID = paymentGateway.ID;
            this.Name = paymentGateway.Name;
            this.IsDefault = paymentGateway.IsDefault;
            this.IsActive = paymentGateway.IsActive;
            this.AdapterUrl = paymentGateway.AdapterUrl;
            this.TransactUrl = paymentGateway.TransactUrl;
            this.StatusUrl = paymentGateway.StatusUrl;
            this.RenewUrl = paymentGateway.RenewUrl;
            this.ExternalIdentifier = paymentGateway.ExternalIdentifier;
            this.PendingInterval = paymentGateway.PendingInterval;
            this.PendingRetries = paymentGateway.PendingRetries;
            this.SharedSecret = paymentGateway.SharedSecret;
            this.Status = paymentGateway.Status;
            this.Selected = paymentGateway.Selected;
            this.Settings = paymentGateway.Settings;
            this.RenewalIntervalMinutes = paymentGateway.RenewalIntervalMinutes;
            this.RenewalStartMinutes = paymentGateway.RenewalStartMinutes;
            this.SupportPaymentMethod = paymentGateway.SupportPaymentMethod;
        }
    }
}
