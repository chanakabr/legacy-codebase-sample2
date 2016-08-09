using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// An item of user asset list
    /// </summary>
    [Serializable]
    [OldStandard("orderIndex", "order_index")]
    [OldStandard("userId", "user_id")]
    [OldStandard("listType", "list_type")]
    public class KalturaUserAssetsListItem : KalturaOTTObject
    {
        /// <summary>
        ///Asset identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        ///The order index of the asset in the list
        /// </summary>
        [DataMember(Name = "orderIndex")]
        [JsonProperty("orderIndex")]
        [XmlElement(ElementName = "orderIndex")]
        public int? OrderIndex { get; set; }

        /// <summary>
        ///The type of the asset
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaUserAssetsListItemType Type { get; set; }

        /// <summary>
        ///The identifier of the user who added the item to the list
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        public string UserId { get; set; }

        /// <summary>
        ///The type of the list
        /// </summary>
        [DataMember(Name = "listType")]
        [JsonProperty("listType")]
        [XmlElement(ElementName = "listType")]
        public KalturaUserAssetsListType ListType { get; set; }

    }
}