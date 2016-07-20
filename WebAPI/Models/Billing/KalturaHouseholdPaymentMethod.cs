using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
        public int? Id { get; set; }

        /// <summary>
        /// Payment-gateway identifier
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the payment method allow multiple instances 
        /// </summary>
        [DataMember(Name = "allowMultiInstance")]
        [JsonProperty("allowMultiInstance")]
        [XmlElement(ElementName = "allowMultiInstance")]
        public bool? AllowMultiInstance { get; set; }

        /// <summary>
        /// Payment method details
        /// </summary>
        [DataMember(Name = "details")]
        [JsonProperty("details")]
        [XmlElement(ElementName = "details")]
        [Obsolete]
        public string Details { get; set; }

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