using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public partial class KalturaBookmarkEvent : KalturaEventObject
    {
        /// <summary>
        /// User Id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(MinLong = 0)]
        public long UserId { get; set; }

        /// <summary>
        /// Household Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(MinLong = 0)]
        public long HouseholdId { get; set; }

        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(MinLong = 0)]
        public long AssetId { get; set; }

        /// <summary>
        /// File Id
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty(PropertyName = "fileId")]
        [XmlElement(ElementName = "fileId")]
        [SchemeProperty(MinLong = 0)]
        public long FileId { get; set; }

        /// <summary>
        /// position
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty(PropertyName = "position")]
        [XmlElement(ElementName = "position")]
        [SchemeProperty(MinInteger = 0)]
        public int Position { get; set; }

        /// <summary>
        /// Bookmark Action Type
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public KalturaBookmarkActionType Action { get; set; }

        /// <summary>
        /// Product Type
        /// </summary>
        [DataMember(Name = "productType")]
        [JsonProperty(PropertyName = "productType")]
        [XmlElement(ElementName = "productType")]
        public KalturaTransactionType ProductType { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty(PropertyName = "productId")]
        [XmlElement(ElementName = "productId")]
        [SchemeProperty(MinInteger = 0)]
        public int ProductId { get; set; }
    }
}