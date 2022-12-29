using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// List of household payment methods.
    /// </summary>
    [DataContract(Name = "KalturaHouseholdPaymentMethodListResponse", Namespace = "")]
    [XmlRoot("KalturaHouseholdPaymentMethodListResponse")]
    public partial class KalturaHouseholdPaymentMethodListResponse : KalturaListResponse
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