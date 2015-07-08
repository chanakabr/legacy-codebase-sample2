using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public class PriceCode
    {
        /// <summary>
        /// The price code identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The price code name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// The price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public Price Price { get; set; }

        /// <summary>
        /// A list of the descriptions for this price on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        public List<TranslationContainer> Descriptions { get; set; }
    }
}