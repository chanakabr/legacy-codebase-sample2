using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public IReadOnlyCollection<long> UserSessionProfileIds { get; }
        public int GroupId { get; }

        public FilterRuleCondition(IReadOnlyCollection<long> userSessionProfileIds, int groupId)
        {
            UserSessionProfileIds = userSessionProfileIds;
            GroupId = groupId;
        }
    }
    
    public class FilterRuleStorage : IFilterRuleStorage
    {
        private static readonly IReadOnlyCollection<AssetRuleAction> Empty = new List<AssetRuleAction>(0);
        private static readonly Lazy<FilterRuleStorage> Lazy =
            new Lazy<FilterRuleStorage>(() => new FilterRuleStorage(AssetRuleManager.Instance), 
            LazyThreadSafetyMode.PublicationOnly);
        public static IFilterRuleStorage Instance => Lazy.Value;

        private readonly IAssetRuleManager _assetRuleManager;

        public FilterRuleStorage(IAssetRuleManager assetRuleManager) 
        {
            _assetRuleManager = assetRuleManager;
        }
        
        public IReadOnlyCollection<FilterAssetByKsql> GetFilterAssetRules(FilterRuleCondition condition) =>
            GetRulesActions(condition).OfType<FilterAssetByKsql>().ToList();
        public IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForPlayback(FilterRuleCondition condition) =>
            GetRulesActions(condition).Where(r => r is IFilterFileInPlayback).ToList();
        public IReadOnlyCollection<AssetRuleAction> GetFilterFileRulesForDiscovery(FilterRuleCondition condition) =>
            GetRulesActions(condition).Where(r => r is IFilterFileInDiscovery).ToList();
        
        private IEnumerable<AssetRuleAction> GetRulesActions(FilterRuleCondition condition)
        {
            if (condition.UserSessionProfileIds == null || condition.UserSessionProfileIds.Count == 0) return Empty;
                
            var assetRules = _assetRuleManager.GetAssetRules(RuleConditionType.UserSessionProfile, condition.GroupId);
            return assetRules
                .GetOrThrow()
                .Where(ar => MatchConditions(ar, condition))
                .SelectMany(ar => ar.Actions);
        }

        private static bool MatchConditions(AssetRule assetRule, FilterRuleCondition condition)
        {
            var scope = new UserSessionProfileConditionScope { RuleId = assetRule.Id, UserSessionProfileIds = condition.UserSessionProfileIds };
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