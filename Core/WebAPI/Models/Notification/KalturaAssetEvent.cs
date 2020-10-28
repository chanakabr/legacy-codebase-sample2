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
    public partial class KalturaAssetEvent : KalturaEventObject
    {
        /// <summary>
        /// User Id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        public long UserId { get; set; }

        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(ReadOnly = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int Type { get; set; }

        /// <summary>
        /// External identifier for the asset
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ExternalId { get; set; }
    }

    [Serializable]
    public partial class KalturaProgramAssetEvent : KalturaAssetEvent
    {
        /// <summary>
        /// The  live asset Id that was identified according liveAssetExternalId
        /// </summary>
        [DataMember(Name = "liveAssetId")]
        [JsonProperty(PropertyName = "liveAssetId")]
        [XmlElement(ElementName = "liveAssetId")]
        [SchemeProperty(ReadOnly = true)]
        public int LiveAssetId { get; set; }
    }
}