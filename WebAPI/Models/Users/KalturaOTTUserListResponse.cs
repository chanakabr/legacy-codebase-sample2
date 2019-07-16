using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Users list
    /// </summary>
    [DataContract(Name = "Users", Namespace = "")]
    [XmlRoot("Users")]
    public partial class KalturaOTTUserListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of users
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaOTTUser> Users { get; set; }
    }
}