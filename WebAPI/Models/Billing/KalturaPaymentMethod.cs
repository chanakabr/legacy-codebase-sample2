using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// payment method
    /// </summary>
    [OldStandard("allowMultiInstance", "allow_multi_instance")]
    [OldStandard("householdPaymentMethods", "household_payment_methods")]
    [Obsolete]
    public class KalturaPaymentMethod : KalturaOTTObject
    {
        /// <summary>
        /// Payment method identifier (internal)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int? Id { get; set; }

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
        /// Payment method name
        /// </summary>
        [DataMember(Name = "householdPaymentMethods")]
        [JsonProperty("householdPaymentMethods")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaHouseholdPaymentMethod> HouseholdPaymentMethods { get; set; }
    }
}