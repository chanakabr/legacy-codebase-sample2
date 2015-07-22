using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription price details
    /// </summary>
    public class KalturaSubscriptionPrice
    {
        /// <summary>
        /// Subscription
        /// </summary>
        [DataMember(Name = "subscription")]
        [JsonProperty("subscription")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Subscription purchase status  
        /// </summary>
        [DataMember(Name = "purchase_status")]
        [JsonProperty("purchase_status")]
        public KalturaPurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// Subscription price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public KalturaPrice Price { get; set; }
    }
}