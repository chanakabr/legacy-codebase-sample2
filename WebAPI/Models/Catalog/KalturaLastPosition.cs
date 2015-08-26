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
    [DataContract(Name = "LastPosition", Namespace = "")]
    [XmlRoot("LastPosition")]
    public class KalturaLastPositionListResponse : KalturaListResponse
    {
        [DataMember(Name = "last_positions")]
        [JsonProperty("last_positions")]
        [XmlArray(ElementName = "last_positions")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLastPosition> LastPositions { get; set; }
    }

    /// <summary>
    /// Representing the last position in a media or nPVR asset until which a user watched   
    /// </summary>
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
        [XmlElement(ElementName = "position_owner")]
        public KalturaPositionOwner PositionOwner { get; set; }
    }
}