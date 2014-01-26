using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class ItemPriceContainer
    {
        public string ppv_module_code { get; set; }

        public bool subscription_only { get; set; }

        public Price price { get; set; }

        public Price full_price { get; set; }

        public PriceReason price_reason { get; set; }

        public Subscription relevant_sub { get; set; }

        public PrePaidModule relevant_pp { get; set; }

        public LanguageContainer[] ppv_description { get; set; }

        public CouponsStatus coupon_status { get; set; }
    }
}
