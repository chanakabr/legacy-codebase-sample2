using System;
using System.Collections.Generic;
using System.Threading;
using ApiLogic.Users.Managers;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IFilterAsset
    {
        string UpdateKsql(string ksqlFilter, int groupId, string sessionCharacteristicKey);
    }
    
    public class FilterAsset : IFilterAsset
    {
        private static readonly Lazy<FilterAsset> Lazy =
            new Lazy<FilterAsset>(
                () => new FilterAsset(FilterRuleStorage.Instance, FilterAssetRule.Instance,
                    SessionCharacteristicManager.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static IFilterAsset Instance => Lazy.Value;

        private readonly IFilterRuleStorage _storage;
        private readonly IFilterAssetRule _filterAssetRule;
        private readonly ISessionCharacteristicManager _sessionCharacteristicManager;

        public FilterAsset(IFilterRuleStorage storage, IFilterAssetRule filterAssetRule,
            ISessionCharacteristicManager sessionCharacteristicManager)
        {
            _storage = storage;
            _filterAssetRule = filterAssetRule;
            _sessionCharacteristicManager = sessionCharacteristicManager;
        }

        public string UpdateKsql(string ksqlFilter, int groupId, string sessionCharacteristicKey)
        {
            var userSessionProfileIds = _sessionCharacteristicManager
                .GetFromCache(groupId, sessionCharacteristicKey)?.UserSessionProfileIds;
            var rules = _storage.GetFilterAssetRules(new FilterRuleCondition(userSessionProfileIds, groupId));
            return rules.Count == 0 ? ksqlFilter : KsqlBuilder.And(ksqlFilter, _filterAssetRule.GetFilteringKsql(rules));
        }
    }
}