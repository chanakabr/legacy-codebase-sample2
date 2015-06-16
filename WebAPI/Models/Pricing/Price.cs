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
    public class Price
    {
        /// <summary>
        ///Price
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public double price { get; set; }

        /// <summary>
        ///Currency
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        public Currency currency { get; set; }
    }
}