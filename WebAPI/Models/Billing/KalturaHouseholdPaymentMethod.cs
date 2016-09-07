using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    public class KalturaHouseholdPaymentMethod : KalturaOTTObject
    {
        /// <summary>
        /// Household payment method identifier (internal)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        // TODO: make sure write only works (jil formater)
        /// <summary>
        /// External identifier for the household payment method
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(WriteOnly = true)]
        public string ExternalId { get; set; }

        /// <summary>
        /// Payment-gateway identifier
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Description of the payment method details
        /// </summary>
        [DataMember(Name = "details")]
        [JsonProperty("details")]
        [XmlElement(ElementName = "details")]
        public string Details { get; set; }


        /// <summary>
        /// indicates whether the payment method is set as default for the household
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Payment method profile identifier
        /// </summary>
        [DataMember(Name = "paymentMethodProfileId")]
        [JsonProperty("paymentMethodProfileId")]
        [XmlElement(ElementName = "paymentMethodProfileId")]
        public int PaymentMethodProfileId { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [Obsolete]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the payment method allow multiple instances 
        /// </summary>
        [DataMember(Name = "allowMultiInstance")]
        [JsonProperty("allowMultiInstance")]
        [XmlElement(ElementName = "allowMultiInstance")]
        [Obsolete]
        public bool? AllowMultiInstance { get; set; }


        /// <summary>
        /// Selected payment method 
        /// </summary>
        [DataMember(Name = "selected")]
        [JsonProperty("selected")]
        [XmlElement(ElementName = "selected")]
        [Obsolete]
        public bool? Selected { get; set; }
    }

    /// <summary>
    /// List of household payment methods.
    /// </summary>
    [DataContract(Name = "KalturaHouseholdPaymentMethodListResponse", Namespace = "")]
    [XmlRoot("KalturaHouseholdPaymentMethodListResponse")]
    public class KalturaHouseholdPaymentMethodListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaHouseholdPaymentMethod> Objects { get; set; }
    }
}