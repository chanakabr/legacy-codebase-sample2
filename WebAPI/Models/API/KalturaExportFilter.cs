using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Bulk export tasks filter
    /// </summary>
    [Obsolete]
    public class KalturaExportFilter : KalturaFilter<KalturaExportTaskOrderBy>
    {

        /// <summary>
        /// The tasks identifiers
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty("ids")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLongValue> ids { get; set; }

        public override KalturaExportTaskOrderBy GetDefaultOrderByValue()
        {
            return KalturaExportTaskOrderBy.CREATE_DATE_ASC;
        }
    }
}