using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using Core.Api.Managers;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IFilterRuleStorage
    {
        IReadOnlyCollection<FilterAssetByKsql> GetFilterAssetRules(FilterRuleCondition condition);
        IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForPlayback(FilterRuleCondition condition);
        IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForDiscovery(FilterRuleCondition condition);
    }
    
    public class FilterRuleCondition
    {
        public string SessionCharacteristicsId { get; }
        public int GroupId { get; }

        public FilterRuleCondition(int groupId, string sessionCharacteristicsId)
        {
            SessionCharacteristicsId = sessionCharacteristicsId;
            GroupId = groupId;
        }
    }
    
    public class FilterRuleStorage : IFilterRuleStorage
    {
        private static readonly IReadOnlyCollection<AssetRuleAction> Empty = new List<AssetRuleAction>(0);

        private static readonly Lazy<FilterRuleStorage> Lazy = new Lazy<FilterRuleStorage>(
            () => new FilterRuleStorage(AssetRuleManager.Instance,
                new Lazy<ISessionCharacteristicManager>(() => SessionCharacteristicManager.Instance)),
            LazyThreadSafetyMode.PublicationOnly);
        public static IFilterRuleStorage Instance => Lazy.Value;

        private readonly IAssetRuleManager _assetRuleManager;
        private readonly Lazy<ISessionCharacteristicManager> _sessionCharacteristicManager;

        public FilterRuleStorage(IAssetRuleManager assetRuleManager, Lazy<ISessionCharacteristicManager> sessionCharacteristicManager)
        {
            _assetRuleManager = assetRuleManager;
            _sessionCharacteristicManager = sessionCharacteristicManager;
        }
        
        public IReadOnlyCollection<FilterAssetByKsql> GetFilterAssetRules(FilterRuleCondition condition) =>
            GetRulesActions(condition).OfType<FilterAssetByKsql>().ToList();
        public IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForPlayback(FilterRuleCondition condition) =>
            GetRulesActions(condition).Where(r => r is IFilterFileInPlayback).ToList();
        public IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForDiscovery(FilterRuleCondition condition) =>
            GetRulesActions(condition).Where(r => r is IFilterFileInDiscovery).ToList();
        
        private IEnumerable<AssetRuleAction> GetRulesActions(FilterRuleCondition condition)
        {
            var assetRules = _assetRuleManager
                .GetAssetRules(RuleConditionType.UserSessionProfile, condition.GroupId)
                .GetOrThrow();
            if (assetRules.Count == 0)
            {
                return Empty;
            }

            var sessionCharacteristics = _sessionCharacteristicManager.Value.GetFromCache(condition.GroupId, condition.SessionCharacteristicsId);
            if (sessionCharacteristics == null)
            {
                return Empty;
            }

            return assetRules
                .Where(ar => MatchConditions(ar, sessionCharacteristics.UserSessionProfileIds))
                .SelectMany(ar => ar.Actions);
        }

        private static bool MatchConditions(AssetRule assetRule, IReadOnlyCollection<long> userSessionProfileIds)
        {
            var scope = new UserSessionProfileConditionScope
            {
                RuleId = assetRule.Id,
                UserSessionProfileIds = userSessionProfileIds
            };

            var match = scope.Evaluate(assetRule.Conditions.Single());
            return match;
        }

        private class UserSessionProfileConditionScope : IUserSessionProfileConditionScope
        {
            public long RuleId { get; set; }
            public IReadOnlyCollection<long> UserSessionProfileIds { get; set; }

            public bool Evaluate(RuleCondition condition)
            {
                switch (condition)
                {
                    case UserSessionProfileCondition c: return ConditionsEvaluator.Evaluate(this, c);
                    case OrCondition c: return ConditionsEvaluator.Evaluate(this, c);
                    default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in UserSessionProfileConditionScope");
                }
            }
        }
    }
}