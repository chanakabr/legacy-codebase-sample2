using System;
using System.Text;
using System.Threading;
using ApiObjects.Rules;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;

namespace ApiLogic.Api.Managers.Rule
{
    public class AssetConditionKsqlFactory : IAssetConditionKsqlFactory
    {
        private static readonly Lazy<AssetConditionKsqlFactory> Lazy = new Lazy<AssetConditionKsqlFactory>(
            () => new AssetConditionKsqlFactory(ShopMarkerService.Instance, new KLogger(nameof(AssetConditionKsqlFactory))),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IShopMarkerService _shopMarkerService;
        private readonly ILogger _logger;

        public static IAssetConditionKsqlFactory Instance => Lazy.Value;

        public AssetConditionKsqlFactory(IShopMarkerService shopMarkerService, ILogger logger)
        {
            _shopMarkerService = shopMarkerService;
            _logger = logger;
        }

        public string GetKsql(long groupId, AssetConditionBase condition)
        {
            if (condition is AssetCondition assetCondition)
            {
                return assetCondition.Ksql;
            }

            if (condition is AssetShopCondition assetShopCondition)
            {
                return GetAssetShopConditionKsql(groupId, assetShopCondition);
            }

            throw new NotSupportedException($"Ksql can not be determined for {condition.GetType().Name} type.");
        }

        private string GetAssetShopConditionKsql(long groupId, AssetShopCondition condition)
        {
            var shopMetaResponse = _shopMarkerService.GetShopMarkerTopic(groupId);
            if (!shopMetaResponse.IsOkStatusCode())
            {
                _logger.LogError($"ShopMarkerMeta has not been determined and Ksql can not be built: {nameof(shopMetaResponse)}.{nameof(shopMetaResponse.Status)}={{{shopMetaResponse.Status.Code} - {shopMetaResponse.Status.Message}}}.");

                throw new Exception(shopMetaResponse.Status.Message);
            }

            if (condition.Values?.Count > 0)
            {
                StringBuilder query = new StringBuilder("(or ");
                
                foreach (var value in condition.Values)
                {
                    query.Append($"{shopMetaResponse.Object.SystemName}='{value}' ");
                }
                
                query.Append(")");
                
                return query.ToString();
            }

            return $"{shopMetaResponse.Object.SystemName}='{condition.Value}'";
        }
    }
}