using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Modules
{
    public class UnifiedPaymentRenewal
    {
        public DateTime Date { get; set; }

        public Price Price { get; set; }

        public long UnifiedPaymentId { get; set; }

        public List<EntitlementRenewalBase> Entitlements { get; set; }
    }

    public class EntitlementRenewal
    {
        public DateTime Date { get; set; }

        public Price Price { get; set; }

        public long PurchaseId { get; set; }

        public long SubscriptionId { get; set; }
    }

    public class EntitlementRenewalBase
    {
        public long PurchaseId { get; set; }

        public long SubscriptionId { get; set; }

        public double PriceAmount { get; set; }
    }
}
