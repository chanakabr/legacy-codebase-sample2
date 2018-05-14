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
    /// Asset user rule
    /// </summary>
    public class KalturaAssetUserRule : KalturaAssetRuleBase
    {
        /// <summary>
        /// List of Ksql conditions for the user rule
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaAssetCondition> Conditions { get; set; }

        /// <summary>
        /// List of actions for the user rule
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        public List<KalturaAssetUserRuleAction> Actions { get; set; }

        public void ValidateActions()
        {
            if (Actions != null)
            {
                var duplicates = Actions.GroupBy(x => x.Type).Where(t => t.Count() >= 2);

                if (duplicates != null && duplicates.ToList().Count > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
                }

                // relevent if there is more then one action type (right now we have only one)
                //var ruleActionBlock = Actions.Where(x => x.Type == KalturaRuleActionType.USER_BLOCK).ToList();
                //if (ruleActionBlock != null && ruleActionBlock.Count > 0 && Actions.Count > 1)
                //{
                //    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "actions=" + KalturaRuleActionType.USER_BLOCK.ToString(),
                //        "actions= " + KalturaRuleActionType.END_DATE_OFFSET.ToString() + "/" + KalturaRuleActionType.START_DATE_OFFSET.ToString());
                //}
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }
        }

        public void ValidateConditions()
        {
            if (Conditions != null && Conditions.Count > 0)
            {
                foreach (var condition in Conditions)
                {
                    if (string.IsNullOrEmpty(condition.Ksql))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
                    }
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }
        }
    }

    public class KalturaAssetUserRuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// Asset user rules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetUserRule> Objects { get; set; }
    }
}