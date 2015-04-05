using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PPVModule
    {
        /// <remarks/>
        public PriceCode price_code { get; set; }

        public UsageModule usage_module { get; set; }

        public DiscountModule discount_module { get; set; }

        public CouponsGroup coupons_group { get; set; }

        public LanguageContainer[] description { get; set; }

        public string object_code { get; set; }

        public string object_virtual_name { get; set; }

        public bool subscription_only { get; set; }

        public int[] related_file_types { get; set; }

        public string product_code{ get; set; }

        public bool first_device_limitation { get; set; }
    }
}
