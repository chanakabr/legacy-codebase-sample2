using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "KalturaRegistryResponse", Namespace = "")]
    [XmlRoot("KalturaRegistryResponse")]
    public class KalturaRegistryResponse : KalturaListResponse
    {
        /// <summary>
        /// push web parameters
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaRegistryParameter> RegistryParameters { get; set; }
    }
}