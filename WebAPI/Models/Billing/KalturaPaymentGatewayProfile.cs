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
    public class KalturaPaymentGatewayProfile : KalturaPaymentGatewayBaseProfile
    {


        /// <summary>
        /// Payment gateway is active status
        /// </summary>
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public int IsActive { get; set; }

        /// <summary>
        /// Payment gateway adapter URL
        /// </summary>
        [DataMember(Name = "adapter_url")]
        [JsonProperty("adapter_url")]
        [XmlElement(ElementName = "adapter_url")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Payment gateway transact URL
        /// </summary>
        [DataMember(Name = "transact_url")]
        [JsonProperty("transact_url")]
        [XmlElement(ElementName = "transact_url")]
        public string TransactUrl { get; set; }

        /// <summary>
        /// Payment gateway status URL
        /// </summary>
        [DataMember(Name = "status_url")]
        [JsonProperty("status_url")]
        [XmlElement(ElementName = "status_url")]
        public string StatusUrl { get; set; }

        /// <summary>
        /// Payment gateway renew URL
        /// </summary>
        [DataMember(Name = "renew_url")]
        [JsonProperty("renew_url")]
        [XmlElement(ElementName = "renew_url")]
        public string RenewUrl { get; set; }

        /// <summary>
        /// Payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "payment_gatewaye_settings")]
        [JsonProperty("payment_gatewaye_settings")]
        [XmlElement(ElementName = "payment_gatewaye_settings")]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// Payment gateway external identifier
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
