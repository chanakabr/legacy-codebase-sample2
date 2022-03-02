using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public partial class KalturaDeviceReferenceDataListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of KalturaDeviceReferenceData
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDeviceReferenceData> Objects { get; set; }
    }
}
