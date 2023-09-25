using System;
using System.Threading.Tasks;
using ApiObjects.Catalog;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.IndexRecording;
using Phoenix.Generated.Api.Events.Logical.mediaMark;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Catalog;

namespace Phoenix.AsyncHandler.Bookmark
{
    public class MediaMarkHandler : IKafkaMessageHandler<MediaMark>
    {
        private readonly ILogger<MediaMarkHandler> _logger;

        public MediaMarkHandler(ILogger<MediaMarkHandler> logger)
        {
            _logger = logger;
        }
        
        public Task<HandleResult> Handle(ConsumeResult<string, MediaMark> consumeResult)
        {
            var mediaMark = consumeResult.GetValue();
            if (mediaMark == null) return Task.FromResult(Result.Ok);
            if (!Enum.TryParse<KalturaAssetType>(mediaMark.AssetType, out var assetType))
            {
                _logger.LogError($"Bookmark action for {mediaMark.PartnerId} {mediaMark.DomainId} {mediaMark.Udid} {mediaMark.AssetId} has invalid AssetType {mediaMark.AssetType}");
                return Task.FromResult(Result.Ok);
            }
            
            try
            {
                var action = mediaMark.Action;
                //handle timeout as stop
                if (mediaMark.Action.ToUpper() == "DEVICE_TIMEOUT")
                    action = MediaPlayActions.STOP.ToString();
                
                var bookmarkResult = ClientsManager.CatalogClient().AddBookmark((int) mediaMark.PartnerId, mediaMark.UserId,
                    (int) mediaMark.DomainId, mediaMark.Udid, mediaMark.AssetId, assetType,
                    mediaMark.FileId ?? 0,
                    (int)(mediaMark.Position ?? 0), action, 0,
                    0,0, mediaMark.ProgramId,
                    true, mediaMark.Ip);
                
                _logger.LogTrace($"Bookmark action for {mediaMark.PartnerId} {mediaMark.DomainId} {mediaMark.Udid} {mediaMark.AssetId} proceed successfully - {bookmarkResult}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Bookmark action for {mediaMark.PartnerId} {mediaMark.DomainId} {mediaMark.Udid} {mediaMark.AssetId} thrown exception {e}");
            }

            return Task.FromResult(Result.Ok);
        }
    }
}