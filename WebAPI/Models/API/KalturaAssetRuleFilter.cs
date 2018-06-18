using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaAssetRuleOrderBy
    {
        NONE
    }

    /// <summary>
    /// Asset rule filter
    /// </summary>
    public class KalturaAssetRuleFilter : KalturaFilter<KalturaAssetRuleOrderBy>
    {
        /// <summary>
        /// Indicates which asset rule list to return by it KalturaRuleConditionType 
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

        public KalturaAssetRuleFilter()
        {
            this.ConditionsContainType = KalturaRuleConditionType.COUNTRY;
        }

        internal void Validate()
        {
            if (!KalturaRuleConditionType.CONCURRENCY.Equals(ConditionsContainType) &&
                !KalturaRuleConditionType.COUNTRY.Equals(ConditionsContainType))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.conditionsContainType");
            }

            if (AssetApplied != null)
            {
                long assetId;
                if (!long.TryParse(AssetApplied.Id, out assetId))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.assetApplied.id");
                }

                if (!KalturaAssetType.epg.Equals(AssetApplied.Type) && !KalturaAssetType.media.Equals(AssetApplied.Type))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.assetApplied.type");
                }
            }
        }

        public override KalturaAssetRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetRuleOrderBy.NONE;
        }
    }
}