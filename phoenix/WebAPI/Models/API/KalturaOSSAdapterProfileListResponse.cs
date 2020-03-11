using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// OSS adapter-profiles list
    /// </summary>
    [DataContract(Name = "GenericRules", Namespace = "")]
    [XmlRoot("GenericRules")]
    public partial class KalturaOSSAdapterProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of OSS adapter-profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaOSSAdapterProfile> OSSAdapterProfiles { get; set; }
    }
}
