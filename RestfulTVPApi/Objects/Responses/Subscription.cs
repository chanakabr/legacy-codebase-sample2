using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class Subscription : PPVModule
    {
        public SubscriptionCodeContainer[] codes { get; set; }

        public System.DateTime start_date { get; set; }

        public System.DateTime end_date { get; set; }

        public int[] file_types { get; set; }

        public bool is_recurring { get; set; }

        public int number_of_rec_periods { get; set; }

        public PriceCode subscription_price_code { get; set; }

        public DiscountModule ext_disount_module { get; set; }

        public LanguageContainer[] name { get; set; }

        public UsageModule subscription_usage_module { get; set; }

        public int fictivic_media_id { get; set; }

        public long priority { get; set; }

        public string subscription_product_code { get; set; }

        public string subscription_code { get; set; }

        public UsageModule[] multi_subscription_usage_module { get; set; }

        public int geo_commerce_id { get; set; }

        public bool is_infinite_recurring { get; set; }

        public PreviewModule preview_module { get; set; }

        public UserType[] user_types { get; set; }
    }
}
