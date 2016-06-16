using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Bulk export tasks filter
    /// </summary>
    [OldStandard("idIn", "ids")]
    public class KalturaExportFilter : KalturaFilter
    {

        /// <summary>
        /// The tasks identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLongValue> IdIn { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public KalturaExportTaskOrderBy? OrderBy { get; set; }

        public override object GetDefaultOrderByValue()
        {
            return KalturaExportTaskOrderBy.CREATE_DATE_ASC;
        }
    }
}