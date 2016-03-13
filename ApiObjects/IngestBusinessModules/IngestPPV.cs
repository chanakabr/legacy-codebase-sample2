using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.IngestBusinessModules
{
    public class IngestPPV : IngestModule
    {
        public string PriceCode { get; set; }

        public string PricePlan { get; set; }

        public string Discount { get; set; }

        public string GroupCoupon { get; set; }

        public bool SubscriptionOnly { get; set; }

        public string Title { get; set; }

        public bool FirstDeviceLimitation { get; set; }

        public string ProductCode { get; set; }

    }
}
