using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset user rule
    /// </summary>
    [Serializable]
    public partial class KalturaAssetUserRule : KalturaAssetRuleBase
    {
        /// <summary>
        /// List of conditions for the user rule
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaAssetConditionBase> Conditions { get; set; }

        /// <summary>
        /// List of actions for the user rule
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        public List<KalturaAssetUserRuleAction> Actions { get; set; }

        public void ValidateActions()
        {
            if (Actions != null && Actions.Count > 0)
            {
                var duplicates = Actions.GroupBy(x => x.Type).Count(t => t.Count() >= 2);
                
                if (duplicates > 1)
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
                    condition.Validate();
                }

                var shopConditions = Conditions.OfType<KalturaAssetShopCondition>().ToList();
                if (shopConditions.Count > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "conditions");
                }

                if (shopConditions.Count == 1 && Actions != null && Actions.Count > 0)
                {
                    if (Actions.Count > 1)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "actions");
                    }

                    if (!(Actions.First() is KalturaAssetUserRuleFilterAction))
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "actions");
                    }
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }
        }
    }

    public partial class KalturaAssetUserRuleListResponse : KalturaListResponse
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