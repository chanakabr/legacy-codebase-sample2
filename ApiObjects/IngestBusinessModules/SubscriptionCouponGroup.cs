using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.IngestBusinessModules
{
    public class SubscriptionCouponGroup
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string CouponGroup { get; set; }
    }
}
