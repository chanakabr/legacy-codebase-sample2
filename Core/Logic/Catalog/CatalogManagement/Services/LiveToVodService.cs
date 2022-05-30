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
        public const string LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME = "LiveToVod";

        private static readonly Lazy<LiveToVodService> LazyInstance = new Lazy<LiveToVodService>(
            () => new LiveToVodService(AssetStructRepository.Instance, CatalogManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static LiveToVodService Instance => LazyInstance.Value;

        private readonly IAssetStructRepository _assetStructRepository;
        private readonly ICatalogManager _catalogManager;
        private readonly IKLogger _logger;
        
        public LiveToVodService(IAssetStructRepository assetStructRepository, ICatalogManager catalogManager) 
            : this(assetStructRepository, catalogManager, new KLogger(nameof(LiveToVodService)))
        {
        }

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
                
                var metaIdsToAdd = catalogGroupCache.TopicsMapBySystemNameAndByType
                    .Where(x => AssetManager.BasicMetasSystemNamesToType.ContainsKey(x.Key)
                        && x.Value.ContainsKey(AssetManager.BasicMetasSystemNamesToType[x.Key]))
                    .Select(x => x.Value[AssetManager.BasicMetasSystemNamesToType[x.Key]].Id)
                    .ToHashSet();
                if (programAssetStruct.MetaIds != null)
                {
                    metaIdsToAdd.UnionWith(programAssetStruct.MetaIds);
                }

                var topicsToAdd = metaIdsToAdd
                    .Select((x, i) => new KeyValuePair<long, int>(x, ++i))
                    .ToList();

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
                
                _catalogManager.InvalidateCatalogGroupCache(groupId, response.Status, true, response.Object);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed AddLiveToVodAssetStruct for groupId: {groupId}", e);
            }

            return response;
        }

        public GenericResponse<AssetStruct> GetLiveToVodAssetStruct(int groupId)
        {
            var response = new GenericResponse<AssetStruct>();

            var assetStructs = _assetStructRepository.GetAssetStructsByGroupId(groupId);
            if (assetStructs == null)
            {
                _logger.Error($"{nameof(IAssetStructRepository.GetAssetStructsByGroupId)} failed. {nameof(groupId)}={groupId}.");
            }
            else
            {
                var liveToVodAssetStruct = assetStructs.FirstOrDefault(x => x.SystemName == LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME);
                response = liveToVodAssetStruct == null
                    ? new GenericResponse<AssetStruct>(eResponseStatus.AssetStructDoesNotExist)
                    : new GenericResponse<AssetStruct>(Status.Ok, liveToVodAssetStruct);
            }

            return response;
        }
    }
}