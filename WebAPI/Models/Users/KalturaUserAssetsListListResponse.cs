using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User assets list 
    /// </summary>
    [DataContract(Name = "UserAssetsLists", Namespace = "")]
    [XmlRoot("UserAssetsLists")]
    public class KalturaUserAssetsListListResponse : KalturaListResponse
    {
        /// <summary> 
        /// A list of favorites
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUserAssetsList> UserAssetsLists { get; set; }
    }
}