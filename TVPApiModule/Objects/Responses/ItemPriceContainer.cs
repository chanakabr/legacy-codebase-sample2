using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class ItemPriceContainer
    {
        public string ppvModuleCode { get; set; }

        public bool subscriptionOnly { get; set; }

        public Price price { get; set; }

        public Price fullPrice { get; set; }

        public PriceReason priceReason { get; set; }

        public Subscription relevantSub { get; set; }

        public PrePaidModule relevantPP { get; set; }

        public LanguageContainer[] ppvVDescription { get; set; }

        public CouponsStatus couponStatus { get; set; }
    }
}
