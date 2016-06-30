using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// A user list of assets
    /// </summary>
    [Serializable]
    public class KalturaUserAssetsList : KalturaOTTObject
    {
        /// <summary>
        ///Assets list
        /// </summary>
        [DataMember(Name = "list")]
        [JsonProperty("list")]
        [XmlArray(ElementName = "list", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUserAssetsListItem> List { get; set; }

        /// <summary>
        ///The type of the list
        /// </summary>
        [DataMember(Name = "list_type")]
        [JsonProperty("list_type")]
        [XmlElement(ElementName = "list_type")]
        public KalturaUserAssetsListType ListType { get; set; }

    }
}