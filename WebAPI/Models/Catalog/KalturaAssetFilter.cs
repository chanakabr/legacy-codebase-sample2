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
    public class KalturaAssetFilter : KalturaFilter
    {
        /// <summary>
        /// Current request identifier (used for paging)
        /// </summary>
        [DataMember(Name = "requestIdEqual")]
        [JsonProperty("requestIdEqual")]
        [XmlElement(ElementName = "requestIdEqual", IsNullable = true)]
        public string RequestIdEqual { get; set; }

        /// <summary>
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// geo_block - only valid value is "true": When enabled, only assets that are not restriced to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// Comparison operators: for numerical fields =, >, >=, <, <=. For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 1024 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }

        /// <summary>
        /// List of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typesIn")]
        [JsonProperty("typesIn")]
        [XmlElement(ElementName = "typesIn", IsNullable = true)]
        public List<KalturaIntegerValue> TypesIn { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderBy? OrderBy { get; set; }

        public override object GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.RELEVANCY;
        }
    }
}



