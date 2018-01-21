using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Media-asset info
    /// </summary>
    [Serializable]
    public class KalturaMediaAsset : KalturaAsset
    {

        private const string GENESIS_VERSION = "4.6.0.0";

        /// <summary>
        /// External identifiers
        /// </summary>
        [DataMember(Name = "externalIds")]
        [JsonProperty(PropertyName = "externalIds")]
        [XmlElement(ElementName = "externalIds")]
        public string ExternalIds { get; set; }

        /// <summary>
        /// Catch-up buffer
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]
        [Deprecated(GENESIS_VERSION)]
        public long? CatchUpBuffer { get; set; }

        /// <summary>
        /// Trick-play buffer
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]
        [Deprecated(GENESIS_VERSION)]
        public long? TrickPlayBuffer { get; set; }

        /// <summary>
        /// Enable Recording playback for non entitled channel
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannel")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannel")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannel")]
        [SchemeProperty(ReadOnly = true)]
        [Deprecated(GENESIS_VERSION)]
        public bool? EnableRecordingPlaybackNonEntitledChannel { get; set; }

        /// <summary>
        /// Asset type description 
        /// </summary>                
        [DataMember(Name = "typeDescription")]
        [JsonProperty(PropertyName = "typeDescription")]
        [XmlElement(ElementName = "typeDescription")]
        [SchemeProperty(ReadOnly = true)]
        [JsonIgnore]
        public string TypeDescription { get; set; }

        /// <summary>
        /// Entry Identifier
        /// </summary>
        [DataMember(Name = "entryId")]
        [JsonProperty(PropertyName = "entryId")]
        [XmlElement(ElementName = "entryId")]
        public string EntryId { get; set; }

        /// <summary>
        /// Device rule identifier
        /// </summary>
        [DataMember(Name = "deviceRuleId")]
        [JsonProperty(PropertyName = "deviceRuleId")]
        [XmlElement(ElementName = "deviceRuleId", IsNullable = true)]
        public int? DeviceRuleId { get; set; }

        /// <summary>
        /// Device rule
        /// </summary>
        [DataMember(Name = "deviceRule")]
        [JsonProperty(PropertyName = "deviceRule")]
        [XmlElement(ElementName = "deviceRule")]
        [SchemeProperty(ReadOnly = true)]
        [JsonIgnore]
        public string DeviceRule { get; set; }

        /// <summary>
        /// Geo block rule identifier
        /// </summary>
        [DataMember(Name = "geoBlockRuleId")]
        [JsonProperty(PropertyName = "geoBlockRuleId")]
        [XmlElement(ElementName = "geoBlockRuleId", IsNullable = true)]
        public int? GeoBlockRuleId { get; set; }

        /// <summary>
        /// Geo block rule
        /// </summary>
        [DataMember(Name = "geoBlockRule")]
        [JsonProperty(PropertyName = "geoBlockRule")]
        [XmlElement(ElementName = "geoBlockRule")]
        [SchemeProperty(ReadOnly = true)]
        [JsonIgnore]
        public string GeoBlockRule { get; set; }

        /// <summary>
        /// Watch permission rule
        /// </summary>
        [DataMember(Name = "watchPermissionRule")]
        [JsonProperty(PropertyName = "watchPermissionRule")]
        [XmlElement(ElementName = "watchPermissionRule")]
        [SchemeProperty(ReadOnly = true)]
        [JsonIgnore]
        public string WatchPermissionRule { get; set; }

        /// <summary>
        ///  The media asset status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]        
        public bool? Status { get; set; }
    }
}