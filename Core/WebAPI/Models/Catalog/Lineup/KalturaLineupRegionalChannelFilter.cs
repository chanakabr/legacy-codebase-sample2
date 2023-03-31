using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.Lineup
{
    [SchemeClass(Required = new[] { "regionIdEqual" })]
    public partial class KalturaLineupRegionalChannelFilter : KalturaFilter<KalturaLineupRegionalChannelOrderBy>
    {
        public override KalturaLineupRegionalChannelOrderBy GetDefaultOrderByValue() =>
            KalturaLineupRegionalChannelOrderBy.LCN_ASC;

        /// <summary>
        /// Region ID filter
        /// </summary>
        [DataMember(Name = "regionIdEqual")]
        [JsonProperty("regionIdEqual")]
        [XmlElement("regionIdEqual")]
        [SchemeProperty(MinInteger = 1)]
        public long RegionIdEqual { get; set; }

        /// <summary>
        /// Should include lineup from parent region into response
        /// </summary>
        [DataMember(Name = "parentRegionIncluded")]
        [JsonProperty("parentRegionIncluded")]
        [XmlElement(ElementName = "parentRegionIncluded", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? ParentRegionIncluded { get; set; }

        /// <summary>
        /// A valid KSQL statement - Only linear channels that satisfies the KSQL statement will be included in the results
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }

        /// <summary>
        /// Filter only LCNs that greater or equals to the provided number
        /// </summary>
        [DataMember(Name = "lcnGreaterThanOrEqual")]
        [JsonProperty("lcnGreaterThanOrEqual")]
        [XmlElement("lcnGreaterThanOrEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? LcnGreaterThanOrEqual { get; set; }

        /// <summary>
        /// Filter only LCNs that less or equals to the provided number
        /// </summary>
        [DataMember(Name = "lcnLessThanOrEqual")]
        [JsonProperty("lcnLessThanOrEqual")]
        [XmlElement("lcnLessThanOrEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? LcnLessThanOrEqual { get; set; }
    }
}