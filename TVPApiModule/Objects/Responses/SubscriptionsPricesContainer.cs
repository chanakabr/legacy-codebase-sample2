using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SubscriptionsPricesContainer
    {
        public string m_sSubscriptionCode { get; set; }

        public Price m_oPrice { get; set; }
       
        public PriceReason m_PriceReason { get; set; }
    }
}
