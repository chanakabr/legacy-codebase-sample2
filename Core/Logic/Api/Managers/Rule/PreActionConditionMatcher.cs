using System;
using System.Linq;
using System.Threading;
using ApiObjects.Rules;
using ApiObjects.Rules.PreActionCondition;
using Core.Api.Managers;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IPreActionConditionMatcher
    {
        bool IsMatched(PreActionConditionContext context, AssetRuleFilterAction action, Lazy<FilterMediaFileAsset> asset);
    }
    
    public class PreActionConditionMatcher : IPreActionConditionMatcher
    {
        private static readonly ILogger Logger = new KLogger(nameof(PreActionConditionMatcher));

        private static readonly Lazy<PreActionConditionMatcher> Lazy = new Lazy<PreActionConditionMatcher>(
            () => new PreActionConditionMatcher(AssetUserRuleManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IPreActionConditionMatcher Instance => Lazy.Value;

        private readonly IAssetUserRuleManager _assetUserRuleManager;

        public PreActionConditionMatcher(IAssetUserRuleManager assetUserRuleManager)
        {
            _assetUserRuleManager = assetUserRuleManager;
        }

        public bool IsMatched(PreActionConditionContext context, AssetRuleFilterAction action, Lazy<FilterMediaFileAsset> asset)
            => action.PreActionCondition == null || IsMatchedToPreActionCondition(action.PreActionCondition, asset, context);

        private bool IsMatchedToPreActionCondition(
            BasePreActionCondition preActionCondition,
            Lazy<FilterMediaFileAsset> asset,
            PreActionConditionContext context)
        {
            switch (preActionCondition)
            {
                case ShopPreActionCondition shopPreActionCondition:
                    return IsMatchedToShopPreActionCondition(shopPreActionCondition, asset, context);
                case NoShopPreActionCondition _:
                    return IsMatchedToNoShopPreActionCondition(asset, context);
                default:
                    throw new NotSupportedException($"{nameof(preActionCondition.GetType)} is not supported");
            }
        }
      
        // ShopPreCondition-logic could be moved to a separate class
        private bool IsMatchedToNoShopPreActionCondition(Lazy<FilterMediaFileAsset> asset, PreActionConditionContext context)
        {
            if (!context.AssetUserRules.Any())
            {
                Logger.LogDebug("Asset user rules with asset shop condition does not exist");
                return false;
            }

            if (context.ShopMeta == null)
            {
                Logger.LogDebug("Shop meta is not defined");
                return false;
            }

            var shopInShopConditions = context.AssetUserRules
                .Select(x => x.Conditions.FirstOrDefault(c => c is AssetShopCondition))
                .Where(x => x != null)
                .Cast<AssetShopCondition>()
                .ToList();

            return shopInShopConditions
                .All(x => !_assetUserRuleManager.IsAssetPartOfShopRule(context.ShopMeta, x, asset.Value.Metas, asset.Value.Tags));
        }

        private bool IsMatchedToShopPreActionCondition(
            ShopPreActionCondition shopPreActionCondition,
            Lazy<FilterMediaFileAsset> asset,
            PreActionConditionContext context)
        {
            var assetUserRule = context.GetRuleById(shopPreActionCondition.ShopAssetUserRuleId);
            var condition = assetUserRule.Conditions
                .OfType<AssetShopCondition>()
                .FirstOrDefault();
            if (condition == null)
            {
                Logger.LogDebug("Shop asset user rule {ShopAssetUserRuleId} doesn't have shop condition", shopPreActionCondition.ShopAssetUserRuleId);
                return false;
            }

            if (context.ShopMeta == null)
            {
                Logger.LogDebug("Shop meta is not defined");
                return false;
            }

            return _assetUserRuleManager.IsAssetPartOfShopRule(context.ShopMeta, condition, asset.Value.Metas, asset.Value.Tags);
        }

    }
}