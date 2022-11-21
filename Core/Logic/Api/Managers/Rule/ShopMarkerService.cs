using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog;
using Core.Catalog.CatalogManagement;

namespace ApiLogic.Api.Managers.Rule
{
    public class ShopMarkerService : IShopMarkerService
    {
        private static readonly Lazy<IShopMarkerService> Lazy = new Lazy<IShopMarkerService>(
            () => new ShopMarkerService(CatalogPartnerConfigManager.Instance, TopicManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ICatalogPartnerConfigManager _catalogPartnerConfigManager;
        private readonly ITopicManager _topicManager;

        public static IShopMarkerService Instance => Lazy.Value;

        public ShopMarkerService(ICatalogPartnerConfigManager catalogPartnerConfigManager, ITopicManager topicManager)
        {
            _catalogPartnerConfigManager = catalogPartnerConfigManager;
            _topicManager = topicManager;
        }

        public GenericResponse<Topic> GetShopMarkerTopic(long groupId)
        {
            GenericResponse<Topic> response;

            var configResponse = _catalogPartnerConfigManager.GetCatalogConfig((int)groupId);
            if (!configResponse.IsOkStatusCode())
            {
                response = new GenericResponse<Topic>(configResponse.Status);
            }
            else if (!configResponse.Object.ShopMarkerMetaId.HasValue)
            {
                response = new GenericResponse<Topic>(eResponseStatus.TopicNotFound);
            }
            else
            {
                var topicsResponse = _topicManager.GetTopicsByIds((int)groupId, new List<long> { configResponse.Object.ShopMarkerMetaId.Value }, MetaType.All);
                var shopMarkerMeta = topicsResponse.Objects?.FirstOrDefault();
                response = shopMarkerMeta == null
                    ? new GenericResponse<Topic>(eResponseStatus.TopicNotFound)
                    : new GenericResponse<Topic>(Status.Ok, shopMarkerMeta);
            }

            return response;
        }

        public Status SetShopMarkerMeta(long groupId, AssetStruct assetStruct, Asset asset, AssetUserRule assetUserRule)
        {
            var shopCondition = assetUserRule.Conditions.OfType<AssetShopCondition>().SingleOrDefault();
            if (shopCondition == null)
            {
                return Status.Error;
            }

            var shopMarkerTopicResponse = GetShopMarkerTopic(groupId);
            if (!shopMarkerTopicResponse.IsOkStatusCode())
            {
                return shopMarkerTopicResponse.Status;
            }

            var shopMarkerTopic = shopMarkerTopicResponse.Object;
            if (!assetStruct.MetaIds.Contains(shopMarkerTopic.Id))
            {
                return Status.Ok;
            }

            bool shopMarkerTopicPopulted = false;

            if (shopMarkerTopic.Type == MetaType.Tag)
            {
                shopMarkerTopicPopulted = asset.Tags.Any(x => x.m_oTagMeta.m_sName == shopMarkerTopic.SystemName);
            }
            else
            {
                shopMarkerTopicPopulted = asset.Metas.Any(x => x.m_oTagMeta.m_sName == shopMarkerTopic.SystemName);
            }

            if (!shopMarkerTopicPopulted)
            {
                TagMeta tm = new TagMeta()
                {
                    m_sName = shopMarkerTopic.SystemName,
                    m_sType = shopMarkerTopic.Type.ToString()
                };

                if (shopMarkerTopic.Type == MetaType.Tag)
                {
                    Tags tag = new Tags()
                    {
                        m_oTagMeta = tm,
                        m_lValues = shopCondition.Values
                    };

                    asset.Tags.Add(tag);
                }
                else
                {
                    string filter = shopCondition.Values[0];

                    asset.Metas.Add(new Metas()
                    {
                        m_oTagMeta = tm,
                        m_sValue = filter
                    });
                }
            }

            return Status.Ok;
        }
    }
}