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
    /// Asset info wrapper
    /// </summary>
    [Serializable]
    public class KalturaAssetInfoWrapper : KalturaBaseListWrapper 
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets")]
        [XmlArrayItem("item")] 
        public List<KalturaAssetInfo> Assets { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    public class KalturaAssetInfo : KalturaOTTObject, KalturaIAssetable
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Identifies the asset type (EPG, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public int Type { get; set; }

        /// <summary>
        /// Asset name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Asset description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Collection of images details that can be used to represent this asset
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images")]
        [XmlArrayItem("item")] 
        public List<KalturaImage> Images { get; set; }

        /// <summary>
        /// Files
        /// </summary>
        [DataMember(Name = "files", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "files", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "files")]
        [XmlArrayItem("item")] 
        public List<KalturaFile> Files { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the String Meta defined in the system
        /// </summary>
        [DataMember(Name = "metas")]
        [JsonProperty(PropertyName = "metas")]
        [XmlElement("metas")]
        public SerializableDictionary<string, string> Metas { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the Tag Types defined in the system
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        [XmlElement("tags")]
        public SerializableDictionary<string, List<string>> Tags { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – since when the asset is available in the catalog. For EPG/Linear – when the program is aired (can be in the future).
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty(PropertyName = "start_date")]
        [XmlElement(ElementName = "start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – till when the asset be available in the catalog. For EPG/Linear – program end time and date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty(PropertyName = "end_date")]
        [XmlElement(ElementName = "end_date")]
        public long EndDate { get; set; }

        /// <summary>
        /// Collection of add-on statistical information for the media. See AssetStats model for more information
        /// </summary>
        [DataMember(Name = "stats", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "stats")]
        public KalturaAssetStatistics Statistics { get; set; }

        /// <summary>
        /// A collection of additional key value pairs that are available per asset type. Possible keys: 
        /// For 0 (EPG linear programs): epg_channel_id - The EPG channel ID, epg_id - The EPG identifier, related_media_id - The linear media ID.
        /// For other : start_date, final_date, external_ids
        /// </summary>
        [DataMember(Name = "extra_params", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "extra_params", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement("extra_params")]
        public SerializableDictionary<string, string> ExtraParams { get; set; }
    }
}