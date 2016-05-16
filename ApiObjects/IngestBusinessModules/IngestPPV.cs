using ApiObjects.IngestBusinessModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{ 
    public class IngestPPV : IngestModule
    {
        public IngestPriceCode PriceCode { get; set; }

        public string UsageModule { get; set; }

        public string Discount { get; set; }

        public string CouponGroup { get; set; }
            
        public bool? SubscriptionOnly { get; set; }

        public List<KeyValuePair> Descriptions { get; set; } // lang, description

        public bool? FirstDeviceLimitation { get; set; }

        public string ProductCode { get; set; }

        public List<string> FileTypes { get; set; }

        public bool? IsActive { get; set; }
        
    }
}
