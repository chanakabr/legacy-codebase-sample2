using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Prices list
    /// </summary>
    [DataContract(Name = "KalturaPpvListResponse", Namespace = "")]
    [XmlRoot("KalturaPpvListResponse")]
    public partial class KalturaPpvListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of PPV
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPpv> Ppvs { get; set; }
    }
}