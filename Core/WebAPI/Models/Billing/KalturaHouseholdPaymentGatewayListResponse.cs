using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
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
}
