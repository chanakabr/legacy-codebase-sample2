using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Subscription : PPVModule
    {
        public SubscriptionCodeContainer[] codes { get; set; }

        public System.DateTime startDate { get; set; }

        public System.DateTime endDate { get; set; }

        public int[] fileTypes { get; set; }

        public bool isRecurring { get; set; }

        public int numberOfRecPeriods { get; set; }

        public PriceCode subscriptionPriceCode { get; set; }

        public DiscountModule extDisountModule { get; set; }

        public LanguageContainer[] name { get; set; }

        public UsageModule subscriptionUsageModule { get; set; }

        public int fictivicMediaID { get; set; }

        public long priority { get; set; }

        public string productCode { get; set; }

        public string subscriptionCode { get; set; }

        public UsageModule[] multiSubscriptionUsageModule { get; set; }

        public int geoCommerceID { get; set; }

        public bool isInfiniteRecurring { get; set; }

        public PreviewModule previewModule { get; set; }
    }
}
