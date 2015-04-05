using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class CouponsGroup
    {
        public DiscountModule discount_module { get; set; }

        public string discount_code { get; set; }

        public LanguageContainer[] description { get; set; }

        public System.DateTime start_date { get; set; }

        public System.DateTime end_date { get; set; }

        public int max_use_count_for_coupon { get; set; }

        public string group_code { get; set; }

        public string group_name { get; set; }

        public int financial_entity_id { get; set; }

        public int max_recurring_uses_count_for_coupon { get; set; }
    }
}
