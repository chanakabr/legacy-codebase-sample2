using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public partial class KalturaHouseholdSegmentFilter : KalturaFilter<KalturaHouseholdSegmentOrderBy>
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaHouseholdSegmentOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdSegmentOrderBy.NONE;
        }
    }
}