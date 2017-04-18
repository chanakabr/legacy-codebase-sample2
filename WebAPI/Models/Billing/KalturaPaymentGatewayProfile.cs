using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment gateway profile 
    /// </summary>
    public class KalturaPaymentGatewayProfile : KalturaPaymentGatewayBaseProfile
    {
        /// <summary>
        /// Payment gateway is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [OldStandardProperty("is_active")]
        public int? IsActive { get; set; }

        /// <summary>
        /// Payment gateway adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [OldStandardProperty("adapter_url")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Payment gateway transact URL
        /// </summary>
        [DataMember(Name = "transactUrl")]
        [JsonProperty("transactUrl")]
        [XmlElement(ElementName = "transactUrl")]
        [OldStandardProperty("transact_url")]
        public string TransactUrl { get; set; }

        /// <summary>
        /// Payment gateway status URL
        /// </summary>
        [DataMember(Name = "statusUrl")]
        [JsonProperty("statusUrl")]
        [XmlElement(ElementName = "statusUrl")]
        [OldStandardProperty("status_url")]
        public string StatusUrl { get; set; }

        /// <summary>
        /// Payment gateway renew URL
        /// </summary>
        [DataMember(Name = "renewUrl")]
        [JsonProperty("renewUrl")]
        [XmlElement(ElementName = "renewUrl")]
        [OldStandardProperty("renew_url")]
        public string RenewUrl { get; set; }

        /// <summary>
        /// Payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "paymentGatewaySettings")]
        [JsonProperty("paymentGatewaySettings")]
        [XmlElement(ElementName = "paymentGatewaySettings", IsNullable = true)]
        [OldStandardProperty("payment_gateway_settings")]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// Payment gateway external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        [OldStandardProperty("external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Pending Interval in minutes
        /// </summary>
        [DataMember(Name = "pendingInterval")]
        [JsonProperty("pendingInterval")]
        [XmlElement(ElementName = "pendingInterval")]
        [OldStandardProperty("pending_interval")]
        public int? PendingInterval { get; set; }

        /// <summary>
        /// Pending Retries
        /// </summary>
        [DataMember(Name = "pendingRetries")]
        [JsonProperty("pendingRetries")]
        [XmlElement(ElementName = "pendingRetries")]
        [OldStandardProperty("pending_retries")]
        public int? PendingRetries { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [OldStandardProperty("shared_secret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// Renew Interval Minutes
        /// </summary>
        [DataMember(Name = "renewIntervalMinutes")]
        [JsonProperty("renewIntervalMinutes")]
        [XmlElement(ElementName = "renewIntervalMinutes")]
        [OldStandardProperty("renew_interval_minutes")]
        public int? RenewIntervalMinutes { get; set; }

        /// <summary>
        /// Renew Start Minutes
        /// </summary>
        [DataMember(Name = "renewStartMinutes")]
        [JsonProperty("renewStartMinutes")]
        [XmlElement(ElementName = "renewStartMinutes")]
        [OldStandardProperty("renew_start_minutes")]
        public int? RenewStartMinutes { get; set; }

        internal int getId()
        {
            return Id.HasValue ? (int)Id : 0;
        }
    }

    /// <summary>
    /// PaymentGatewayProfile list
    /// </summary>
    [DataContract(Name = "PaymentGatewayProfiles", Namespace = "")]
    [XmlRoot("PaymentGatewayProfiles")]
    public class KalturaPaymentGatewayProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of payment-gateway profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPaymentGatewayProfile> PaymentGatewayProfiles { get; set; }
    }
}
