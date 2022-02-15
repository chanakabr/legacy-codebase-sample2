using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    using ConditionsMap = ILookup<KalturaRuleConditionType, KalturaCondition>;
    /// <summary>
    /// Asset rule
    /// </summary>
    [Serializable]
    public partial class KalturaAssetRule : KalturaAssetRuleBase
    {
        private static readonly Dictionary<KalturaRuleConditionType, Func<ConditionsMap, HashSet<KalturaRuleActionType>>> 
            AllowedConditionsToRelationValidationsFunc = new Dictionary<KalturaRuleConditionType, Func<ConditionsMap, HashSet<KalturaRuleActionType>>>()
            {
                { KalturaRuleConditionType.COUNTRY, ValidateCountryConditionRelations },
                { KalturaRuleConditionType.CONCURRENCY, ValidateConcurrencyConditionRelations },
                { KalturaRuleConditionType.ASSET, ValidateAssetConditionRelations },
                { KalturaRuleConditionType.IP_RANGE, ValidateIpRangeConditionRelations },
                { KalturaRuleConditionType.OR, NoValidation },
                { KalturaRuleConditionType.HEADER, NoValidation },
                { KalturaRuleConditionType.USER_SESSION_PROFILE, ValidateUserSessionProfileConditionRelations },
            };

        /// <summary>
        /// List of conditions for the rule
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaCondition> Conditions { get; set; }

        /// <summary>
        /// List of actions for the rule
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaAssetRuleAction> Actions { get; set; }

        /// <summary>
        /// List of actions for the rule
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetRuleStatus Status { get; set; }

       
        private static HashSet<KalturaRuleActionType> NoValidation(ConditionsMap existConditions) => null;

        private static HashSet<KalturaRuleActionType> ValidateCountryConditionRelations(ConditionsMap existConditions)
        {
            if (existConditions.Contains(KalturaRuleConditionType.CONCURRENCY))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    "conditions=" + KalturaRuleConditionType.COUNTRY.ToString(),
                    "conditions= " + KalturaRuleConditionType.CONCURRENCY.ToString());
            }
            return null;
        }

        private static HashSet<KalturaRuleActionType> ValidateConcurrencyConditionRelations(ConditionsMap existConditions)
        {
            if (existConditions[KalturaRuleConditionType.CONCURRENCY].Count() > 1)
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "conditions");

            if (existConditions.Contains(KalturaRuleConditionType.COUNTRY) || existConditions.Contains(KalturaRuleConditionType.ASSET))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    "conditions=" + KalturaRuleConditionType.CONCURRENCY.ToString(),
                    "conditions= " + KalturaRuleConditionType.COUNTRY.ToString() + "/" + KalturaRuleConditionType.ASSET.ToString());
            }
            var allowedActions = new HashSet<KalturaRuleActionType>() { KalturaRuleActionType.BLOCK };
            return allowedActions;
        }

        private static HashSet<KalturaRuleActionType> ValidateAssetConditionRelations(ConditionsMap existConditions)
        {
            if (existConditions.Contains(KalturaRuleConditionType.CONCURRENCY))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    "conditions=" + KalturaRuleConditionType.ASSET.ToString(),
                    "conditions= " + KalturaRuleConditionType.CONCURRENCY.ToString());
            }
            return null;
        }

        private static HashSet<KalturaRuleActionType> ValidateIpRangeConditionRelations(ConditionsMap existConditions)
        {
            var allowedActions = new HashSet<KalturaRuleActionType>() { KalturaRuleActionType.ALLOW_PLAYBACK, KalturaRuleActionType.BLOCK_PLAYBACK };
            return allowedActions;
        }

        private static HashSet<KalturaRuleActionType> ValidateUserSessionProfileConditionRelations(ConditionsMap existConditions)
        {
            if (existConditions.Count > 1)
            {
                var otherConditionTypes = existConditions.Select(_ => _.Key).Except(new[]{KalturaRuleConditionType.USER_SESSION_PROFILE});
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    $"conditions.{KalturaRuleConditionType.USER_SESSION_PROFILE}", $"conditions.{string.Join(",", otherConditionTypes)}");
            }
            var allowedActions = new HashSet<KalturaRuleActionType>()
                        {
                            KalturaRuleActionType.FilterAssetByKsql,
                            KalturaRuleActionType.FilterFileByQualityInDiscovery,
                            KalturaRuleActionType.FilterFileByQualityInPlayback,
                            KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInDiscovery,
                            KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInPlayback,
                            KalturaRuleActionType.FilterFileByFileTypeIdInDiscovery,
                            KalturaRuleActionType.FilterFileByFileTypeIdInPlayback,
                            KalturaRuleActionType.FilterFileByAudioCodecInDiscovery,
                            KalturaRuleActionType.FilterFileByAudioCodecInPlayback,
                            KalturaRuleActionType.FilterFileByVideoCodecInDiscovery,
                            KalturaRuleActionType.FilterFileByVideoCodecInPlayback,
                            KalturaRuleActionType.FilterFileByStreamerTypeInDiscovery,
                            KalturaRuleActionType.FilterFileByStreamerTypeInPlayback,
                            KalturaRuleActionType.FilterFileByLabelInDiscovery,
                            KalturaRuleActionType.FilterFileByLabelInPlayback,
                        };

            return allowedActions;
        }

        private void ValidateActions(HashSet<KalturaRuleActionType> allowedActions)
        {
            if (Actions == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }

            var duplicates = this.Actions.GroupBy(x => x.Type).Count(t => t.Count() >= 2);
            if (duplicates > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
            }

            if (allowedActions != null)
            {
                if (!Actions.Any(x => allowedActions.Contains(x.Type)))
                {
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "actions", string.Join(",", allowedActions));
                }
            }

            if (Actions.Any(x => x.Type == KalturaRuleActionType.BLOCK) && Actions.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, $"actions.{Actions[0].objectType}", $"actions.{Actions[1].objectType}");
            }

            foreach (var action in Actions)
            {
                action.Validate();
            }
        }


    }
}