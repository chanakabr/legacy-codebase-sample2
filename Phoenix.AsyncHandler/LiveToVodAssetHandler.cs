using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Managers;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using Core.Catalog;
using LiveToVod;
using LiveToVod.BOL;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using SchemaRegistryEvents.Catalog;
using TVinciShared;
using Image = Core.Catalog.Image;

namespace Phoenix.AsyncHandler
{
    public class LiveToVodAssetHandler : CrudHandler<ProgramAsset>
    {
        private readonly ILiveToVodAssetManager _liveToVodAssetManager;
        private readonly ILiveToVodAssetCrudMessagePublisher _liveToVodPublisher;
        private readonly ILiveToVodImageService _liveToVodImageService;
        private readonly ILiveToVodAssetFileService _liveToVodAssetFileService;
        private readonly IProgramAssetCrudEventMapper _liveToVodAssetMapper;
        private readonly IAssetManager _assetManager;
        private readonly ILiveToVodManager _liveToVodManager;
        private readonly ILiveToVodPpvModuleParser _ppvModuleParser;
        private readonly ILogger<LiveToVodAssetHandler> _logger;

        public LiveToVodAssetHandler(
            ILiveToVodAssetManager liveToVodAssetManager,
            ILiveToVodAssetCrudMessagePublisher liveToVodPublisher,
            ILiveToVodImageService liveToVodImageService,
            ILiveToVodAssetFileService liveToVodAssetFileService,
            IProgramAssetCrudEventMapper liveToVodAssetMapper,
            IAssetManager assetManager,
            ILiveToVodManager liveToVodManager,
            ILiveToVodPpvModuleParser ppvModuleParser,
            ILogger<LiveToVodAssetHandler> logger)
        {
            _liveToVodAssetManager = liveToVodAssetManager;
            _liveToVodPublisher = liveToVodPublisher;
            _liveToVodImageService = liveToVodImageService;
            _liveToVodAssetFileService = liveToVodAssetFileService;
            _liveToVodAssetMapper = liveToVodAssetMapper;
            _assetManager = assetManager;
            _liveToVodManager = liveToVodManager;
            _ppvModuleParser = ppvModuleParser;
            _logger = logger;
        }

        protected override long GetOperation(ProgramAsset value) => value.Operation;

        protected override HandleResult Create(ConsumeResult<string, ProgramAsset> consumeResult)
        {
            var programAsset = consumeResult.GetValue();
            var partnerConfiguration = _liveToVodManager.GetCachedFullConfiguration(programAsset.PartnerId);
            var linearConfiguration = partnerConfiguration?.LinearAssets
                .FirstOrDefault(x => x.LinearAssetId == programAsset.LinearAssetId);
            if (linearConfiguration?.IsLiveToVodEnabled != true)
            {
                _logger.LogDebug("L2v configuration is disabled on {partnerId} for {linearChannelId}",
                    programAsset.PartnerId,
                    programAsset.LinearAssetId);

                return Result.Ok;
            }
            
            if (!ContainsLiveToVodMeta(programAsset, partnerConfiguration.MetadataClassifier))
            {
                _logger.LogDebug("l2v meta is missed on AssetId:[{assetId}] PartnerId:[{partnerId}]",
                    programAsset.Id,
                    programAsset.PartnerId);
                
                return Result.Ok;
            }

            var liveAsset = GetAsset<LiveAsset>(programAsset.PartnerId, programAsset.LinearAssetId, programAsset.Id);
            if (liveAsset == null)
            {
                return Result.Ok;
            }

            var catchupBufferInSeconds = liveAsset.SummedCatchUpBuffer * 60;
            var l2vCatalogStartDate = programAsset.StartDate + catchupBufferInSeconds;
            var currentDateTime = DateTime.UtcNow.ToUtcUnixTimestampSeconds();
            if (currentDateTime > l2vCatalogStartDate)
            {
                _logger.LogDebug("EPG program is not available in catchup buffer anymore:[{assetId}] PartnerId:[{partnerId}]",
                    programAsset.Id,
                    programAsset.PartnerId);

                return Result.Ok;
            }

            return HandleCreationFlow(programAsset, liveAsset, linearConfiguration);
        }
        
        protected override HandleResult Update(ConsumeResult<string, ProgramAsset> consumeResult)
        {
            var programAsset = consumeResult.GetValue();
            var mediaIdResponse = _liveToVodAssetManager.GetMediaIdByEpgId(programAsset.Id);
            if (!mediaIdResponse.IsOkStatusCode())
            {
                return Result.Ok;
            }
            
            return !mediaIdResponse.Object.HasValue
                ? Create(consumeResult)
                : HandleUpdateFlow(programAsset, mediaIdResponse.Object.Value);
        }
        
        protected override HandleResult Delete(ConsumeResult<string, ProgramAsset> consumeResult)
        {
            var epgAsset = consumeResult.GetValue();
            var mediaIdResponse = _liveToVodAssetManager.GetMediaIdByEpgId(epgAsset.Id);
            if (!mediaIdResponse.IsOkStatusCode())
            {
                return Result.Ok;
            }

            return !mediaIdResponse.Object.HasValue
                ? Result.Ok
                : HandleDeletionFlow(epgAsset, mediaIdResponse.Object.Value);
        }

        private HandleResult HandleCreationFlow(ProgramAsset programAsset, LiveAsset liveAsset, LiveToVodLinearAssetConfiguration configuration)
        {
            var assetToAdd = _liveToVodAssetMapper.MapToAssetForAdd(programAsset, liveAsset, configuration.RetentionPeriodDays.Value);
            var response = _liveToVodAssetManager.AddLiveToVodAsset((int)programAsset.PartnerId, assetToAdd, programAsset.UpdaterId);
            if (!response.IsOkStatusCode())
            {
                return Result.Ok;
            }

            var liveToVodAsset = response.Object;
            // images
            var images = programAsset.Images?.Select(_liveToVodAssetMapper.Map) ?? Enumerable.Empty<Image>();
            _liveToVodImageService.AddImages(programAsset.PartnerId, images, liveToVodAsset.Id, programAsset.UpdaterId);
            // asset files
            var assetFiles = _liveToVodAssetFileService.AddAssetFiles(programAsset.PartnerId, liveToVodAsset.Id, liveAsset.Id, programAsset.UpdaterId);
            var ppv = _ppvModuleParser.GetParsedPpv(programAsset);
            _liveToVodAssetFileService.AssignPpvOnAssetCreated(programAsset.PartnerId, liveToVodAsset.Id, assetFiles, ppv);

            _liveToVodPublisher.Publish(
                programAsset.PartnerId,
                liveToVodAsset,
                assetFiles.Select(x => x.Url),
                CrudOperationType.CREATE_OPERATION,
                programAsset.UpdaterId);

            return Result.Ok;
        }
        
        private HandleResult HandleUpdateFlow(ProgramAsset programAsset, long mediaId)
        {
            var partnerConfiguration = _liveToVodManager.GetCachedFullConfiguration(programAsset.PartnerId);
            if (!ContainsLiveToVodMeta(programAsset, partnerConfiguration.MetadataClassifier))
            {
                return HandleDeletionFlow(programAsset, mediaId);
            }

            var assets = _assetManager.GetAssets(
                programAsset.PartnerId,
                new List<KeyValuePair<eAssetTypes, long>>
                {
                    new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, mediaId),
                    new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, programAsset.LinearAssetId)
                }, true);

            var liveAsset = assets?.FirstOrDefault(x => x.Id == programAsset.LinearAssetId) as LiveAsset;
            var currentLiveToVodAsset = assets?.FirstOrDefault(x => x.Id == mediaId) as LiveToVodAsset;
            if (liveAsset == null || currentLiveToVodAsset == null)
            {
                return Result.Ok;
            }

            var linearConfiguration = partnerConfiguration.LinearAssets.First(x => x.LinearAssetId == liveAsset.Id);
            var assetToUpdate = _liveToVodAssetMapper.MapToAssetForUpdate(programAsset, liveAsset, currentLiveToVodAsset,  linearConfiguration.RetentionPeriodDays.Value);
            var response = _liveToVodAssetManager.UpdateLiveToVodAsset(
                programAsset.PartnerId,
                mediaId,
                assetToUpdate,
                programAsset.UpdaterId);
            if (!response.IsOkStatusCode())
            {
                return Result.Ok;
            }

            var liveToVodAsset = response.Object;
            // images
            var images = programAsset.Images?.Select(_liveToVodAssetMapper.Map) ?? Enumerable.Empty<Image>();
            _liveToVodImageService.UpdateImages(programAsset.PartnerId, images, liveToVodAsset.Id, programAsset.UpdaterId);
            // asset files
            var assetFiles = _liveToVodAssetFileService.UpdateAssetFiles(programAsset.PartnerId, liveAsset.Id, liveToVodAsset, programAsset.UpdaterId);
            var ppv = _ppvModuleParser.GetParsedPpv(programAsset);
            _liveToVodAssetFileService.AssignPpvOnAssetUpdated(programAsset.PartnerId, liveToVodAsset.Id, assetFiles, ppv);
            
            _liveToVodPublisher.Publish(
                programAsset.PartnerId,
                liveToVodAsset,
                assetFiles.Select(x => x.Url),
                CrudOperationType.UPDATE_OPERATION,
                programAsset.UpdaterId);

            return Result.Ok;
        }
        
        private HandleResult HandleDeletionFlow(ProgramAsset epgAsset, long mediaId)
        {
            var liveToVodAsset = GetAsset<LiveToVodAsset>(epgAsset.PartnerId, mediaId, epgAsset.Id);
            if (liveToVodAsset == null)
            {
                return Result.Ok;
            }

            if (DateTime.UtcNow > liveToVodAsset.CatalogStartDate)
            {
                _logger.LogDebug("it's not allowed to remove l2v asset, it's already visible for end clients: AssetId:[{assetId}] PartnerId:[{partnerId}]",
                    liveToVodAsset.Id,
                    epgAsset.PartnerId);

                return Result.Ok;
            }
            
            var result = _assetManager.DeleteAsset((int)epgAsset.PartnerId, mediaId, eAssetTypes.MEDIA, epgAsset.UpdaterId);
            if (!result.IsOkStatusCode())
            {
                _logger.LogError("failed to delete l2v media. groupId:[{GroupId}]. mediaId:[{mediaId}]. epgId:[{EpgId}]",
                    epgAsset.PartnerId,
                    mediaId,
                    epgAsset.Id);
            }

            _liveToVodPublisher.Publish(
                epgAsset.PartnerId,
                liveToVodAsset,
                liveToVodAsset.Files.Select(x => x.Url),
                CrudOperationType.DELETE_OPERATION,
                epgAsset.UpdaterId);

            return Result.Ok;
        }
        
        private T GetAsset<T>(long partnerId, long assetId, long epgId) where T : MediaAsset
        {
            var asset = _assetManager.GetAsset((int)partnerId, assetId, eAssetTypes.MEDIA, true);
            if (!asset.IsOkStatusCode())
            {
                return null;
            }

            if (asset.Object is T currentAsset)
            {
                return currentAsset;
            }

            _logger.LogError("asset is expected to be {type}. groupId:[{partnerId}]. assetId:[{assetId}]. epgId:[{epgId}]",
                nameof(T),
                partnerId,
                assetId,
                epgId);

            return null;
        }

        private static bool ContainsLiveToVodMeta(ProgramAsset programAsset, string metadataClassifier)
        {
            var liveToVodMeta = programAsset.Metas?.FirstOrDefault(x =>
                x.Name.Equals(metadataClassifier, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(liveToVodMeta?.Value))
            {
                return false;
            }

            const int liveToVodEnabledFlag = 1;

            return (bool.TryParse(liveToVodMeta.Value, out var isLiveToVodFlow) && isLiveToVodFlow)
                || (int.TryParse(liveToVodMeta.Value, out var liveToVodFlag) && liveToVodFlag == liveToVodEnabledFlag);
        }
    }
}