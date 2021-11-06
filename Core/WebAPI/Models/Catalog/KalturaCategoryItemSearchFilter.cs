using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaCategoryItemSearchFilter : KalturaCategoryItemFilter
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        /// Root only
        /// </summary>
        [DataMember(Name = "rootOnly")]
        [JsonProperty("rootOnly")]
        [XmlElement(ElementName = "rootOnly")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool RootOnly { get; set; }

        /// <summary>
        /// Indicates which category to return by their type.
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement(ElementName = "typeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string TypeEqual { get; set; }

        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }
    }
}