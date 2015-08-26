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
    public class KalturaProductsPriceListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of subscriptions prices
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")] 
        public List<KalturaProductPrice> ProductsPrices { get; set; }
    }
}