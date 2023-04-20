using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaAssetRuleOrderBy
    {
        NONE,
        NAME_ASC,
        NAME_DESC
    }

    /// <summary>
    /// Asset rule filter
    /// </summary>
    public partial class KalturaAssetRuleFilter : KalturaFilter<KalturaAssetRuleOrderBy>
    {
        /// <summary>
        /// Indicates which asset rule list to return by it KalturaRuleConditionType.
        /// Default value: KalturaRuleConditionType.COUNTRY
        /// </summary>
        [DataMember(Name = "conditionsContainType")]
        [JsonProperty("conditionsContainType")]
        [XmlElement(ElementName = "conditionsContainType")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaRuleConditionType ConditionsContainType { get; set; }

        /// <summary>
        /// Indicates if to return an asset rule list that related to specific asset
        /// </summary>
        [DataMember(Name = "assetApplied")]
        [JsonProperty("assetApplied")]
        [XmlElement(ElementName = "assetApplied", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaSlimAsset AssetApplied { get; set; }

        /// <summary>
        /// Indicates which asset rule list to return by this KalturaRuleActionType.
        /// </summary>
        [DataMember(Name = "actionsContainType")]
        [JsonProperty("actionsContainType")]
        [XmlElement(ElementName = "actionsContainType")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaRuleActionType? ActionsContainType { get; set; }

        /// <summary>
        /// Asset rule id
        /// </summary>
        [DataMember(Name = "assetRuleIdEqual")]
        [JsonProperty("assetRuleIdEqual")]
        [XmlElement(ElementName = "assetRuleIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? AssetRuleIdEqual { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "nameContains")]
        [JsonProperty("nameContains")]
        [XmlElement(ElementName = "nameContains", IsNullable = true)]
        [SchemeProperty(MinLength = 1, IsNullable = true, MaxLength = 50)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string NameContains { get; set; }

        protected override void Init()
        {
            base.Init();
            this.ConditionsContainType = KalturaRuleConditionType.COUNTRY;
        }

        internal void Validate()
        {
            if (KalturaRuleConditionType.BUSINESS_MODULE == ConditionsContainType ||
                KalturaRuleConditionType.SEGMENTS == ConditionsContainType ||
                KalturaRuleConditionType.DATE == ConditionsContainType ||
                KalturaRuleConditionType.OR == ConditionsContainType ||
                KalturaRuleConditionType.HEADER == ConditionsContainType)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.conditionsContainType");
            }

            if (KalturaRuleConditionType.ASSET == ConditionsContainType &&
                !ActionsContainType.HasValue || 
                (ActionsContainType.HasValue &&
                 (ActionsContainType.Value == KalturaRuleActionType.START_DATE_OFFSET ||
                  ActionsContainType.Value == KalturaRuleActionType.END_DATE_OFFSET ||
                  ActionsContainType.Value == KalturaRuleActionType.USER_BLOCK ||
                  ActionsContainType.Value == KalturaRuleActionType.ALLOW_PLAYBACK ||
                  ActionsContainType.Value == KalturaRuleActionType.BLOCK_PLAYBACK ||
                  ActionsContainType.Value == KalturaRuleActionType.APPLY_DISCOUNT_MODULE ||
                  ActionsContainType.Value == KalturaRuleActionType.FILTER)))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.actionsContainType");
            }

            long assetId;
            if (AssetApplied != null)
            {            
                if (!long.TryParse(AssetApplied.Id, out assetId))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.assetApplied.id");
                }

                if (!KalturaAssetType.epg.Equals(AssetApplied.Type) && !KalturaAssetType.media.Equals(AssetApplied.Type))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.assetApplied.type");
                }
            }

            if (AssetRuleIdEqual.HasValue)
            {
                if (AssetApplied != null && !long.TryParse(AssetApplied.Id, out assetId))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetRuleFilter.assetRuleIdEqual", "KalturaAssetRuleFilter.assetApplied.id");
                }

                if (ActionsContainType.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetRuleFilter.assetRuleIdEqual", "KalturaAssetRuleFilter.actionsContainType");
                }
            }
        }

        public override KalturaAssetRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetRuleOrderBy.NONE;
        }
    }
}