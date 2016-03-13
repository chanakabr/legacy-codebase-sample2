using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class IngestPPV : IngestModule
    {
        public string PriceCode { get; set; }

        public string PricePlan { get; set; }

        public string Discount { get; set; }

        public string GroupCoupon { get; set; }
            
        public bool SubscriptionOnly { get; set; }

        public List<KeyValuePair> Descriptions { get; set; } // lang, description

        public bool FirstDeviceLimitation { get; set; }

        public string ProductCode { get; set; }

        public List<string> FileTypes { get; set; }

    }
}
