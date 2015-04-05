using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class ItemPriceContainer
    {
        public string ppv_module_code { get; set; }

        public bool subscription_only { get; set; }

        public Price price { get; set; }

        public Price full_price { get; set; }

        public PriceReason price_reason { get; set; }

        public Subscription relevant_sub { get; set; }

        public Collection relevant_collection { get; set; }

        public PrePaidModule relevant_pp { get; set; }

        public LanguageContainer[] ppv_description { get; set; }

        public CouponsStatus coupon_status { get; set; }

        public string first_device_name_found { get; set; }

        public bool is_cancel_window { get; set; }
    }
}
