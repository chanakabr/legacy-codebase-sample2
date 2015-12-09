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
    public class KalturaAssetInfoListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetInfo> Objects { get; set; }

        [DataMember(Name = "request_id")]
        [JsonProperty(PropertyName = "request_id")]
        [XmlElement("request_id", IsNullable = true)]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    public class KalturaAssetInfo : KalturaBaseAssetInfo, KalturaIAssetable
    {
        /// <summary>
        /// Dynamic collection of key-value pairs according to the String Meta defined in the system
        /// </summary>
        [DataMember(Name = "metas")]
        [JsonProperty(PropertyName = "metas")]
        [XmlElement("metas", IsNullable = true)]
        public SerializableDictionary<string, KalturaValue> Metas { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the Tag Types defined in the system
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        [XmlElement("tags", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValueArray> Tags { get; set; }

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
        /// A collection of additional key value pairs that are available per asset type. Possible keys: 
        /// For 0 (EPG linear programs): epg_channel_id - The EPG channel ID, epg_id - The EPG identifier, related_media_id - The linear media ID.
        /// For other : start_date, final_date, external_ids
        /// </summary>
        [DataMember(Name = "extra_params", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "extra_params", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement("extra_params", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> ExtraParams { get; set; }
    }
}