using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User interest list
    /// </summary>
    [DataContract(Name = "UserInterests", Namespace = "")]
    [XmlRoot("UserInterests")]
    public partial class KalturaUserInterestListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of UserInterests
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaUserInterest> UserInterests { get; set; }
    }
}
