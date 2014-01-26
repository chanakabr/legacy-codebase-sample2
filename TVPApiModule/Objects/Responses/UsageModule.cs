using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UsageModule
    {
        public int objectID { get; set; }

        public string virtualName { get; set; }

        public int maxNumberOfViews { get; set; }

        public int viewLifeCycle { get; set; }

        public int maxUsageModuleLifeCycle { get; set; }

        public int extDiscountId { get; set; }

        public int internalDiscountID { get; set; }

        public int pricingID { get; set; }

        public int couponID { get; set; }

        public int type { get; set; }

        public int subscriptionOnly { get; set; }

        public int isRenew { get; set; }

        public int numOfRecPeriods { get; set; }

        public int deviceLimitID { get; set; }
    }
}
