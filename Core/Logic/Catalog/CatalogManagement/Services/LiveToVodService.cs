using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using DAL;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodService : ILiveToVodService
    {
        public const string LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME = "LiveToVOD";

        private static readonly Lazy<LiveToVodService> LazyInstance = new Lazy<LiveToVodService>(
            () => new LiveToVodService(
                AssetStructRepository.Instance,
                CatalogManager.Instance,
                new KLogger(nameof(LiveToVodService))),
            LazyThreadSafetyMode.PublicationOnly);

        public static LiveToVodService Instance => LazyInstance.Value;

        private readonly IAssetStructRepository _assetStructRepository;
        private readonly ICatalogManager _catalogManager;
        private readonly IKLogger _logger;

        public LiveToVodService(IAssetStructRepository assetStructRepository, ICatalogManager catalogManager, IKLogger logger)
        {
            _assetStructRepository = assetStructRepository;
            _catalogManager = catalogManager;
            _logger = logger;
        }

        public GenericResponse<AssetStruct> AddLiveToVodAssetStruct(int groupId, long userId)
        {
            var response = new GenericResponse<AssetStruct>();
            try
            {
                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
                {
                    _logger.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddLiveToVodAssetStruct", groupId);
                    return response;
                }

                if (!catalogGroupCache.AssetStructsMapById.TryGetValue(catalogGroupCache.GetProgramAssetStructId(), out var programAssetStruct))
                {
                    _logger.ErrorFormat("failed to get program asset struct for groupId: {0} when calling AddLiveToVodAssetStruct", groupId);
                    return response;
                }

                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME))
                {
                    response.SetStatus(eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                    return response;
                }

                var topicsToAdd = programAssetStruct
                    .MetaIds?
                    .Select((x, i) => new KeyValuePair<long, int>(x, ++i))
                    .ToList() ?? new List<KeyValuePair<long, int>>();

                var liveToVodAssetStruct = new AssetStruct
                {
                    Name = "Live To VOD",
                    SystemName = LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME,
                    IsPredefined = false,
                    PluralName = "Live To VOD assets"
                };

                response = _assetStructRepository.InsertAssetStruct(
                    groupId,
                    userId,
                    liveToVodAssetStruct,
                    new List<KeyValuePair<string, string>>(),
                    topicsToAdd);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed AddLiveToVodAssetStruct for groupId: {groupId}", e);
            }

            return response;
        }
    }
}