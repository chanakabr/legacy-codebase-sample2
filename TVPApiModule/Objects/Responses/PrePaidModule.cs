using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PrePaidModule
    {
        public PriceCode priceCode { get; set; }

        public PriceCode creditValue { get; set; }

        public UsageModule usageModule { get; set; }

        public DiscountModule discountModule { get; set; }

        public CouponsGroup couponsGroup { get; set; }

        public LanguageContainer[] description { get; set; }

        public int objectCode { get; set; }

        public string title { get; set; }

        public bool isFixedCredit { get; set; }
    }
}
