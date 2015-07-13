using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Pricing
{
    [DataContract(Name = "SubscriptionsPrices", Namespace = "")]
    [XmlRoot("SubscriptionsPrices")]
    public class SubscriptionsPricesList
    {
        [DataMember(Name = "subscriptions_prices")]
        [JsonProperty("subscriptions_prices")]
        public List<SubscriptionPrice> SubscriptionsPrices { get; set; }
    }
}