using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Api.Managers;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ShopAssetUserRuleResolver : IShopAssetUserRuleResolver
    {
        private static readonly KLogger Logger = new KLogger(nameof(ShopAssetUserRuleResolver));

        private readonly IShopMarkerService _shopMarkerService;
        private readonly IAssetUserRuleManager _assetUserRuleManager;
        private readonly ICatalogManager _catalogManager;

        private static readonly Lazy<IShopAssetUserRuleResolver> LazyInstance =
            new Lazy<IShopAssetUserRuleResolver>(
                () => new ShopAssetUserRuleResolver(
                    ShopMarkerService.Instance,
                    AssetUserRuleManager.Instance,
                    CatalogManager.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        public static IShopAssetUserRuleResolver Instance = LazyInstance.Value;

        public ShopAssetUserRuleResolver(
            IShopMarkerService shopMarkerService,
            IAssetUserRuleManager assetUserRuleManager,
            ICatalogManager catalogManager)
        {
            _shopMarkerService = shopMarkerService;
            _assetUserRuleManager = assetUserRuleManager;
            _catalogManager = catalogManager;
        }

        public AssetUserRule ResolveByMediaAsset(int groupId, string assetType, IEnumerable<Metas> metas, IEnumerable<Tags> tags)
        {
            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var cache)
                || string.IsNullOrEmpty(assetType)
                || !cache.AssetStructsMapBySystemName.TryGetValue(assetType, out var assetStruct))
            {
                return null;
            }

            var shopMeta = GetShopMeta(groupId);
            if (shopMeta == null) return null;
            if (!assetStruct.MetaIds.Contains(shopMeta.Id))
            {
                Logger.DebugFormat("Asset type {0} does not contain shop meta {1}", assetType, shopMeta.SystemName);

                return null;
            }

            var assetUserRules = GetAssetUserRules(groupId);
            foreach (var assetUserRule in assetUserRules)
            {
                var shopCondition = assetUserRule.Conditions.OfType<AssetShopCondition>().FirstOrDefault();
                if (shopCondition == null)
                {
                    continue;
                }

                if (!_assetUserRuleManager.IsAssetPartOfShopRule(shopMeta, shopCondition, metas, tags))
                {
                    continue;
                }

                return assetUserRule;
            }

            return null;
        }

        private Topic GetShopMeta(int groupId)
        {
            var shopMetaResponse = _shopMarkerService.GetShopMarkerTopic(groupId);
            if (shopMetaResponse.IsOkStatusCode())
            {
                return shopMetaResponse.Object;
            }

            if (shopMetaResponse.Status.Code == (int)eResponseStatus.TopicNotFound)
            {
                Logger.InfoFormat("Topic meta is not set for groupId={0}", groupId);

                return null;
            }

            Logger.ErrorFormat("Failed to retrieve shop meta for groupId={0}", groupId);

            return null;
        }

        private IEnumerable<AssetUserRule> GetAssetUserRules(int groupId)
        {
            var response = _assetUserRuleManager.GetAssetUserRuleList(
                groupId,
                null,
                true,
                RuleActionType.UserFilter,
                RuleConditionType.AssetShop);

            return response.IsOkStatusCode() ? response.Objects.ToArray() : Enumerable.Empty<AssetUserRule>();
        }
    }
}