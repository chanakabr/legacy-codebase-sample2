using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class Collection : PPVModule
    {
        public BundleCodeContainer[] codes { get; set; }

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public int[] file_types { get; set; }

        public PriceCode collection_price_code { get; set; }

        public DiscountModule ext_discount_module { get; set; }

        public LanguageContainer[] name { get; set; }

        public UsageModule collection_usage_module { get; set; }

        public int fictivic_media_iD { get; set; }

        public string collection_product_code { get; set; }

        public string collection_code { get; set; }
    }
}
