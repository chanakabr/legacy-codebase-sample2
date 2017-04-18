using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// A user list of assets
    /// </summary>
    [Serializable]
    [Obsolete]
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
        [DataMember(Name = "listType")]
        [JsonProperty("listType")]
        [XmlElement(ElementName = "listType")]
        [OldStandardProperty("list_type")]
        public KalturaUserAssetsListType ListType { get; set; }

    }
}