using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class SubscriptionPrice
    {
        public double price { get; set; }

        public string currency { get; set; }

        public string subscription_code { get; set; }
    }
}
