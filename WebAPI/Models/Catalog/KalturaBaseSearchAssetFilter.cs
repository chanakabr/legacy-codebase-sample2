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

            List<string> values = GroupBy.Select(x=>x.GetValue()).ToList();                

            return values;
        }
    }
}