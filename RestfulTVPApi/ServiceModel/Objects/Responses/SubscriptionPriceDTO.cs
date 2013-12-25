using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class SubscriptionPriceDTO
    {
        public double Price { get; set; }
        public string Currency { get; set; }
        public string SubscriptionCode { get; set; }
    }
}