using System;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using DAL.DTO;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public class LiveToVodAssetManager : ILiveToVodAssetManager
    {
        private static readonly Lazy<ILiveToVodAssetManager> Lazy = new Lazy<ILiveToVodAssetManager>(
            () => new LiveToVodAssetManager(
                LiveToVodAssetRepository.Instance,
                AssetManager.Instance,
                IndexManagerFactory.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly IKLogger Logger = new KLogger(nameof(LiveToVodAssetManager));

        public static ILiveToVodAssetManager Instance => Lazy.Value;

        private readonly IAssetManager _assetManager;
        private readonly ILiveToVodAssetRepository _liveToVodAssetRepository;
        private readonly IIndexManagerFactory _indexManagerFactory;

        public LiveToVodAssetManager(
            ILiveToVodAssetRepository liveToVodAssetRepository,
            IAssetManager assetManager,
            IIndexManagerFactory indexManagerFactory)
        {
            _liveToVodAssetRepository = liveToVodAssetRepository;
            _assetManager = assetManager;
            _indexManagerFactory = indexManagerFactory;
        }

        public GenericResponse<LiveToVodAsset> AddLiveToVodAsset(long partnerId, LiveToVodAsset assetToAdd, long updaterId)
        {
            // isFromIngest = true to not update ES index right now and to do it after l2v data will be inserted.
            var mediaAssetResponse = _assetManager.AddAsset((int)partnerId, assetToAdd, updaterId, isFromIngest: true);
            if (!mediaAssetResponse.IsOkStatusCode())
            {
                return new GenericResponse<LiveToVodAsset>(mediaAssetResponse.Status);
            }

            var assetId = mediaAssetResponse.Object.Id;
            var liveToVodAssetDto = MapToDto(assetToAdd, assetId, updaterId);
            var liveToVodAssetId = _liveToVodAssetRepository.InsertLiveToVodAsset(liveToVodAssetDto);

            if (!liveToVodAssetId.HasValue)
            {
                Logger.Error($"failed to create live to vod asset. partnerId:[{partnerId}]. epgId:[{assetToAdd.EpgId}]]");

                return new GenericResponse<LiveToVodAsset>();
            }

            var indexResult = _indexManagerFactory.GetIndexManager((int)partnerId).UpsertMedia(assetId);
            if (!indexResult)
            {
                Logger.ErrorFormat("Failed UpsertMedia index for assetId: {0}, partnerId: {1} after live to vod asset created", assetId, partnerId);
            }

            _assetManager.InvalidateAsset(mediaAssetResponse.Object.AssetType, (int)partnerId, assetId);

            return GetLiveToVodAsset(partnerId, assetId);
        }

        public GenericResponse<LiveToVodAsset> UpdateLiveToVodAsset(long partnerId, long assetId, LiveToVodAsset assetToUpdate, long updaterId)
        {
            // isFromIngest = true to not update ES index right now and to do it after l2v data will be updated.
            var mediaAssetResponse = _assetManager.UpdateAsset(
                (int)partnerId,
                assetId,
                assetToUpdate,
                updaterId,
                isFromIngest: true);
            if (!mediaAssetResponse.IsOkStatusCode())
            {
                return new GenericResponse<LiveToVodAsset>(mediaAssetResponse.Status);
            }

            var liveToVodAssetDto = MapToDto(assetToUpdate, assetId, updaterId);
            var isSuccessfulUpdate = _liveToVodAssetRepository.UpdateLiveToVodAsset(liveToVodAssetDto);

            if (!isSuccessfulUpdate)
            {
                Logger.Error($"failed to update live to vod asset. groupId:[{partnerId}]. assetId:[{assetId}].]");

                return new GenericResponse<LiveToVodAsset>();
            }

            var indexResult = _indexManagerFactory.GetIndexManager((int)partnerId).UpsertMedia(assetId);
            if (!indexResult)
            {
                Logger.ErrorFormat("Failed UpsertMedia index for assetId: {0}, partnerId: {1} after live to vod asset updated", assetId, partnerId);
            }

            _assetManager.InvalidateAsset(mediaAssetResponse.Object.AssetType, (int)partnerId, assetId);
            if (mediaAssetResponse.Object is MediaAsset mediaAsset
                && mediaAsset.IsActive == true
                && CatalogManager.Instance.TryGetCatalogGroupCacheFromCache((int)partnerId, out var cache))
            {
                Core.Notification.Module.AddFollowNotificationRequestForOpc((int)partnerId, mediaAsset, updaterId, cache);
            }

            return GetLiveToVodAsset(partnerId, assetId);
        }
        
        public GenericResponse<long?> GetMediaIdByEpgId(long epgId)
        {
            var result = _liveToVodAssetRepository.TryGetMediaIdByEpgId(epgId, out var mediaId);
            if (!result)
            {
                Logger.Error($"failed to get media Id by EpgId. epgId:[{epgId}]]");
                
                return new GenericResponse<long?>();
            }

            return new GenericResponse<long?>(Status.Ok, mediaId > 0 ? mediaId : (long?)null);
        }

        private GenericResponse<LiveToVodAsset> GetLiveToVodAsset(long partnerId, long assetId)
        {
            var assetResponse =  _assetManager.GetAsset((int)partnerId, assetId, eAssetTypes.MEDIA, true);
            if (!assetResponse.IsOkStatusCode())
            {
                return new GenericResponse<LiveToVodAsset>(assetResponse.Status);
            }

            if (assetResponse.Object is LiveToVodAsset liveToVodAsset)
            {
                return new GenericResponse<LiveToVodAsset>(Status.Ok, liveToVodAsset);
            }

            Logger.Error($"asset is not liveToVod. groupId: [partnerId]. assetId: [{assetResponse.Object.Id}]");

            return new GenericResponse<LiveToVodAsset>();
        }

        private static LiveToVodAssetDTO MapToDto(LiveToVodAsset source, long mediaId, long updaterId)
            => new LiveToVodAssetDTO
            {
                LinearAssetId = source.LinearAssetId,
                EpgId = source.EpgId,
                EpgIdentifier = source.EpgIdentifier,
                EpgChannelId = source.EpgChannelId,
                Crid = source.Crid,
                OriginalStartDate = source.OriginalStartDate,
                OriginalEndDate = source.OriginalEndDate,
                PaddingBeforeProgramStarts = source.PaddingBeforeProgramStarts,
                PaddingAfterProgramEnds = source.PaddingAfterProgramEnds,
                MediaId = mediaId,
                UpdaterId = updaterId
            };
    }
}