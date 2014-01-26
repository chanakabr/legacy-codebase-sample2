using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PermittedSubscriptionContainer
    {
        public string subscriptionCode { get; set; }

        public int maxUses { get; set; }

        public int currentUses { get; set; }

        public DateTime endDate { get; set; }

        public DateTime currentDate { get; set; }

        public DateTime lastViewDate { get; set; }

        public DateTime purchaseDate { get; set; }

        public DateTime nextRenewalDate { get; set; }

        public bool recurringStatus { get; set; }

        public bool isSubRenewable { get; set; }

        public int subscriptionPurchaseID { get; set; }

        public PaymentMethod paymentMethod { get; set; }

        public string deviceUDID { get; set; }

        public string deviceName { get; set; }
    }

}
