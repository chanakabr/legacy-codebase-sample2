using System;
using System.Threading;

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
                () => new FilterAsset(FilterRuleStorage.Instance, FilterAssetRule.Instance),
                LazyThreadSafetyMode.PublicationOnly);
        public static IFilterAsset Instance => Lazy.Value;

        private readonly IFilterRuleStorage _storage;
        private readonly IFilterAssetRule _filterAssetRule;

        public FilterAsset(IFilterRuleStorage storage, IFilterAssetRule filterAssetRule)
        {
            _storage = storage;
            _filterAssetRule = filterAssetRule;
        }

        public string UpdateKsql(string ksqlFilter, int groupId, string sessionCharacteristicKey)
        {
            var rules = _storage.GetFilterAssetRules(
                new FilterRuleCondition(groupId, sessionCharacteristicKey));
            return rules.Count == 0 ? ksqlFilter : KsqlBuilderOld.And(ksqlFilter, _filterAssetRule.GetFilteringKsql(groupId, rules));
        }
    }
}