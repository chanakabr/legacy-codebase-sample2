using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PPVModule
    {
        /// <remarks/>
        public PriceCode priceCode { get; set; }

        public UsageModule usageModule { get; set; }

        public DiscountModule discountModule { get; set; }

        public CouponsGroup couponsGroup { get; set; }

        public LanguageContainer[] description { get; set; }

        public string objectCode { get; set; }

        public string objectVirtualName { get; set; }

        public bool subscriptionOnly { get; set; }

        public int[] relatedFileTypes { get; set; }

        public string productCode{ get; set; }
    }
}
