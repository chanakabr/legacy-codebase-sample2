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
    [OldStandard("isActive", "is_active")]
    [OldStandard("adapterUrl", "adapter_url")]
    [OldStandard("transactUrl", "transact_url")]
    [OldStandard("statusUrl", "status_url")]
    [OldStandard("renewUrl", "renew_url")]
    [OldStandard("paymentGatewayeSettings", "payment_gatewaye_settings")]
    [OldStandard("externalIdentifier", "external_identifier")]
    [OldStandard("pendingInterval", "pending_interval")]
    [OldStandard("pendingRetries", "pending_retries")]
    [OldStandard("sharedSecret", "shared_secret")]
    [OldStandard("renewIntervalMinutes", "renew_interval_minutes")]
    [OldStandard("renewStartMinutes", "renew_start_minutes")]
    public class KalturaPaymentGatewayProfile : KalturaPaymentGatewayBaseProfile
    {
        /// <summary>
        /// Payment gateway is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public int? IsActive { get; set; }

        /// <summary>
        /// Payment gateway adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Payment gateway transact URL
        /// </summary>
        [DataMember(Name = "transactUrl")]
        [JsonProperty("transactUrl")]
        [XmlElement(ElementName = "transactUrl")]
        public string TransactUrl { get; set; }

        /// <summary>
        /// Payment gateway status URL
        /// </summary>
        [DataMember(Name = "statusUrl")]
        [JsonProperty("statusUrl")]
        [XmlElement(ElementName = "statusUrl")]
        public string StatusUrl { get; set; }

        /// <summary>
        /// Payment gateway renew URL
        /// </summary>
        [DataMember(Name = "renewUrl")]
        [JsonProperty("renewUrl")]
        [XmlElement(ElementName = "renewUrl")]
        public string RenewUrl { get; set; }

        /// <summary>
        /// Payment gateway extra parameters
        /// </summary>
        [DataMember(Name = "paymentGatewayeSettings")]
        [JsonProperty("paymentGatewayeSettings")]
        [XmlElement(ElementName = "paymentGatewayeSettings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// Payment gateway external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Pending Interval in minutes
        /// </summary>
        [DataMember(Name = "pendingInterval")]
        [JsonProperty("pendingInterval")]
        [XmlElement(ElementName = "pendingInterval")]
        public int? PendingInterval { get; set; }

        /// <summary>
        /// Pending Retries
        /// </summary>
        [DataMember(Name = "pendingRetries")]
        [JsonProperty("pendingRetries")]
        [XmlElement(ElementName = "pendingRetries")]
        public int? PendingRetries { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// Renew Interval Minutes
        /// </summary>
        [DataMember(Name = "renewIntervalMinutes")]
        [JsonProperty("renewIntervalMinutes")]
        [XmlElement(ElementName = "renewIntervalMinutes")]
        public int? RenewIntervalMinutes { get; set; }

        /// <summary>
        /// Renew Start Minutes
        /// </summary>
        [DataMember(Name = "renewStartMinutes")]
        [JsonProperty("renewStartMinutes")]
        [XmlElement(ElementName = "renewStartMinutes")]
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
