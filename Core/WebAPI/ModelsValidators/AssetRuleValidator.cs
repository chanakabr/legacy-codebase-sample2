using System;
using System.Collections.Generic;
using Nest;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using System.Linq;

namespace WebAPI.ModelsValidators
{
    using ConditionsMap = ILookup<KalturaRuleConditionType, KalturaCondition>;
    public static class AssetRuleValidator
    {
        private static readonly Dictionary<KalturaRuleConditionType, Func<ConditionsMap, HashSet<KalturaRuleActionType>>> 
            AllowedConditionsToRelationValidationsFunc = new Dictionary<KalturaRuleConditionType, Func<ConditionsMap, HashSet<KalturaRuleActionType>>>()
            {
                { KalturaRuleConditionType.COUNTRY, ValidateCountryConditionRelations },
                { KalturaRuleConditionType.CONCURRENCY, ValidateConcurrencyConditionRelations },
                { KalturaRuleConditionType.ASSET, ValidateAssetConditionRelations },
                { KalturaRuleConditionType.IP_RANGE, ValidateIpRangeConditionRelations },
                { KalturaRuleConditionType.IP_V6_RANGE, ValidateIpV6RangeConditionRelations },
                { KalturaRuleConditionType.OR, NoValidation },
                { KalturaRuleConditionType.HEADER, NoValidation },
                { KalturaRuleConditionType.USER_SESSION_PROFILE, ValidateUserSessionProfileConditionRelations },
            };
        
        public static void Validate(this KalturaAssetRule model)
        {
            if (model.Conditions == null || model.Conditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }

            var existConditions = model.Conditions.ToLookup(x => x.Type);
            HashSet<KalturaRuleActionType> allowedActions = null;
            foreach (var condition in model.Conditions)
            {
                if (!AllowedConditionsToRelationValidationsFunc.TryGetValue(condition.Type, out var validationFunc))
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, $"{condition.objectType}");
                }

                // validate the relation of current condition to other existing conditions + get allowed actions of this condition
                var allowed = validationFunc(existConditions);

                condition.Validate();

                if (allowedActions == null)
                {
                    allowedActions = allowed;
                }
            }

            model.ValidateActions(allowedActions);
        }
        
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

        private static HashSet<KalturaRuleActionType> ValidateIpV6RangeConditionRelations(ConditionsMap existConditions)
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
                            KalturaRuleActionType.FilterFileByDynamicDataInDiscovery,
                            KalturaRuleActionType.FilterFileByDynamicDataInPlayback
                        };

            return allowedActions;
        }

        private static void ValidateActions(this KalturaAssetRule model, HashSet<KalturaRuleActionType> allowedActions)
        {
            if (model.Actions == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }

            var duplicates = model.Actions.GroupBy(x => x.Type).Count(t => t.Count() >= 2);
            if (duplicates > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
            }

            if (allowedActions != null)
            {
                if (!model.Actions.Any(x => allowedActions.Contains(x.Type)))
                {
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "actions", string.Join(",", allowedActions));
                }
            }

            if (model.Actions.Any(x => x.Type == KalturaRuleActionType.BLOCK) && model.Actions.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, $"actions.{model.Actions[0].objectType}", $"actions.{model.Actions[1].objectType}");
            }

            foreach (var action in model.Actions)
            {
                action.Validate();
            }
        }
    }
}