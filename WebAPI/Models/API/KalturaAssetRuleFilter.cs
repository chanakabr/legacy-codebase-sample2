using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
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
        /// Indicates if to get the asset user rule list for the attached user or for the entire group
        /// </summary>
        [DataMember(Name = "conditionsContainType")]
        [JsonProperty("conditionsContainType")]
        [XmlElement(ElementName = "conditionsContainType", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaRuleConditionType ConditionsContainType { get; set; }

        public KalturaAssetRuleFilter()
        {
            this.ConditionsContainType = KalturaRuleConditionType.COUNTRY;
        }

        internal void Validate()
        {
            if (!KalturaRuleConditionType.CONCURRENCY.Equals(ConditionsContainType) &&
                !KalturaRuleConditionType.COUNTRY.Equals(ConditionsContainType))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetRuleFilter.conditionTypeEqual");
            }
        }

        public override KalturaAssetRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetRuleOrderBy.NONE;
        }
    }
}