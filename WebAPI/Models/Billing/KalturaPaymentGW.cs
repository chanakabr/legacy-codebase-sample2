using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// PaymentGW
    /// </summary>
    public class KalturaPaymentGW : KalturaOTTObject
    {
        /// <summary>
        /// payment gateway id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int ID { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// payment gateway default (true / false)
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        [XmlElement(ElementName = "is_default")]
        public bool IsDefault { get; set; }
        /// <summary>
        /// payment gateway is active status
        /// </summary>

        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public int IsActive { get; set; }

        /// <summary>
        /// payment gateway url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "payment_gatewaye_settings")]
        [JsonProperty("payment_gatewaye_settings")]
        [XmlElement(ElementName = "payment_gatewaye_settings")]
        public Dictionary<string,string> Settings { get; set; }

        /// <summary>
        /// payment gateway external identifier
        /// </summary>
        [DataMember(Name = "external_identifier")]
        [JsonProperty("external_identifier")]
        [XmlElement(ElementName = "external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Pending Interval in minutes
        /// </summary>
        [DataMember(Name = "pending_interval")]
        [JsonProperty("pending_interval")]
        [XmlElement(ElementName = "pending_interval")]
        public int PendingInterval { get; set; }

        /// <summary>
        /// Pending Retries
        /// </summary>
        [DataMember(Name = "pending_retries")]
        [JsonProperty("pending_retries")]
        [XmlElement(ElementName = "pending_retries")]
        public int PendingRetries { get; set; }


        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "shared_secret")]
        [JsonProperty("shared_secret")]
        [XmlElement(ElementName = "shared_secret")]
        public string SharedSecret { get; set; }
    }
}
