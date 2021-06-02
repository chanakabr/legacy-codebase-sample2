using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment gateway base profile
    /// </summary>
    public partial class KalturaPaymentGatewayBaseProfile : KalturaOTTObject
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
        [OldStandardProperty("is_default")]
        [SchemeProperty(IsNullable = true)]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// distinction payment gateway selected by account or household
        /// </summary>
        [DataMember(Name = "selectedBy")]
        [JsonProperty("selectedBy")]
        [XmlElement(ElementName = "selectedBy", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [OldStandardProperty("selected_by")]
        public KalturaHouseholdPaymentGatewaySelectedBy? selectedBy { get; set; }

        /// <summary>
        /// payment method
        /// </summary>
        [DataMember(Name = "paymentMethods")]
        [JsonProperty("paymentMethods")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [OldStandardProperty("payment_methods")]
        [Obsolete]
        public List<KalturaPaymentMethod> PaymentMethods { get; set; }
    }
}
