using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupons group list
    /// </summary>
    [DataContract(Name = "KalturaCouponsGroupListResponse", Namespace = "")]
    [XmlRoot("KalturaCouponsGroupListResponse")]
    public partial class KalturaCouponsGroupListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of coupons groups
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCouponsGroup> couponsGroups { get; set; }
    }
}
