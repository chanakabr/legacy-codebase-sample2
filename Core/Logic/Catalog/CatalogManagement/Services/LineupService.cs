using System;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiObjects.Response;
using KLogMonitor;

namespace Core.Catalog.CatalogManagement
{
    public class LineupService : ILineupService
    {
        private static readonly Lazy<LineupService> Lazy = new Lazy<LineupService>(() => new LineupService(RegionManager.Instance, AssetManager.Instance, new KLogger(nameof(LineupService))), LazyThreadSafetyMode.PublicationOnly);
        private readonly IRegionManager _regionManager;
        private readonly IAssetManager _assetManager;
        private readonly IKLogger _logger;

        public static LineupService Instance => Lazy.Value;

        public LineupService(IRegionManager regionManager, IAssetManager assetManager, IKLogger logger)
        {
            _regionManager = regionManager;
            _assetManager = assetManager;
            _logger = logger;
        }

        public GenericListResponse<LineupChannelAsset> GetLineupChannelAssets(long groupId, long regionId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            // TODO: Currently AssetUserRule and GeoBlockRule are ignored so we use this UserSearchContext
            var searchContextWithoutRules = new UserSearchContext(0, 0, 0, null, null, searchContext.IgnoreEndDate, searchContext.UseStartDate, searchContext.UseFinal, searchContext.GetOnlyActiveAssets, searchContext.IsAllowedToViewInactiveAssets);

            var result = regionId > 0
                ? GetRegionLineup(groupId, regionId, searchContextWithoutRules, pageIndex, pageSize)
                : GetNonRegionLineup(groupId, searchContextWithoutRules, pageIndex, pageSize);

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

            var linearChannelsIds = regionResponse.Object.linearChannels.Select(x => x.Key).Distinct();
            var linearChannelsResponse = _assetManager.GetLinearChannels(groupId, linearChannelsIds, searchContext);
            if (!linearChannelsResponse.IsOkStatusCode())
            {
                _logger.Error($"{nameof(IAssetManager.GetLinearChannels)} with parameters {nameof(searchContext)}:{searchContext} completed with status {{{linearChannelsResponse.Status.Code} - {linearChannelsResponse.Status.Message}}}.");

                return new GenericListResponse<LineupChannelAsset>(linearChannelsResponse.Status, null);
            }

            var linearChannels = linearChannelsResponse.Objects
                .OfType<LiveAsset>()
                .ToDictionary(x => x.Id, x => x);

            var pagedLineupChannelAssets = regionResponse.Object.linearChannels
                .OrderBy(x => x.Value)
                .Where(x => linearChannels.Values.Any(_ => _.Id == x.Key))
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new LineupChannelAsset(linearChannels[x.Key], x.Value))
                .ToList();

            var result = new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLineupChannelAssets, linearChannels.Count);

            return result;
        }

        private GenericListResponse<LineupChannelAsset> GetNonRegionLineup(long groupId, UserSearchContext searchContext, int pageIndex, int pageSize)
        {
            var linearChannelsResponse = _assetManager.GetLinearChannels(groupId, Enumerable.Empty<long>(), searchContext);
            if (!linearChannelsResponse.IsOkStatusCode())
            {
                _logger.Error($"{nameof(IAssetManager.GetLinearChannels)} with parameters {nameof(searchContext)}:{searchContext} completed with status {{{linearChannelsResponse.Status.Code} - {linearChannelsResponse.Status.Message}}}.");

                return new GenericListResponse<LineupChannelAsset>(linearChannelsResponse.Status, null);
            }

            var linearChannels = linearChannelsResponse.Objects
                .OfType<LiveAsset>()
                .ToArray();

            var pagedLinearChannels = linearChannels
                .OrderBy(x => x.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new LineupChannelAsset(x, null))
                .ToList();

            var result = new GenericListResponse<LineupChannelAsset>(Status.Ok, pagedLinearChannels, linearChannels.Length);

            return result;
        }
    }
}