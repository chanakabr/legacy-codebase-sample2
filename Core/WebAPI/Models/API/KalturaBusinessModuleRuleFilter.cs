using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaBusinessModuleOrderBy
    {
        NONE
    }

    /// <summary>
    /// Business module rule filter
    /// </summary>
    public partial class KalturaBusinessModuleRuleFilter : KalturaFilter<KalturaBusinessModuleOrderBy>
    {
        /// <summary>
        /// Business module type the rules applied on
        /// </summary>
        [DataMember(Name = "businessModuleTypeApplied")]
        [JsonProperty("businessModuleTypeApplied")]
        [XmlElement(ElementName = "businessModuleTypeApplied", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaTransactionType? BusinessModuleTypeApplied { get; set; }

        /// <summary>
        /// Business module ID the rules applied on
        /// </summary>
        [DataMember(Name = "businessModuleIdApplied")]
        [JsonProperty("businessModuleIdApplied")]
        [XmlElement(ElementName = "businessModuleIdApplied", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? BusinessModuleIdApplied { get; set; }

        /// <summary>
        /// Comma separated segment IDs the rules applied on
        /// </summary>
        [DataMember(Name = "segmentIdsApplied")]
        [JsonProperty("segmentIdsApplied")]
        [XmlElement(ElementName = "segmentIdsApplied", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string SegmentIdsApplied { get; set; }

        /// <summary>
        /// Indicates which business module rule list to return by their action.
        /// </summary>
        [DataMember(Name = "actionsContainType")]
        [JsonProperty("actionsContainType")]
        [XmlElement(ElementName = "actionsContainType")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaRuleActionType? ActionsContainType { get; set; }

        public override KalturaBusinessModuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaBusinessModuleOrderBy.NONE;
        }

        public void Validate()
        {
            if (BusinessModuleIdApplied.HasValue && !BusinessModuleTypeApplied.HasValue)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "businessModuleTypeApplied", "businessModuleIdApplied");
            }
        }
    }
}