using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaAssetUserRuleOrderBy
    {
        NONE
    }

    /// <summary>
    /// Asset user rule filter
    /// </summary>
    public partial class KalturaAssetUserRuleFilter : KalturaFilter<KalturaAssetUserRuleOrderBy>
    {
        /// <summary>
        /// Indicates if to get the asset user rule list for the attached user or for the entire group
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "attachedUserIdEqualCurrent")]
        [JsonProperty("attachedUserIdEqualCurrent")]
        [XmlElement(ElementName = "attachedUserIdEqualCurrent", IsNullable = true)]
        public bool? AttachedUserIdEqualCurrent { get; set; }

        /// <summary>
        /// Indicates which asset rule list to return by this KalturaRuleActionType.
        /// </summary>
        [DataMember(Name = "actionsContainType")]
        [JsonProperty("actionsContainType")]
        [XmlElement(ElementName = "actionsContainType")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaRuleActionType? ActionsContainType { get; set; }

        public override KalturaAssetUserRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetUserRuleOrderBy.NONE;
        }

        internal void Validate()
        {            
            if (!ActionsContainType.HasValue || 
                !(ActionsContainType.Value == KalturaRuleActionType.USER_BLOCK || ActionsContainType.Value == KalturaRuleActionType.FILTER))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.actionsContainType");
            }
        }
    }
}