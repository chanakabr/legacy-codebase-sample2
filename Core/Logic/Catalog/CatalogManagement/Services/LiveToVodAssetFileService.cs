using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects;
using ApiObjects.Base;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using Force.DeepCloner;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodAssetFileService : ILiveToVodAssetFileService
    {
        private readonly IMediaFileTypeManager _fileManager;
        private readonly IPriceManager _priceManager;
        private readonly ILogger<LiveToVodAssetFileService> _logger;

        public LiveToVodAssetFileService(IMediaFileTypeManager fileManager, IPriceManager priceManager, ILogger<LiveToVodAssetFileService> logger)
        {
            _fileManager = fileManager;
            _priceManager = priceManager;
            _logger = logger;
        }

        public IEnumerable<AssetFile> AddAssetFiles(long partnerId, long liveToVodAssetId, long liveAssetId, long updaterId)
        {
            var result = new List<AssetFile>();
            var assetFilesToHandle = GetAssetFilesToHandle(partnerId, liveAssetId);
            foreach (var assetFileToAdd in assetFilesToHandle)
            {
                var addedAssetFile = AddAssetFile(partnerId, liveToVodAssetId, assetFileToAdd, updaterId);
                if (addedAssetFile != null)
                {
                    result.Add(addedAssetFile);
                }
            }

            if (result.Any())
            {
                _fileManager.InvalidateAssetAfterFilesUpdated((int)partnerId, liveToVodAssetId, updaterId);
            }

            return result;
        }

        public IEnumerable<AssetFile> UpdateAssetFiles(long partnerId, long liveAssetId, MediaAsset liveToVodAsset, long updaterId)
        {
            // only one media file per media file type ID is allowed.
            var assetFilesByTypeIdToHandle = liveToVodAsset.Files?.ToDictionary(x => x.TypeId.Value)
                ?? new Dictionary<int, AssetFile>();
            var result = new List<AssetFile>();
            var assetFilesToHandle = GetAssetFilesToHandle(partnerId, liveAssetId);
            foreach (var fileToHandle in assetFilesToHandle)
            {
                var processedAssetFile = assetFilesByTypeIdToHandle.TryGetValue(
                    fileToHandle.TypeId.Value,
                    out var currentAssetFile)
                    ? UpdateAssetFile(partnerId, liveToVodAsset.Id, fileToHandle, currentAssetFile.Id, updaterId)
                    : AddAssetFile(partnerId, liveToVodAsset.Id, fileToHandle, updaterId);

                if (processedAssetFile != null)
                {
                    result.Add(processedAssetFile);
                }
                else if (currentAssetFile != null)
                {
                    result.Add(currentAssetFile);
                }

                assetFilesByTypeIdToHandle.Remove(fileToHandle.TypeId.Value);
            }

            // remove not updated files.
            foreach (var fileToRemove in assetFilesByTypeIdToHandle.Values)
            {
                var status = _fileManager.DeleteMediaFile((int)partnerId, updaterId, fileToRemove.Id, isFromIngest: true);
                if (!status.IsOkStatusCode())
                {
                    // still assigned to asset, then should be added to output result
                    result.Add(fileToRemove);
                }
            }

            if (assetFilesToHandle.Any() || assetFilesByTypeIdToHandle.Any())
            {
                _fileManager.InvalidateAssetAfterFilesUpdated((int)partnerId, liveToVodAsset.Id, updaterId);
            }

            return result;
        }

        public void AssignPpvOnAssetCreated(long partnerId, long assetId, IEnumerable<AssetFile> assetFiles, IEnumerable<PpvModuleInfo> ppv)
        {
            var ppvToAssignByFileTypeId = ppv.ToLookup(x => x.FileTypeId);
            foreach (var assetFile in assetFiles)
            {
                if (!ppvToAssignByFileTypeId.Contains(assetFile.TypeId.Value))
                {
                    continue;
                }

                var currentPpv = Enumerable.Empty<AssetFilePpv>().ToList();
                var ppvToAssign = ppvToAssignByFileTypeId[assetFile.TypeId.Value];
                AssignPpvToAssetFile(partnerId, assetId, assetFile, ppvToAssign, currentPpv);
            }
        }
        
        public void AssignPpvOnAssetUpdated(
            long partnerId,
            long assetId,
            IEnumerable<AssetFile> assetFiles,
            IEnumerable<PpvModuleInfo> ppv)
        {
            var ppvModulesByFileId = GetPpvModules(partnerId, assetId);
            var ppvToAssignByFileTypeId = ppv.ToLookup(x => x.FileTypeId);
            foreach (var assetFile in assetFiles)
            {
                var ppvToAssign = ppvToAssignByFileTypeId.Contains(assetFile.TypeId.Value)
                    ? ppvToAssignByFileTypeId[assetFile.TypeId.Value]
                    : Enumerable.Empty<PpvModuleInfo>();
                var currentPpv = ppvModulesByFileId.Contains(assetFile.Id)
                    ? ppvModulesByFileId[assetFile.Id]
                    : Enumerable.Empty<AssetFilePpv>();
                AssignPpvToAssetFile(partnerId, assetId, assetFile, ppvToAssign, currentPpv);
            }
        }

        private IEnumerable<AssetFile> GetAssetFilesToHandle(long partnerId, long liveAssetId)
        {
            var liveAssetFilesResponse = _fileManager.GetMediaFiles((int)partnerId, default, liveAssetId);
            if (!liveAssetFilesResponse.IsOkStatusCode())
            {
                _logger.LogError("Failed to retrieve media files for {liveAssetId} asset (partnerId {partnerId}). Add/update asset files operation will be skipped", liveAssetId, partnerId);

                return Enumerable.Empty<AssetFile>();
            }

            var mediaFileTypesResponse = _fileManager.GetMediaFileTypes((int)partnerId);
            if (!mediaFileTypesResponse.IsOkStatusCode())
            {
                _logger.LogError("Failed to retrieve media file types for {partnerId}. Add/update asset files operation will be skipped", partnerId);

                return Enumerable.Empty<AssetFile>();
            }

            var mediaFileTypes = mediaFileTypesResponse.Objects.ToDictionary(x => x.Id);

            return liveAssetFilesResponse.Objects.Where(
                x => mediaFileTypes.TryGetValue(x.TypeId.Value, out var type) && type.StreamerType != StreamerType.multicast);
        }

        private AssetFile AddAssetFile(long partnerId, long assetId, AssetFile fileToAdd, long updaterId)
        {
            var fileToInsert = fileToAdd.DeepClone();
            fileToInsert.AssetId = assetId;
            var addFileResponse = _fileManager.InsertMediaFile((int)partnerId, updaterId, fileToInsert, isFromIngest: true);
            if (addFileResponse.IsOkStatusCode())
            {
                return addFileResponse.Object;
            }
            
            _logger.LogWarning("failed to add media file. groupId:[{PartnerId}]. assetId:[{EpgId}]. typeId:[{TypeId}]",
                partnerId,
                assetId,
                fileToAdd.TypeId);

            return null;
        }

        private AssetFile UpdateAssetFile(long partnerId, long liveToVodAssetId, AssetFile fileToUpdate, long currentAssetFileId, long updaterId)
        {
            var assetFileToUpdate = fileToUpdate.DeepClone();
            assetFileToUpdate.AssetId = liveToVodAssetId;
            assetFileToUpdate.Id = currentAssetFileId;
            var updateFileResponse = _fileManager.UpdateMediaFile((int)partnerId, assetFileToUpdate, updaterId, isFromIngest: true);
            if (updateFileResponse.IsOkStatusCode())
            {
                return updateFileResponse.Object;
            }
            
            _logger.LogError("failed to update media file. groupId:[{PartnerId}]. assetId:[{AssetId}]. fileId:[{FileId}]",
                partnerId,
                liveToVodAssetId,
                fileToUpdate.Id);
            
            return null;
        }

        private void AssignPpvToAssetFile(
            long partnerId,
            long assetId,
            AssetFile assetFile,
            IEnumerable<PpvModuleInfo> ppvToAssign,
            IEnumerable<AssetFilePpv> currentPpv)
        {
            // clean existing PPV if there is no PPV to assign
            if (!ppvToAssign.Any())
            {
                RemovePpvFromFile(partnerId, assetFile, currentPpv);

                return;
            }

            // remove PPV not specified in PPV meta if fileTypeId is matched.
            var ppvToDelete = currentPpv
                .Where(x => !ppvToAssign.Any(p => x.PpvModuleId == p.PpvModuleId));
            RemovePpvFromFile(partnerId, assetFile, ppvToDelete);

            // add or update PPV from PPV meta if fileTypeId is matched.
            foreach (var ppvModule in ppvToAssign)
            {
                var newPpv = new AssetFilePpv
                {
                    PpvModuleId = ppvModule.PpvModuleId,
                    AssetFileId = assetFile.Id,
                    StartDate = ppvModule.StartDate,
                    EndDate = ppvModule.EndDate
                };
                var contextData = new ContextData((int)partnerId);
                var previousPpv = currentPpv.FirstOrDefault(x => ppvModule.PpvModuleId == x.PpvModuleId);
                var ppvResult = previousPpv != null
                    ? _priceManager.UpdateAssetFilePPV(contextData, newPpv)
                    : _priceManager.AddAssetFilePPV(contextData, newPpv);
                if (!ppvResult.IsOkStatusCode())
                {
                    _logger.LogError("failed to add or update PPV from media file. groupId:[{PartnerId}]. ppvModuleId:[{EpgId}]. fileId:[{FileId}]. status:[{Status}].",
                        partnerId,
                        ppvModule.PpvModuleId,
                        assetFile.Id,
                        ppvResult.Status);
                    continue;
                }
                
                _fileManager.DoFreeItemIndexUpdateIfNeeded(
                    (int)partnerId,
                    (int)assetId,
                    previousPpv?.StartDate,
                    ppvResult.Object.StartDate,
                    previousPpv?.EndDate,
                    ppvResult.Object.EndDate);
            }
        }

        private void RemovePpvFromFile(long partnerId, AssetFile assetFile, IEnumerable<AssetFilePpv> ppvList)
        {
            var contextData = new ContextData((int)partnerId);
            foreach (var ppvToRemove in ppvList)
            {
                var deleteStatus = _priceManager.DeleteAssetFilePPV(contextData, assetFile.Id, ppvToRemove.PpvModuleId);
                if (!deleteStatus.IsOkStatusCode())
                {
                    _logger.LogWarning("failed to remove PPV from media file. groupId:[{PartnerId}]. ppvModuleId:[{EpgId}]. fileId:[{FileId}]. status:[{Status}].",
                        partnerId,
                        ppvToRemove.PpvModuleId,
                        assetFile.Id,
                        deleteStatus);
                }
            }
        }

        private ILookup<long, AssetFilePpv> GetPpvModules(long partnerId, long assetId)
        {
            var contextData = new ContextData((int)partnerId);
            var existingPpvModulesResponse = _priceManager.GetAssetFilePPVList(contextData, assetId, 0);

            return existingPpvModulesResponse.IsOkStatusCode()
                ? existingPpvModulesResponse.Objects.ToLookup(x => x.AssetFileId)
                : Enumerable.Empty<AssetFilePpv>().ToLookup(x => x.AssetFileId);
        }
    }
}