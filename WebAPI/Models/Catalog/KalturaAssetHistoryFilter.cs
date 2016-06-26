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
    [OldStandard("filterTypes", "filter_types")]
    [OldStandard("filterStatus", "filter_status")]
    public class KalturaAssetHistoryFilter : KalturaOTTObject
    {
        /// <summary>
        /// List of asset types to search within. The list is a string separated be comma.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "filterTypes")]
        [JsonProperty(PropertyName = "filterTypes")]
        [XmlArray(ElementName = "filterTypes", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> filterTypes { get; set; }

        /// <summary>
        /// Which type of recently watched media to include in the result – those that finished watching, those that are in progress or both.
        /// If omitted or specified filter = all – return all types.
        /// Allowed values: progress – return medias that are in-progress, done – return medias that finished watching.
        /// </summary>
        [DataMember(Name = "filterStatus")]
        [JsonProperty(PropertyName = "filterStatus")]
        [XmlElement(ElementName = "filterStatus", IsNullable = true)]
        public KalturaWatchStatus? filterStatus { get; set; }

        /// <summary>
        /// How many days back to return the watched media. If omitted, default to 7 days
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        [XmlElement(ElementName = "days")]
        public int? days { get; set; }

        /// <summary>
        /// Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        [XmlArray(ElementName = "with", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaCatalogWithHolder> with { get; set; }

        internal int getDays()
        {
            return days.HasValue ? (int)days : 0;
        }
    }
}