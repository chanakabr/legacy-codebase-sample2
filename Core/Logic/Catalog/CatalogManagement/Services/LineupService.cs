using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.EventBus;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LineupService : ILineupService
    {
        private static readonly Lazy<LineupService> Lazy = new Lazy<LineupService>(
            () => new LineupService(
                CatalogManager.Instance,
                RegionManager.Instance,
                AssetManager.Instance,
                SearchProvider.Instance,
                EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration(),
                new KLogger(nameof(LineupService))),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly IAssetManager _assetManager;
        private readonly ISearchProvider _searchProvider;
        private readonly IEventBusPublisher _publisher;
        private readonly IKLogger _logger;

        public static LineupService Instance => Lazy.Value;

        public LineupService(ICatalogManager catalogManager, IRegionManager regionManager, IAssetManager assetManager, ISearchProvider searchProvider, IEventBusPublisher publisher, IKLogger logger)
        {
            _catalogManager = catalogManager;
            _regionManager = regionManager;
            _assetManager = assetManager;
            _searchProvider = searchProvider;
            _publisher = publisher;
            _logger = logger;
        }

        public GenericListResponse<LineupChannelAsset> GetLineupChannelAssets(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            if (regionId == -1 || regionId == 0)
            {
                var defaultRegionId = _regionManager.GetDefaultRegionId((int)groupId);
                if (defaultRegionId.HasValue)
                {
                    regionId = defaultRegionId.Value;
                }
            }

            var result = regionId > 0
                ? GetRegionLineup(groupId, regionId, searchContext, pageIndex, pageSize)
                : GetNonRegionLineup(groupId, searchContext, pageIndex, pageSize);

            return result;
        }

        private GenericListResponse<LineupChannelAsset> GetRegionLineup(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            var regionResponse = _regionManager.GetRegion(groupId, regionId);
            if (!regionResponse.IsOkStatusCode())
            {
                _logger.Error($"{nameof(IRegionManager.GetRegion)} with parameters groupId:{groupId}, id:{regionId} completed with status {{{regionResponse.ToStringStatus()}}}.");

                return new GenericListResponse<LineupChannelAsset>(regionResponse.Status, null);
            }

            var linearChannelIds = regionResponse.Object.linearChannels
                .Select(x => x.Key)
                .ToArray();
            var linearChannelsKSql = GetLinearChannelsKSql(groupId, linearChannelIds);
            if (string.IsNullOrEmpty(linearChannelsKSql))
            {
                return new GenericListResponse<LineupChannelAsset>(Status.Error, null);
            }

            var searchResult = _searchProvider.SearchAssets(groupId, searchContext, linearChannelsKSql);
            if (!searchResult.status.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResult.status, null);
            }

            var allLinearChannels = regionResponse.Object.linearChannels
                .OrderBy(x => x.Value)
                .Where(x => searchResult.searchResults.Any(_ => long.Parse(_.AssetId) == x.Key))
                .ToArray();
            var pagedLinearChannels = allLinearChannels
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToArray();

            var assetsRequestQuery = pagedLinearChannels
                .Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x.Key))
                .Distinct();
            var linearChannels = _assetManager
                .GetAssets((int)groupId, assetsRequestQuery.ToList(), searchContext.IsAllowedToViewInactiveAssets)?
                .OfType<LiveAsset>()
                .ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, LiveAsset>();

            var pagedLineupChannelAssets = pagedLinearChannels
                .Select(x => new LineupChannelAsset(linearChannels[x.Key], x.Value))
                .ToList();

            var result = new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLineupChannelAssets, allLinearChannels.Length);

            return result;
        }

        private GenericListResponse<LineupChannelAsset> GetNonRegionLineup(long groupId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            var linearChannelsKSql = GetLinearChannelsKSql(groupId, Array.Empty<long>());
            if (string.IsNullOrEmpty(linearChannelsKSql))
            {
                return new GenericListResponse<LineupChannelAsset>(Status.Error, null);
            }

            var searchResult = _searchProvider.SearchAssets(groupId, searchContext, linearChannelsKSql, pageIndex, pageSize);
            if (!searchResult.status.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResult.status, null);
            }

            var pagedLinearChannelIds = searchResult.searchResults
                .Select(x => long.Parse(x.AssetId))
                .ToArray();

            var assetsRequestQuery = pagedLinearChannelIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x));
            var linearChannels = _assetManager
                .GetAssets((int)groupId, assetsRequestQuery.ToList(), searchContext.IsAllowedToViewInactiveAssets)?
                .OfType<LiveAsset>()
                .ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, LiveAsset>();

            var pagedLineupChannelAssets = pagedLinearChannelIds
                .Select(x => new LineupChannelAsset(linearChannels[x], null))
                .ToList();

            var result = new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLineupChannelAssets, searchResult.m_nTotalItems);

            return result;
        }

        private string GetLinearChannelsKSql(long groupId, IReadOnlyCollection<long> linearChannelIds)
        {
            var ksqlBuilder = new StringBuilder("(and");

            var linearAssetTypes = _catalogManager.GetLinearMediaTypes((int)groupId);
            if (linearAssetTypes.Any())
            {
                ksqlBuilder.Append(" (or");
                foreach (var linearAssetType in linearAssetTypes)
                {
                    ksqlBuilder.Append($" asset_type='{linearAssetType.Id}'");
                }

                ksqlBuilder.Append(")");
            }
            else
            {
                _logger.Error($"Linear asset structs were not found. {nameof(groupId)}:{groupId}.");

                return null;
            }

            if (linearChannelIds.Any())
            {
                ksqlBuilder.Append($" (or media_id:'{string.Join(",", linearChannelIds.Distinct())}')");
            }

            ksqlBuilder.Append(")");

            return ksqlBuilder.ToString();
        }

        public Status SendUpdatedNotification(long groupId, string userId, List<long> regionIds)
        {
            var groupRegionIds = _regionManager.GetRegionIds((int)groupId);
            if (groupRegionIds == null || !regionIds.All(x => groupRegionIds.Contains((int)x)))
            {
                _logger.Error($"{nameof(SendUpdatedNotification)} - one of regionIds ({string.Join(",", regionIds)}) don't belong to group {groupId}.");

                return new Status(eResponseStatus.RegionNotFound);
            }

            var requestId = KLogger.GetRequestId();
            var @event = new LineupNotificationRequestedEvent
            {
                GroupId = (int)groupId,
                SiteGuid = userId,
                RequestId = requestId,
                RegionIds = regionIds.Distinct().ToList()
            };

            _publisher.Publish(@event);
            _logger.Debug($"[Lineup updated notification] was sent successfully. requestId:{requestId}");

            return Status.Ok;
        }
    }
}