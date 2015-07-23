using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// SubscriptionsPrices list
    /// </summary>
    [DataContract(Name = "SubscriptionsPrices", Namespace = "")]
    [XmlRoot("SubscriptionsPrices")]
    public class KalturaSubscriptionsPricesList : KalturaOTTObject
    {
        /// <summary>
        /// A list of subscriptions prices
        /// </summary>
        [DataMember(Name = "subscriptions_prices")]
        [JsonProperty("subscriptions_prices")]
        public List<KalturaSubscriptionPrice> SubscriptionsPrices { get; set; }
    }
}