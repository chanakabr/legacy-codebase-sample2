using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
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
    [Serializable]
    public partial class KalturaAssetRule : KalturaAssetRuleBase
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

        internal void Validate()
        {
            if (this.Conditions == null || this.Conditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }

            bool countryConditionExist = false;
            bool concurrencyConditionExist = false;
            bool assetConditionExist = false;
            bool ipRangeConditionExist = false;
            bool orConditionExist = false;
            bool headerConditionExist = false;

            foreach (var condition in Conditions)
            {
                switch (condition.Type)
                {
                    case KalturaRuleConditionType.COUNTRY:
                        {
                            if (concurrencyConditionExist)
                            {
                                throw new BadRequestException
                                (BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                 "conditions=" + KalturaRuleConditionType.COUNTRY.ToString(), "conditions= " + KalturaRuleConditionType.CONCURRENCY.ToString());
                            }

                            countryConditionExist = true;
                        }
                        break;
                    case KalturaRuleConditionType.CONCURRENCY:
                        {
                            if (concurrencyConditionExist)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "conditions");
                            }

                            if (countryConditionExist || assetConditionExist)
                            {
                                throw new BadRequestException
                                (BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                 "conditions=" + KalturaRuleConditionType.CONCURRENCY.ToString(),
                                 "conditions= " + KalturaRuleConditionType.COUNTRY.ToString() + "/" + KalturaRuleConditionType.ASSET.ToString());
                            }

                            concurrencyConditionExist = true;
                        }
                        break;
                    case KalturaRuleConditionType.ASSET:
                        {
                            if (concurrencyConditionExist)
                            {
                                throw new BadRequestException
                                (BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                 "conditions=" + KalturaRuleConditionType.ASSET.ToString(), "conditions= " + KalturaRuleConditionType.CONCURRENCY.ToString());
                            }

                            assetConditionExist = true;
                        }
                        break;
                    case KalturaRuleConditionType.IP_RANGE:
                        {
                            ipRangeConditionExist = true;
                        }
                        break;
                    case KalturaRuleConditionType.OR:
                        {
                            orConditionExist = true;
                        }
                        break;
                    case KalturaRuleConditionType.HEADER:
                        {
                            headerConditionExist = true;
                        }
                        break;
                    default:
                        break;
                }
                
                condition.Validate();
            }

            if (!orConditionExist && !countryConditionExist && !concurrencyConditionExist && !ipRangeConditionExist && !headerConditionExist )
            {
                if (assetConditionExist && this.Actions != null && Actions.Count > 0)
                {
                    KalturaApplyPlaybackAdapterAction applyPlaybackAdapterAction = Actions[0] as KalturaApplyPlaybackAdapterAction;
                    if (applyPlaybackAdapterAction == null)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
                    }
                }
                else
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }

            ValidateActions();

            if (ipRangeConditionExist)
            {
                ValidateIpRangeActions();
            }
            else
            {
                ValidateActions(concurrencyConditionExist);

            }
        }

        private void ValidateIpRangeActions()
        {
            var ruleAction = Actions.Count(x => x.Type == KalturaRuleActionType.ALLOW_PLAYBACK || x.Type == KalturaRuleActionType.BLOCK_PLAYBACK);

            if (ruleAction == 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "actions");
            }
        }
        
        private void ValidateActions()
        {
            if (this.Actions == null || this.Actions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }

            var duplicates = this.Actions.GroupBy(x => x.Type).Count(t => t.Count() >= 2);

            if (duplicates > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
            }
        }

        private void ValidateActions(bool concurrencyConditionExist)
        {
            var ruleActionBlock = Actions.Count(x => x.Type == KalturaRuleActionType.BLOCK);

            if (concurrencyConditionExist && ruleActionBlock == 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "actions", KalturaRuleActionType.BLOCK.ToString());
            }

            if (ruleActionBlock > 0 && Actions.Count > 1)
            {
                throw new BadRequestException
                    (BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    "actions=" + KalturaRuleActionType.BLOCK.ToString(),
                    "actions= " + KalturaRuleActionType.END_DATE_OFFSET.ToString() + "/" + KalturaRuleActionType.START_DATE_OFFSET.ToString());
            }
        }
        
        /// <summary>
        /// Fill current AssetRule data members with givven assetRule only if they are empty\null
        /// </summary>
        /// <param name="oldAssetRule">givven assetRule to fill with</param>
        internal void FillEmpty(KalturaAssetRule oldAssetRule)
        {
            if (oldAssetRule != null)
            {
                if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
                {
                    this.Name = oldAssetRule.Name;
                }

                if (string.IsNullOrEmpty(this.Description) || string.IsNullOrWhiteSpace(this.Description))
                {
                    this.Description = oldAssetRule.Description;
                }

                if (this.Actions == null || this.Actions.Count == 0)
                {
                    this.Actions = oldAssetRule.Actions;
                }

                if (this.Conditions == null || this.Conditions.Count == 0)
                {
                    this.Conditions = oldAssetRule.Conditions;
                }
            }
        }
    }

    public partial class KalturaAssetRuleListResponse : KalturaListResponse
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