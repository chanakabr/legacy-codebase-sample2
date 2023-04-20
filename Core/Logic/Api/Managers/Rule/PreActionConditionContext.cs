using System.Collections.Generic;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Api.Managers;

namespace ApiLogic.Api.Managers.Rule
{
    public class PreActionConditionContext
    {
        private readonly int _groupId;
        private readonly IAssetUserRuleManager _assetUserRuleManager;
        private readonly IShopMarkerService _shopMarkerService;

        private readonly Dictionary<long, AssetUserRule> _rules = new Dictionary<long, AssetUserRule>();
        private Topic _shopMeta;
        private List<AssetUserRule> _assetUserRules;

        public PreActionConditionContext(
            int groupId,
            IAssetUserRuleManager assetUserRuleManager,
            IShopMarkerService shopMarkerService)
        {
            _groupId = groupId;
            _assetUserRuleManager = assetUserRuleManager;
            _shopMarkerService = shopMarkerService;
        }

        public AssetUserRule GetRuleById(long ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                return rule;
            }

            var assetUserRule = _assetUserRuleManager.GetCachedAssetUserRuleByRuleId(_groupId, ruleId).GetOrThrow();
            _rules.Add(ruleId, assetUserRule);

            return assetUserRule;
        }

        public IEnumerable<AssetUserRule> AssetUserRules
        {
            get
            {
                if (_assetUserRules != null)
                {
                    return _assetUserRules;
                }

                _assetUserRules = _assetUserRuleManager.GetAssetUserRuleList(
                        _groupId,
                        null,
                        true,
                        RuleActionType.UserFilter,
                        RuleConditionType.AssetShop)
                    .GetOrThrow();

                return _assetUserRules;
            }
        }

        public Topic ShopMeta
        {
            get
            {
                if (_shopMeta != null)
                {
                    return _shopMeta;
                }
                var shopMetaResponse = _shopMarkerService.GetShopMarkerTopic(_groupId);
                if (shopMetaResponse.IsOkStatusCode())
                {
                    _shopMeta = shopMetaResponse.Object;

                    return _shopMeta;
                }

                if (shopMetaResponse.Status.Code == (int)eResponseStatus.TopicNotFound)
                {
                    return null;
                }

                throw new KalturaException(shopMetaResponse.Status.Message, shopMetaResponse.Status.Code);
            }
        }
    }
}