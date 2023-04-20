using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using ApiObjects.Rules.PreActionCondition;
using Core.Api.Managers;
using TVinciShared;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IFilterAssetRule
    {
        string GetFilteringKsql(int groupId, IReadOnlyCollection<FilterAssetByKsql> rules);
    }

    public class FilterAssetRule : IFilterAssetRule
    {
        private readonly IAssetUserRuleManager _assetUserRuleManager;
        private readonly IShopMarkerService _shopMarkerService;

        private static readonly Lazy<FilterAssetRule> Lazy =
            new Lazy<FilterAssetRule>(
                () => new FilterAssetRule(AssetUserRuleManager.Instance, ShopMarkerService.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        public FilterAssetRule(IAssetUserRuleManager assetUserRuleManager, IShopMarkerService shopMarkerService)
        {
            _assetUserRuleManager = assetUserRuleManager;
            _shopMarkerService = shopMarkerService;
        }

        public static IFilterAssetRule Instance => Lazy.Value;


        public string GetFilteringKsql(int groupId, IReadOnlyCollection<FilterAssetByKsql> rules)
        {
            return rules.Count == 0
                ? string.Empty
                : KsqlBuilderOld.And(rules.Select(r => GetKsqlFromRule(groupId, r)));
        }

        private string GetKsqlFromRule(int groupId, FilterAssetByKsql rule)
        {
            if (rule.PreActionCondition == null)
            {
                return rule.Ksql;
            }

            var context = new PreActionConditionContext(groupId, _assetUserRuleManager, _shopMarkerService);
            switch (rule.PreActionCondition)
            {
                case ShopPreActionCondition shopPreActionCondition:
                    return GetKsqlFromShopPreActionCondition(context, shopPreActionCondition, rule);
                case NoShopPreActionCondition _:
                    return GetKsqlFromNoShopPreActionCondition(context, rule);
                default:
                    throw new NotImplementedException($"Can not handle preAction condition with type {rule.PreActionCondition.GetType()}");
            }
        }

        private static string GetKsqlFromNoShopPreActionCondition(PreActionConditionContext context, FilterAssetByKsql rule)
        {
            if (!context.AssetUserRules.Any())
            {
                return rule.Ksql;
            }

            if (context.ShopMeta == null)
            {
                return rule.Ksql;
            }

            var allValues = context.AssetUserRules
                .Select(x => x.Conditions.FirstOrDefault(c => c is AssetShopCondition))
                .Where(x => x != null)
                .Cast<AssetShopCondition>()
                .SelectMany(x => x.Values)
                .ToList();
            if (!allValues.Any())
            {
                return rule.Ksql;
            }

            return new KsqlBuilder()
                .And(x => x
                    .RawKSql(rule.Ksql)
                    .Values(x.NotEqual, context.ShopMeta.SystemName, allValues))
                .Build();
        }

        private string GetKsqlFromShopPreActionCondition(
            PreActionConditionContext context,
            ShopPreActionCondition shopPreActionCondition,
            FilterAssetByKsql rule)
        {
            var assetUserRule = context.GetRuleById(shopPreActionCondition.ShopAssetUserRuleId);
            var shopCondition = assetUserRule.Conditions.OfType<AssetShopCondition>().FirstOrDefault();
            if (shopCondition == null)
            {
                return rule.Ksql;
            }

            if (context.ShopMeta == null)
            {
                return rule.Ksql;
            }

            return new KsqlBuilder()
                .And(x => x
                    .RawKSql(rule.Ksql)
                    .Or(y => y.Values(y.Equal, context.ShopMeta.SystemName, shopCondition.Values)))
                .Build();
        }
    }
}