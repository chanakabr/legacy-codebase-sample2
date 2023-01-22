using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IFilterAssetRule
    {
        string GetFilteringKsql(IReadOnlyCollection<FilterAssetByKsql> rules);
    }

    public class FilterAssetRule : IFilterAssetRule
    {
        private static readonly Lazy<FilterAssetRule> Lazy =
            new Lazy<FilterAssetRule>(() => new FilterAssetRule(), LazyThreadSafetyMode.PublicationOnly);
        public static IFilterAssetRule Instance => Lazy.Value;
        
        public string GetFilteringKsql(IReadOnlyCollection<FilterAssetByKsql> rules)
        {
            return rules.Count == 0
                ? string.Empty
                : KsqlBuilderOld.And(rules.Select(r => r.Ksql));
        }
    }
}