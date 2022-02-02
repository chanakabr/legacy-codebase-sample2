using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog.Ordering
{
    public partial class KalturaChannelDynamicOrder : KalturaBaseChannelOrder
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Name { get; set; }

        /// <summary>
        /// Order By
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaMetaTagOrderBy OrderBy { get; set; }
    }
}