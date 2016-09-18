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
    /// Payment gateway base profile
    /// </summary>
    [OldStandard("isDefault", "is_default")]
    [OldStandard("selectedBy", "selected_by")]
    [OldStandard("paymentMethods", "payment_methods")]
    public class KalturaPaymentGatewayBaseProfile : KalturaOTTObject
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
        [XmlElement(ElementName = "selectedBy", IsNullable = true)]
        public KalturaHouseholdPaymentGatewaySelectedBy? selectedBy { get; set; }

        /// <summary>
        /// payment method
        /// </summary>
        [DataMember(Name = "paymentMethods")]
        [JsonProperty("paymentMethods")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [Obsolete]
        public List<KalturaPaymentMethod> PaymentMethods { get; set; }
    }

    public class KalturaHouseholdPaymentGateway : KalturaOTTObject
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
        [XmlElement(ElementName = "selectedBy", IsNullable = true)]
        public KalturaHouseholdPaymentGatewaySelectedBy selectedBy { get; set; }
    }

    /// <summary>
    /// List of household payment gateways.
    /// </summary>
    [DataContract(Name = "KalturaHouseholdPaymentGatewayListResponse", Namespace = "")]
    [XmlRoot("KalturaHouseholdPaymentGatewayListResponse")]
    public class KalturaHouseholdPaymentGatewayListResponse : KalturaListResponse
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
}
