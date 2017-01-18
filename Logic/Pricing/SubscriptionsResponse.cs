using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public class SubscriptionsResponse
    {
        public Subscription[] Subscriptions { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
