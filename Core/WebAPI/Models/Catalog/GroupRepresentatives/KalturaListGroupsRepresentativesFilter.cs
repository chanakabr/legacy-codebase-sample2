using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.GroupRepresentatives
{
    public partial class KalturaListGroupsRepresentativesFilter : KalturaFilter<KalturaListGroupsRepresentativesOrderBy>
    {
        /// <summary>
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }

        public override KalturaListGroupsRepresentativesOrderBy GetDefaultOrderByValue()
            => KalturaListGroupsRepresentativesOrderBy.None;
    }
}