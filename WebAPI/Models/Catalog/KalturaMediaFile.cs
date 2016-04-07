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
    /// Media file details
    /// </summary>
    public class KalturaMediaFile : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public int AssetId { get; set; }

        /// <summary>
        /// File unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Device types as defined in the system
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// URL of the media file to be played
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Duration of the media file
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration")]
        public long Duration { get; set; }

        /// <summary>
        /// External identifier for the media file
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty(PropertyName = "external_id")]
        [XmlElement(ElementName = "external_id")]
        public string ExternalId { get; set; }
    }
}