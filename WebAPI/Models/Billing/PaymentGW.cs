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
        public int ID { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// payment gateway default (true / false)
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }
        /// <summary>
        /// payment gateway is active status
        /// </summary>

        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        public int IsActive { get; set; }

        /// <summary>
        /// payment gateway url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "payment_gatewaye_settings")]
        [JsonProperty("payment_gatewaye_settings")]
        public Dictionary<string,string> Settings { get; set; }

        /// <summary>
        /// payment gateway external identifier
        /// </summary>
        [DataMember(Name = "external_identifier")]
        [JsonProperty("external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Pending Interval in minutes
        /// </summary>
        [DataMember(Name = "pendding_interval")]
        [JsonProperty("pending_interval")]
        public int PendingInterval { get; set; }

        /// <summary>
        /// Pending Retries
        /// </summary>
        [DataMember(Name = "pending_retries")]
        [JsonProperty("pending_retries")]
        public int PendingRetries { get; set; }


        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "shared_secret")]
        [JsonProperty("shared_secret")]
        public string SharedSecret { get; set; }
    }
}
