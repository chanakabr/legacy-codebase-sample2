using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    public class SubscriptionPrice
    {
        /// <summary>
        /// Subscription
        /// </summary>
        [DataMember(Name = "subscription")]
        [JsonProperty("subscription")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Subscription price 
        /// </summary>
        [DataMember(Name = "purchase_status")]
        [JsonProperty("purchase_status")]
        public PurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// Price reason
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public Price Price { get; set; }
    }
}