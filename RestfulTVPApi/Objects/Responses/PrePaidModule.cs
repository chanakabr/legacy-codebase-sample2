using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PrePaidModule
    {
        public PriceCode price_code { get; set; }

        public PriceCode credit_value { get; set; }

        public UsageModule usage_module { get; set; }

        public DiscountModule discount_module { get; set; }

        public CouponsGroup coupons_group { get; set; }

        public LanguageContainer[] description { get; set; }

        public int object_code { get; set; }

        public string title { get; set; }

        public bool is_fixed_credit { get; set; }
    }
}
