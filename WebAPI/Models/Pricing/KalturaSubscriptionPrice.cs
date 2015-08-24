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
    /// Subscription price details
    /// </summary>
    public class KalturaSubscriptionPrice : KalturaProductPrice
    {
        /// <summary>
        /// Subscription purchase status  
        /// </summary>
        [DataMember(Name = "purchase_status")]
        [JsonProperty("purchase_status")]
        [XmlElement(ElementName = "purchase_status")]
        public KalturaPurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// Subscription price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public KalturaPrice Price { get; set; }
    }
}