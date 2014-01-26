using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class CouponsGroup
    {
        public DiscountModule discountModule { get; set; }

        public string discountCode { get; set; }

        public LanguageContainer[] description { get; set; }

        public System.DateTime startDate { get; set; }

        public System.DateTime endDate { get; set; }

        public int maxUseCountForCoupon { get; set; }

        public string groupCode { get; set; }

        public string groupName { get; set; }

        public int financialEntityID { get; set; }

        public int maxRecurringUsesCountForCoupon { get; set; }
    }
}
