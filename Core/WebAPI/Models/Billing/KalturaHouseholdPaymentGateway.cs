using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    public partial class KalturaHouseholdPaymentGateway : KalturaOTTObject
    {
        /// <summary>
        /// payment gateway id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Payment gateway default (true/false)
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// distinction payment gateway selected by account or household
        /// </summary>
        [DataMember(Name = "selectedBy")]
        [JsonProperty("selectedBy")]
        [XmlElement(ElementName = "selectedBy")]
        public KalturaHouseholdPaymentGatewaySelectedBy selectedBy { get; set; }

        /// <summary>
        /// suspend settings
        /// </summary>
        [DataMember(Name = "suspendSettings")]
        [JsonProperty("suspendSettings")]
        [XmlElement(ElementName = "suspendSettings", IsNullable = true)]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public KalturaSuspendSettings SuspendSettings { get; set; }
    }

    /// <summary>
    /// List of household payment gateways.
    /// </summary>
    [DataContract(Name = "KalturaHouseholdPaymentGatewayListResponse", Namespace = "")]
    [XmlRoot("KalturaHouseholdPaymentGatewayListResponse")]
    public partial class KalturaHouseholdPaymentGatewayListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaHouseholdPaymentGateway> Objects { get; set; }
    }

    /// <summary>
    /// Suspend Settings
    /// </summary>
    public partial class KalturaSuspendSettings : KalturaOTTObject
    {
        /// <summary>
        /// revoke entitlements
        /// </summary>
        [DataMember(Name = "revokeEntitlements")]
        [JsonProperty("revokeEntitlements")]
        [XmlElement(ElementName = "revokeEntitlements")]
        public bool RevokeEntitlements { get; set; }

        /// <summary>
        /// stop renew
        /// </summary>
        [DataMember(Name = "stopRenew")]
        [JsonProperty("stopRenew")]
        [XmlElement(ElementName = "stopRenew")]
        public bool StopRenew { get; set; }
    }
}
