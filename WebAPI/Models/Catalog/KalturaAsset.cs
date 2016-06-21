using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaAssetListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAsset> Objects { get; set; }

        [DataMember(Name = "requestId")]
        [JsonProperty(PropertyName = "requestId")]
        [XmlElement("requestId", IsNullable = true)]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    public class KalturaAsset : KalturaBaseAssetInfo, KalturaIAssetable
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
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – till when the asset be available in the catalog. For EPG/Linear – program end time and date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// A collection of additional key value pairs that are available per asset type. Possible keys: 
        /// For 0 (EPG linear programs): epg_channel_id - The EPG channel ID, epg_id - The EPG identifier, related_media_id - The linear media ID.
        /// For other : start_date, final_date, external_ids
        /// </summary>
        [DataMember(Name = "extraParams", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "extraParams", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement("extraParams", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> ExtraParams { get; set; }
    }
}