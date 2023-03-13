using System;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;
using SchemaRegistryEvents.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class MediaAssetCrudMessageService
    {
        private static readonly Lazy<MediaAssetCrudMessageService> LazyInstance = new Lazy<MediaAssetCrudMessageService>(
            () => new MediaAssetCrudMessageService(
                LiveToVodAssetCrudMessagePublisher.Instance,
                AssetManager.Instance,
                new KLogger(nameof(MediaAssetCrudMessageService))),
            LazyThreadSafetyMode.PublicationOnly);

        public static MediaAssetCrudMessageService Instance => LazyInstance.Value;

        private readonly ILiveToVodAssetCrudMessagePublisher _publisher;
        private readonly IAssetManager _assetManager;
        private readonly ILogger _logger;

        public MediaAssetCrudMessageService(ILiveToVodAssetCrudMessagePublisher publisher, IAssetManager assetManager, ILogger logger)
        {
            _publisher = publisher;
            _assetManager = assetManager;
            _logger = logger;
        }

        public void PublishKafkaDeleteEvent(long partnerId, MediaAsset asset, long updaterId)
        {
            if (asset != null && asset is LiveToVodAsset liveToVodAsset)
            {
                _publisher.Publish(
                    partnerId,
                    liveToVodAsset,
                    liveToVodAsset.Files.Select(x => x.Url).ToArray(),
                    CrudOperationType.DELETE_OPERATION,
                    updaterId);
            }
        }

        public void PublishKafkaUpdateEvent(long partnerId, long assetId, long updaterId)
        {
            var assetResponse = _assetManager.GetAsset((int)partnerId, assetId, eAssetTypes.MEDIA, true);
            if (!assetResponse.IsOkStatusCode())
            {
                _logger.LogError($"Failed to publish update event. PartnerId: {partnerId}, AssetID: {assetId}. Status: {assetResponse.Status}");
                return;
            }

            if (assetResponse.Object is LiveToVodAsset liveToVodAsset)
            {
                _publisher.Publish(
                    partnerId,
                    liveToVodAsset,
                    liveToVodAsset.Files.Select(x => x.Url).ToArray(),
                    CrudOperationType.UPDATE_OPERATION,
                    updaterId);
            }
        }
    }
}