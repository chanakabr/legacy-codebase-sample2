using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users.UserSessionProfile
{
    public partial class KalturaUserSessionProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of KalturaUserSessionProfile
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUserSessionProfile> Objects { get; set; }
    }
}
