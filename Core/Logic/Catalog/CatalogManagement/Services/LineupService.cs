using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.EventBus;
using ApiObjects.Lineup;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
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
                FilterAsset.Instance,
                new KLogger(nameof(LineupService))),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ICatalogManager _catalogManager;
        private readonly IRegionManager _regionManager;
        private readonly IAssetManager _assetManager;
        private readonly ISearchProvider _searchProvider;
        private readonly IEventBusPublisher _publisher;
        private readonly IFilterAsset _filterAsset;
        private readonly IKLogger _logger;

        public static LineupService Instance => Lazy.Value;

        public LineupService(ICatalogManager catalogManager, IRegionManager regionManager, IAssetManager assetManager,
            ISearchProvider searchProvider, IEventBusPublisher publisher, IFilterAsset filterAsset, IKLogger logger)
        {
            _catalogManager = catalogManager;
            _regionManager = regionManager;
            _assetManager = assetManager;
            _searchProvider = searchProvider;
            _publisher = publisher;
            _filterAsset = filterAsset;
            _logger = logger;
        }

        public GenericListResponse<LineupChannelAsset> GetLineupChannelAssets(long groupId, long regionId,
            UserSearchContext searchContext, int pageIndex, int pageSize)
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

        public GenericListResponse<LineupChannelAsset> GetLineupChannelAssetsWithFilter(
            UserSearchContext userSearchContext,
            LineupRegionalChannelRequest request)
        {
            // Apply filter by region and LCN
            var linearChannelLcnResponse = GetLinearChannelsByRegion(request);
            if (!linearChannelLcnResponse.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(linearChannelLcnResponse.Status, null);
            }

            var linearChannelLcn = FilterByLcn(request, linearChannelLcnResponse.Object);

            return request.OrderBy == LineupRegionalChannelOrderBy.LCN_ASC ||
                request.OrderBy == LineupRegionalChannelOrderBy.LCN_DESC
                    ? GetRegionLineupOrderedByLcn(userSearchContext, request, linearChannelLcn)
                    : GetRegionLineupOrderedByLinearChannel(userSearchContext, request, linearChannelLcn);
        }

        private GenericListResponse<LineupChannelAsset> GetRegionLineupOrderedByLinearChannel(
            UserSearchContext userSearchContext,
            LineupRegionalChannelRequest request,
            ICollection<KeyValuePair<long, int>> linearChannelLcn)
        {
            var pageSize = (request.PageIndex + 1) * request.PageSize;
            var orderDirection = request.OrderBy == LineupRegionalChannelOrderBy.NAME_ASC
                ? OrderDir.ASC
                : OrderDir.DESC;
            var order = new List<AssetOrder> { new AssetOrder { Field = OrderBy.NAME, Direction = orderDirection } };

            var builder = new UnifiedSearchRequestBuilder(_filterAsset)
                .WithPageSize(pageSize)
                .WithOrdering(order);
            // Get all linear channels after filter by LCN and by KSQL
            var searchResponse = SearchAssets(request.PartnerId, builder, userSearchContext, linearChannelLcn, request.Ksql);
            if (!searchResponse.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResponse.Status, null);
            }

            // Get all pairs <linear_channel_id, lcn>
            var linearChannelLcnLookup = linearChannelLcn.ToLookup(x => x.Key);
            var allLinearChannelToLcn = searchResponse.Object.searchResults
                .SelectMany(x => linearChannelLcnLookup[long.Parse(x.AssetId)].OrderBy(y => y.Value))
                .ToArray();

            // Apply paging and map result
            return MapResponse(allLinearChannelToLcn, request.PartnerId, request.PageIndex, request.PageSize,
                userSearchContext);

        }

        private GenericListResponse<LineupChannelAsset> GetRegionLineupOrderedByLcn(
            UserSearchContext userSearchContext,
            LineupRegionalChannelRequest request,
            ICollection<KeyValuePair<long, int>> linearChannelLcn)
        {
            var builder = new UnifiedSearchRequestBuilder(_filterAsset);
            // Get all linear channels after filter by LCN and by KSQL
            var searchResponse = SearchAssets(request.PartnerId, builder, userSearchContext, linearChannelLcn, request.Ksql);
            if (!searchResponse.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResponse.Status, null);
            }

            // Get all pairs <linear_channel_id, lcn> ordered by LCN
            var linearChannelIds = searchResponse.Object.searchResults
                .Select(x => long.Parse(x.AssetId))
                .ToHashSet();
            var filteredLinearChannelLcn = linearChannelLcn
                .Where(x => linearChannelIds.Contains(x.Key));
            var orderedLinearChannelLcn = request.OrderBy == LineupRegionalChannelOrderBy.LCN_ASC
                ? filteredLinearChannelLcn.OrderBy(x => x.Value).ToArray()
                : filteredLinearChannelLcn.OrderByDescending(x => x.Value).ToArray();

            // Apply paging and map result
            return MapResponse(orderedLinearChannelLcn, request.PartnerId, request.PageIndex, request.PageSize,
                userSearchContext);
        }

        private GenericListResponse<LineupChannelAsset> GetRegionLineup(long groupId, long regionId,
            UserSearchContext userSearchContext, int pageIndex, int pageSize)
        {
            var regionResponse = _regionManager.GetRegion(groupId, regionId);
            if (!regionResponse.IsOkStatusCode())
            {
                _logger.Error(
                    $"{nameof(IRegionManager.GetRegion)} with parameters groupId:{groupId}, id:{regionId} completed with status {{{regionResponse.ToStringStatus()}}}.");

                return new GenericListResponse<LineupChannelAsset>(regionResponse.Status, null);
            }

            var builder = new UnifiedSearchRequestBuilder(_filterAsset);
            var searchResponse = SearchAssets(groupId, builder, userSearchContext, regionResponse.Object.linearChannels);
            // Get all linear channels
            if (!searchResponse.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResponse.Status, null);
            }

            // Get all pairs <linear_channel_id, lcn> ordered by LCN
            var linearChannelIds = searchResponse.Object.searchResults
                .Select(x => long.Parse(x.AssetId))
                .ToHashSet();
            var allLinearChannelLcn = regionResponse.Object.linearChannels
                .Where(x => linearChannelIds.Contains(x.Key))
                .OrderBy(x => x.Value)
                .ToArray();

            // Apply paging and map result
            return MapResponse(allLinearChannelLcn, groupId, pageIndex, pageSize, userSearchContext);
        }

        private GenericListResponse<LineupChannelAsset> GetNonRegionLineup(long groupId, UserSearchContext userSearchContext, int pageIndex, int pageSize)
        {
            var builder = new UnifiedSearchRequestBuilder(_filterAsset)
                .WithPageIndex(pageIndex)
                .WithPageSize(pageSize);

            var searchResponse = SearchAssets(groupId, builder, userSearchContext, Array.Empty<KeyValuePair<long, int>>());
            if (!searchResponse.IsOkStatusCode())
            {
                return new GenericListResponse<LineupChannelAsset>(searchResponse.Status, null);
            }

            var pagedLinearChannelIds = searchResponse.Object.searchResults
                .Select(x => long.Parse(x.AssetId))
                .ToArray();

            var assetsRequestQuery =
                pagedLinearChannelIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x));
            var linearChannels = _assetManager
                .GetAssets((int)groupId, assetsRequestQuery.ToList(), userSearchContext.IsAllowedToViewInactiveAssets)?
                .OfType<LiveAsset>()
                .ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, LiveAsset>();

            var pagedLineupChannelAssets = pagedLinearChannelIds
                .Select(x => new LineupChannelAsset(linearChannels[x], null))
                .ToList();

            return new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLineupChannelAssets,
                searchResponse.Object.m_nTotalItems);
        }

        private GenericResponse<UnifiedSearchResponse> SearchAssets(
            long partnerId,
            UnifiedSearchRequestBuilder builder,
            UserSearchContext context,
            ICollection<KeyValuePair<long, int>> linearChannelLcn,
            string additionalFilterQuery = null)
        {
            var linearAssetTypes = _catalogManager.GetLinearMediaTypes((int)partnerId);
            if (!linearAssetTypes.Any())
            {
                _logger.Error($"Linear asset structs were not found. {nameof(partnerId)}:{partnerId}.");

                return new GenericResponse<UnifiedSearchResponse>(Status.Error, null);
            }

            var filterQuery = BuildQueryByLinearChannels(linearAssetTypes, linearChannelLcn, additionalFilterQuery);
            var searchRequest = builder
                .WithFilterQuery(filterQuery)
                .Build(partnerId, context);
            if (searchRequest == null)
            {
                return new GenericResponse<UnifiedSearchResponse>(Status.Error, null);
            }

            var result = _searchProvider.SearchAssets(searchRequest);

            return !result.status.IsOkStatusCode()
                ? new GenericResponse<UnifiedSearchResponse>(result.status, null)
                : new GenericResponse<UnifiedSearchResponse>(Status.Ok, result);
        }

        private GenericResponse<ICollection<KeyValuePair<long, int>>> GetLinearChannelsByRegion(
            LineupRegionalChannelRequest request)
        {
            var regionFilter = new RegionFilter
            {
                RegionIds = new List<int> { (int)request.RegionId },
                ExclusiveLcn = !request.ParentRegionIncluded
            };

            var regionResponse = _regionManager.GetRegions(request.PartnerId, regionFilter);
            if (!regionResponse.IsOkStatusCode())
            {
                return new GenericResponse<ICollection<KeyValuePair<long, int>>>(regionResponse.Status);
            }

            if (!regionResponse.HasObjects())
            {
                return new GenericResponse<ICollection<KeyValuePair<long, int>>>(eResponseStatus.RegionNotFound);
            }

            return new GenericResponse<ICollection<KeyValuePair<long, int>>>(
                Status.Ok,
                regionResponse.Objects.First().linearChannels);
        }

        private static ICollection<KeyValuePair<long, int>> FilterByLcn(
            LineupRegionalChannelRequest request,
            ICollection<KeyValuePair<long, int>> linearChannelLcn)
        {
            if (!request.LcnGreaterThanOrEqual.HasValue && !request.LcnLessThanOrEqual.HasValue)
            {
                return linearChannelLcn;
            }

            var filteredLinearChannelLcn = linearChannelLcn.AsEnumerable();
            if (request.LcnGreaterThanOrEqual.HasValue)
            {
                filteredLinearChannelLcn = filteredLinearChannelLcn.Where(x => x.Value >= request.LcnGreaterThanOrEqual.Value);
            }

            if (request.LcnLessThanOrEqual.HasValue)
            {
                filteredLinearChannelLcn = filteredLinearChannelLcn.Where(x => x.Value <= request.LcnLessThanOrEqual.Value);
            }

            return filteredLinearChannelLcn.ToList();
        }

        private GenericListResponse<LineupChannelAsset> MapResponse(
            KeyValuePair<long, int>[] orderedLinearChannelLcn,
            long partnerId,
            int pageIndex,
            int pageSize,
            UserSearchContext searchContext)
        {
            var pagedLinearChannels = orderedLinearChannelLcn
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToArray();
            var assetsRequestQuery = pagedLinearChannels
                .Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x.Key))
                .Distinct();
            var linearChannels = _assetManager
                .GetAssets(partnerId, assetsRequestQuery.ToList(), searchContext.IsAllowedToViewInactiveAssets)?
                .OfType<LiveAsset>()
                .ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, LiveAsset>();
            var pagedLineupChannelAssets = pagedLinearChannels
                .Select(x => new LineupChannelAsset(linearChannels[x.Key], x.Value))
                .ToList();

            return new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLineupChannelAssets, orderedLinearChannelLcn.Length);
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

        private static string BuildQueryByLinearChannels(
            IEnumerable<AssetStruct> linearChannelTypes,
            ICollection<KeyValuePair<long, int>> linearChannelLcn,
            string additionalFilterQuery = null)
        {
            return new TVinciShared.KsqlBuilder()
                .And(x =>
                {
                    x.AnyAssetTypes(linearChannelTypes.Select(t => t.Id));
                    if (linearChannelLcn.Any())
                    {
                        x.AnyMediaIds(linearChannelLcn.Select(l => l.Key).Distinct());
                    }

                    if (!string.IsNullOrEmpty(additionalFilterQuery))
                    {
                        x.RawKSql(additionalFilterQuery);
                    }
                }).Build();
        }
    }
}