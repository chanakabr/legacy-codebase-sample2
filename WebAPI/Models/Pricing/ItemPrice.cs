using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Pricing
{
    public class ItemPrice
    {
        /// <summary>
        /// PPV module code
        /// </summary>
        [DataMember(Name = "ppv_module_code")]
        [JsonProperty("ppv_module_code")]
        public string PpvModuleCode { get; set; }

        /// <summary>
        /// Is subscription only - ???
        /// </summary>
        [DataMember(Name = "is_subscription_only")]
        [JsonProperty("is_subscription_only")]
        public bool IsSubscriptionOnly { get; set; }

        /// <summary>
        /// Price - ???
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public Price Price { get; set; }

        /// <summary>
        /// Full price - ???   
        /// </summary>
        [DataMember(Name = "full_price")]
        [JsonProperty("full_price")]
        public Price FullPrice { get; set; }
    }
}