using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Collections list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public partial class KalturaCollectionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of collections
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCollection> Collections { get; set; }
    }
}