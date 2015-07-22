using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price 
    /// </summary>
    [Serializable]
    public class KalturaPrice
    {
        /// <summary>
        ///Price
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        public double Amount { get; set; }

        /// <summary>
        ///Currency
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}