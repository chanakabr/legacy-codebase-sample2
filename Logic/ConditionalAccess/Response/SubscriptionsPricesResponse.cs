using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess.Response
{
    public class SubscriptionsPricesResponse
    {
        public SubscriptionsPricesContainer[] SubscriptionsPrices { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
