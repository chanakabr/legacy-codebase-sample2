using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelFilter : KalturaAssetFilter
    {

        private bool shouldUseChannelDefault = true;

        /// <summary>
        ///Channel Id
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        ///  /// <![CDATA[
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
        public string KSql { get; set; }

        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderBy OrderBy
        {
            get { return base.OrderBy; }
            set
            {
                base.OrderBy = value;
                shouldUseChannelDefault = false;
            }
        }

        public bool GetShouldUseChannelDefault()
        {
            if (DynamicOrderBy != null)
            {
                return false;
            }
            return shouldUseChannelDefault;
        }

    }
}