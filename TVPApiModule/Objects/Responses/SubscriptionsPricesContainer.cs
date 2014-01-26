using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SubscriptionsPricesContainer
    {
        public string subscriptionCode { get; set; }

        public Price price { get; set; }
       
        public PriceReason priceReason { get; set; }
    }
}
