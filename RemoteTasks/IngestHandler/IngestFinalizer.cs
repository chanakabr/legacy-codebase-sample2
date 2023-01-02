using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using IngestHandler.Common;
using Phx.Lib.Log;
using TVinciShared;

namespace IngestHandler
{
    public class IngestFinalizer : IIngestFinalizer
    {
        private static readonly KLogger Logger = new KLogger(nameof(IngestFinalizer));

        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private readonly EpgNotificationManager _notificationManager;
        private readonly IProgramAssetCrudMessageService _crudMessageService;
        private Events.eEvent _kmonEvt = Events.eEvent.EVENT_WS;
        private string _partnerIdStr;
        private string _bulkUploadId;

        public IngestFinalizer(IIndexManagerFactory indexManagerFactory, IEpgIngestMessaging epgIngestMessaging, EpgNotificationManager notificationManager, IProgramAssetCrudMessageService crudMessageService)
        {
            _indexManagerFactory = indexManagerFactory;
            _epgIngestMessaging = epgIngestMessaging;
            _notificationManager = notificationManager;
            _crudMessageService = crudMessageService;
        }

        public async Task FinalizeEpgV3Ingest(int partnerId, CRUDOperations<EpgProgramBulkUploadObject> crudOps, BulkUpload bulkUpload)
        {
            _partnerIdStr = partnerId.ToString();
            _bulkUploadId = bulkUpload.Id.ToString();
            if (BulkUpload.IsProcessCompletedByStatus(bulkUpload.Status))
            {
                SendIngestV3PartCompletedEvent(bulkUpload);
                await FinalizeEpgCommon(bulkUpload, crudOps, bulkUpload.Status);
            }
        }
        
        public async Task FinalizeEpgV2Ingest(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
        {
            _partnerIdStr = bulkUpload.GroupId.ToString();
            _bulkUploadId = bulkUpload.Id.ToString();
            try
            {
                Logger.Debug($"Starting IngestFinalizer for epg v2 BulkUploadId:[{bulkUpload.Id}]");

                var indexManager = _indexManagerFactory.GetIndexManager(serviceEvent.GroupId);
                var isRefreshSuccess = indexManager.ForceRefreshEpgIndex(serviceEvent.TargetIndexName);

                if (!isRefreshSuccess)
                {
                    Logger.Error($"BulkUploadId [{bulkUpload.Id}], targetIndexName:[{serviceEvent.TargetIndexName}] > index refresh failed");
                }

                var newBulkUploadStatus = SetOkayStatusToResults(bulkUpload, relevantResults);

                // Need to refresh the data from the bulk upload object after updating with the validation result
                bulkUpload = BulkUploadMethods.GetBulkUploadData(bulkUpload.GroupId, bulkUpload.Id);

                // All separated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the changes.
                if (BulkUpload.IsProcessCompletedByStatus(newBulkUploadStatus))
                {
                    var bulkUploadJobData = bulkUpload.JobData as BulkUploadIngestJobData;
                    Logger.Debug($"BulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}], Final part of bulk is marked, status is: [{newBulkUploadStatus}], finalizing bulk object");
                    bool finalizeResult = indexManager.FinalizeEpgV2Indices(bulkUploadJobData.DatesOfProgramsToIngest.ToList());

                    if (!finalizeResult)
                    {
                        Logger.Error($"BulkUploadId [{bulkUpload.Id}], targetIndexName:[{serviceEvent.TargetIndexName}]] > index set refresh to -1 failed ]");
                        throw new Exception("Could not set index refresh interval");
                    }

                    await FinalizeEpgCommon(bulkUpload, serviceEvent.CrudOperations, newBulkUploadStatus);
                }

                SendIngestV2PartCompletedEvent(bulkUpload, relevantResults);

                Logger.Info($"BulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}]] > Ingest Validation for part of the bulk is completed, status:[{newBulkUploadStatus}]");
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}]", ex);
                    bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest validation, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(bulkUpload, BulkUploadJobStatus.Fatal);
                    if (result.IsOkStatusCode()) TrySendIngestCompleted(bulkUpload, BulkUploadJobStatus.Fatal);
                    Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler BulkUploadId:[{bulkUpload.Id}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    Logger.Error($"Error while trying to update bulk upload with failed status from ingestValidation, trying one last time...", innerEx);
                }

                throw;
            }
        }

        /// <summary>
        /// This part of finalization is common for v2 and v3 and is called by both
        /// </summary>
        private async Task FinalizeEpgCommon(BulkUpload bulkUpload, CRUDOperations<EpgProgramBulkUploadObject> crudOps, BulkUploadJobStatus newBulkUploadStatus)
        {
            using var km = CreateKMonitor("FinalizeEpgCommon-Total");
            var bulkUploadJobData = bulkUpload.JobData as BulkUploadIngestJobData;
            Logger.Debug($"BulkUploadId: [{bulkUpload.Id}] : Final part of bulk is marked, status is: [{newBulkUploadStatus}], finalizing bulk object");
            InvalidateEpgAssets(bulkUpload);
            var bulkUploadResultsDictionaries = bulkUpload.ConstructResultsDictionary();
            var operations = CalculateOperations(crudOps, bulkUploadResultsDictionaries);
            UpdateRecordings(bulkUpload, operations);
            await PublishProgramAssetMessages(bulkUpload.GroupId, operations, bulkUpload.UpdaterId);

            var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(bulkUpload, newBulkUploadStatus);
            if (result.IsOkStatusCode()) { TrySendIngestCompleted(bulkUpload, newBulkUploadStatus); }

            Logger.Info($"BulkUploadId: [{bulkUpload.Id}] > Ingest Validation for entire bulk is completed, status:[{newBulkUploadStatus}]");
            if (newBulkUploadStatus == BulkUploadJobStatus.Success)
            {
                NotifyChannelUpdated(bulkUpload, bulkUploadJobData, bulkUploadResultsDictionaries);
            }
        }

        private void NotifyChannelUpdated(BulkUpload bulkUpload, BulkUploadIngestJobData bulkUploadJobData, BulkUploadResultsDictionary bulkUploadResultsDictionaries)
        {
            using var km = CreateKMonitor("NotifyChannelUpdated");
            // DatesOfProgramsToIngest are ordered ascending [min..max]. 
            // DatesOfProgramsToIngest - these are dates for ALL channels
            // and potentially specific channel could have no updates on some dates,
            // but we still notify that it was changed in date range [min..max]. could be better.
            foreach ((var epgChannelId, var externalIdToProgram) in bulkUploadResultsDictionaries)
            {
                var minDate = bulkUploadResultsDictionaries[epgChannelId].Values.Min(p => p.StartDate).StartOfDay();
                var maxDate = bulkUploadResultsDictionaries[epgChannelId].Values.Max(p => p.EndDate).EndOfDay();
                var linearAssetId = externalIdToProgram.Values.First().LiveAssetId;

                _notificationManager.ChannelWasUpdated(
                    KLogger.GetRequestId(),
                    bulkUpload.GroupId,
                    bulkUpload.UpdaterId,
                    linearAssetId,
                    epgChannelId,
                    minDate,
                    maxDate,
                    bulkUploadJobData.DisableEpgNotification);
            }
        }

        private BulkUploadJobStatus SetOkayStatusToResults(BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
        {
            var bulkUploadResultsOfCurrentDate = relevantResults.SelectMany(channel => channel.Value).Select(prog => prog.Value).ToList();
            bulkUploadResultsOfCurrentDate.ForEach(r => r.Status = BulkUploadResultStatus.Ok);

            BulkUploadManager.UpdateBulkUploadResults(bulkUploadResultsOfCurrentDate, out BulkUploadJobStatus newStatus);
            Logger.Info($"updated result bulkUploadId: [{bulkUpload.Id}], countOfUpdates:[{bulkUploadResultsOfCurrentDate.Count}], results updated in CB, calculated status [{newStatus}]");
            return newStatus;
        }

        private void InvalidateEpgAssets(BulkUpload bulkUpload)
        {
            using var km = CreateKMonitor("InvalidateEpgAssets");
            var affectedProgramIds = bulkUpload.AffectedObjects?.Select(p => (long)p.ObjectId).ToArray();
            var ingestedProgramIds = bulkUpload.Results.Where(r => r.ObjectId.HasValue).Select(r => r.ObjectId.Value) ?? new List<long>();
            var programIdsToInvalidate = affectedProgramIds?.Any() == true
                ? ingestedProgramIds.Concat(affectedProgramIds)
                : ingestedProgramIds;

            var isOPC = GroupSettingsManager.Instance.IsOpc(bulkUpload.GroupId);
            foreach (var progId in programIdsToInvalidate)
            {
                string invalidationKey = isOPC
                    ? LayeredCacheKeys.GetAssetInvalidationKey(bulkUpload.GroupId, eAssetTypes.EPG.ToString(), progId)
                    : LayeredCacheKeys.GetEpgInvalidationKey(bulkUpload.GroupId, progId);

                var invalidationResult = LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                if (!invalidationResult)
                {
                    Logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", progId, eAssetType.PROGRAM.ToString(), invalidationKey);
                }

                Logger.Debug($"SetInvalidationKey: [{invalidationKey}] done with result: [{invalidationResult}]");
            }
        }

        // Logically hard part, which could have errors, should be tested
        // Logical trick: DeletedObjects - it's all EPGs, which should be removed from Elastic. Even just updated EPGs are added to DeletedObjects
        // Probably this calculation should be in BulkUploadTransformationHandler
        private Operations CalculateOperations(CRUDOperations<EpgProgramBulkUploadObject> crudOps, BulkUploadResultsDictionary result)
        {
            // _bulkUpload.AddedObjects don't have EpgIds, because they were added in BulkUploadTransformationHandler, before inserting to DB
            // so we take EpgIds by EpgExternalIds
            var addedEpgIds = crudOps.ItemsToAdd.Where(_ => !_.IsAutoFill).Select(o => result[o.ChannelId][o.EpgExternalId].ObjectId.Value).ToArray();

            // Formulas:
            // AddedObjects = really-added
            // really-updated = AffectedObjects ∪ UpdatedObjects
            // DeletedObjects = really-deleted ∪ really-updated
            var deletedObjects = crudOps.ItemsToDelete.Where(_ => !_.IsAutoFill);
            var updatedObjects = crudOps.ItemsToUpdate.Where(_ => !_.IsAutoFill).Select(_ => (long)_.EpgId);
            var affectedObjects = crudOps.AffectedItems.Where(_ => !_.IsAutoFill).Select(_ => (long)_.EpgId);

            var reallyUpdated = affectedObjects.Union(updatedObjects).ToArray();
            var reallyDeleted = deletedObjects
                .Where(x => reallyUpdated.All(u => (long)x.EpgId != u))
                .ToArray();

            Logger.Info($"reallyUpdated: {string.Join(",", reallyUpdated)}");
            Logger.Info($"reallyDeleted: {string.Join(",", reallyDeleted.Select(x => (long)x.EpgId))}");

            return new Operations
            {
                AddedEpgIds = addedEpgIds,
                UpdatedEpgIds = reallyUpdated,
                DeletedEpgs = reallyDeleted
            };
        }

        private void UpdateRecordings(BulkUpload bulkUpload, Operations operations)
        {
            using var km = CreateKMonitor("UpdateRecordings");
            UpdateRecordings(bulkUpload, operations.DeletedEpgs.Select(x => (long)x.EpgId).ToArray(), eAction.Delete);
            UpdateRecordings(bulkUpload, operations.AddedEpgIds, eAction.On);
            UpdateRecordings(bulkUpload, operations.UpdatedEpgIds, eAction.Update);
        }

        private void UpdateRecordings(BulkUpload bulkUpload, long[] epgIds, eAction action)
        {
            if (epgIds.Length == 0) return;

            var logSuffix = $"bulkUploadId:[{bulkUpload.Id}] action:[{action}] EPGids:[{string.Join(',', epgIds)}]";
            Logger.Info($"Try to update recordings. {logSuffix}");
            try
            {
                Core.ConditionalAccess.Module.IngestRecording(bulkUpload.GroupId, epgIds, action);
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to update recordings. {logSuffix}", exception);
            }
        }

        private class Operations
        {
            public long[] AddedEpgIds;
            public long[] UpdatedEpgIds;
            public EpgProgramBulkUploadObject[] DeletedEpgs;
        }

        private void TrySendIngestCompleted(BulkUpload bulkUpload, BulkUploadJobStatus newStatus)
        {
            using var km = CreateKMonitor("TrySendIngestCompleted");
            if (!BulkUpload.IsProcessCompletedByStatus(newStatus)) return;

            var updateDate = DateTime.UtcNow; // TODO looks like _bulUpload.UpdateDate is not updated in CB
            var parameters = new EpgIngestCompletedParameters
            {
                GroupId = bulkUpload.GroupId,
                BulkUploadId = bulkUpload.Id,
                Status = newStatus,
                Errors = bulkUpload.Errors,
                CompletedDate = updateDate,
                UserId = bulkUpload.UpdaterId,
                Results = bulkUpload.Results
            };

            _epgIngestMessaging.EpgIngestCompleted(parameters);
        }

        private void SendIngestV2PartCompletedEvent(BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
        {
            var programIngestResults = relevantResults.Values
                .SelectMany(x => x.Values)
                .ToArray();
            var hasMoreToIngest = !BulkUpload.IsProcessCompletedByStatus(bulkUpload.Status);
            var parameters = new EpgIngestPartCompletedParameters
            {
                BulkUploadId = bulkUpload.Id,
                GroupId = bulkUpload.GroupId,
                HasMoreEpgToIngest = hasMoreToIngest,
                UserId = bulkUpload.UpdaterId,
                Results = programIngestResults
            };

            _epgIngestMessaging.EpgIngestPartCompleted(parameters);
        }

        private void SendIngestV3PartCompletedEvent(BulkUpload bulkUpload)
        {
            var programIngestResults = bulkUpload.Results
                .Cast<BulkUploadProgramAssetResult>()
                .GroupBy(x => (x.LiveAssetId, x.StartDate.Date))
                .Select(x => new EpgIngestPartCompletedParameters
                {
                    BulkUploadId = bulkUpload.Id,
                    GroupId = bulkUpload.GroupId,
                    HasMoreEpgToIngest = true,
                    UserId = bulkUpload.UpdaterId,
                    Results = x
                })
                .ToList();

            if (programIngestResults.Any()) { programIngestResults.Last().HasMoreEpgToIngest = false; }

            _epgIngestMessaging.EpgIngestPartCompleted(programIngestResults);
        }

        private Task PublishProgramAssetMessages(long groupId, Operations operations, long updaterId)
        {
            using var km = CreateKMonitor("PublishProgramAssetMessages");
            var createdEventsTask = _crudMessageService.PublishCreateEventsAsync(groupId, operations.AddedEpgIds, updaterId);
            var updatedEventsTask = _crudMessageService.PublishUpdateEventsAsync(groupId, operations.UpdatedEpgIds, updaterId);
            var deletedEventsTask = _crudMessageService.PublishDeleteEventsAsync(groupId, operations.DeletedEpgs, updaterId);

            return Task.WhenAll(createdEventsTask, updatedEventsTask, deletedEventsTask);
        }

        private KMonitor CreateKMonitor(string name, params string[] args)
        {
            var argsStr = args?.Any() == true ? $"-{string.Join("-", args)}" : "";
            return new KMonitor(_kmonEvt, _partnerIdStr, $"ingest-profiler-{name}-{_partnerIdStr}-{_bulkUploadId}{argsStr}");
        }
    }
}