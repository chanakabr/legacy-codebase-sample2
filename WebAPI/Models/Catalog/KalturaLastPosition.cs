using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// List of last positions
    /// </summary>
    [Obsolete]
    [DataContract(Name = "LastPosition", Namespace = "")]
    [XmlRoot("LastPosition")]
    public class KalturaLastPositionListResponse : KalturaListResponse
    {
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLastPosition> LastPositions { get; set; }
    }

    /// <summary>
    /// Representing the last position in a media or nPVR asset until which a user watched   
    /// </summary>
    [Obsolete]
    [Serializable]
    public class KalturaLastPosition : KalturaOTTObject
    {
        /// <summary>
        ///User identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        ///The position in the media duration in seconds
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty("position")]
        [XmlElement(ElementName = "position")]
        public int Position { get; set; }

        /// <summary>
        ///Indicates who is the owner of this position
        /// </summary>
        [DataMember(Name = "position_owner")]
        [JsonProperty("position_owner")]
        [XmlElement(ElementName = "position_owner", IsNullable = true)]
        public KalturaPositionOwner PositionOwner { get; set; }
    }

    [Obsolete]
    public enum KalturaLastPositionAssetType
    {
        media,
        recording
    }

    [Serializable]
    [Obsolete]
    public class KalturaLastPositionFilter : KalturaOTTObject
    {
        /// <summary>
        /// Assets identifier
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> Ids { get; set; }

        /// <summary>
        /// Assets type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaLastPositionAssetType Type { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}