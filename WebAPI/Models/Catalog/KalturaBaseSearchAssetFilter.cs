using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    // <summary>
    /// Kaltura Base Search Asset Filter
    /// </summary>
    [Serializable]
    abstract public class KalturaBaseSearchAssetFilter : KalturaAssetFilter
    {
        /// <summary>
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restricted to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// user_interests - only valid value is "true". When enabled, only assets that the user defined as his interests (by tags and metas) will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "not_entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// asset_type - valid values: "media", "epg", "recording" or any number that represents media type in group.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). 
        /// For alpha-numerical fields =, != (not), ~ (like), !~, ^ (any word starts with), ^= (phrase starts with), + (exists), !+ (not exists).
        /// Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each for the next operators: ~, !~, ^, ^=
        /// (maximum length of entire filter is 2048 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        /// groupBy
        /// </summary>
        [DataMember(Name = "groupBy")]
        [JsonProperty("groupBy")]
        [XmlArray(ElementName = "groupBy", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public List<KalturaAssetGroupBy> GroupBy
        {
            get;
            set;
        }


        internal List<string> getGroupByValue()
        {
            if (GroupBy == null || GroupBy.Count == 0)
                return null;

            List<string> values = GroupBy.Select(x => x.GetValue()).ToList();

            return values;
        }
    }
}