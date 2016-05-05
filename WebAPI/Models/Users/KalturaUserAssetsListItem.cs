using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// An item of user asset list
    /// </summary>
    [Serializable]
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
        [DataMember(Name = "order_index")]
        [JsonProperty("order_index")]
        [XmlElement(ElementName = "order_index")]
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
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        ///The type of the list
        /// </summary>
        [DataMember(Name = "list_type")]
        [JsonProperty("list_type")]
        [XmlElement(ElementName = "list_type")]
        public KalturaUserAssetsListType ListType { get; set; }

    }
}