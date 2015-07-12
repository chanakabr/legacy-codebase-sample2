using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// PaymentGW
    /// </summary>
    public class PaymentGW
    {
        /// <summary>
        /// payment gateway id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int id { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// payment gateway default (true / false)
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        public bool isDefault { get; set; }
        /// <summary>
        /// payment gateway is active status
        /// </summary>

        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        public int isActive { get; set; }

        /// <summary>
        /// payment gateway url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        public string url { get; set; }

        /// <summary>
        /// payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "payment_gatewaye_settings")]
        [JsonProperty("payment_gatewaye_settings")]
        public Dictionary<string,string> settings { get; set; }
    }
}
