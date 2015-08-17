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
    public class KalturaSubscriptionsPriceListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of subscriptions prices
        /// </summary>
        [DataMember(Name = "subscriptions_prices")]
        [JsonProperty("subscriptions_prices")]
        [XmlArray(ElementName = "subscriptions_prices")]
        [XmlArrayItem("item")] 
        public List<KalturaSubscriptionPrice> SubscriptionsPrices { get; set; }
    }
}