using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset rule
    /// </summary>
    public class KalturaAssetRule : KalturaAssetRuleBase
    {
        /// <summary>
        /// List of conditions for the rule
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaCondition> Conditions { get; set; }

        /// <summary>
        /// List of actions for the rule
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        public List<KalturaAssetRuleAction> Actions { get; set; }

        public void ValidateActions()
        {
            if (this.Actions != null)
            {
                var duplicates = this.Actions.GroupBy(x => x.Type).Where(t => t.Count() >= 2);
                
                if (duplicates != null && duplicates.ToList().Count > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
                }

                var ruleActionBlock = Actions.Where(x => x.Type == KalturaRuleActionType.BLOCK).ToList();
                if (ruleActionBlock != null && ruleActionBlock.Count > 0 && Actions.Count > 1)
                {
                    throw new BadRequestException
                        (BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, 
                        "actions=" + KalturaRuleActionType.BLOCK.ToString(),
                        "actions= " + KalturaRuleActionType.END_DATE_OFFSET.ToString() + "/" + KalturaRuleActionType.START_DATE_OFFSET.ToString());
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }
        }

        public void ValidateConditions()
        {
            bool countryConditionExist = false;

            if (this.Conditions != null)
            {
                foreach (var condition in Conditions)
                {
                    if (condition is KalturaCountryCondition)
                    {
                        countryConditionExist = true;
                        KalturaCountryCondition kAssetCondition = condition as KalturaCountryCondition;
                        if (string.IsNullOrEmpty(kAssetCondition.Countries))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "countries");
                        }
                    }
                    else if (condition is KalturaAssetCondition)
                    {
                        KalturaAssetCondition ksqlCondition = condition as KalturaAssetCondition;
                        if (string.IsNullOrEmpty(ksqlCondition.Ksql) || string.IsNullOrWhiteSpace(ksqlCondition.Ksql))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
                        }
                    }
                }

                if (!countryConditionExist)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "countries");
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }
        }
    }

    public class KalturaAssetRuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// Asset rules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetRule> Objects { get; set; }
    }
}