using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialFriendActivityListResponse: KalturaListResponse
    {
        /// <summary>
        /// Social friends activity
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSocialFriendActivity> Objects { get; set; }
    }
}