using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price plan
    /// </summary>
    public class KalturaPricePlan : KalturaUsageModule
    {
        /// <summary>
        /// Denotes whether or not this object can be renewed
        /// </summary>
        [DataMember(Name = "is_renewable")]
        [JsonProperty("is_renewable")]
        public bool IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times the module will be renewed (for the life_cycle period)
        /// </summary>
        [DataMember(Name = "renewals_number")]
        [JsonProperty("renewals_number")]
        public int RenewalsNumber { get; set; }

        /// <summary>
        /// Unique identifier associated with this object's price 
        /// </summary>
        [DataMember(Name = "price_id")]
        [JsonProperty("price_id")]
        public int price_id { get; set; }
    }
}