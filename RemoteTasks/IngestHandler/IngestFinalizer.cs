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

        public IngestFinalizer(IIndexManagerFactory indexManagerFactory, IEpgIngestMessaging epgIngestMessaging, EpgNotificationManager notificationManager, IProgramAssetCrudMessageService crudMessageService)
        {
            _indexManagerFactory = indexManagerFactory;
            _epgIngestMessaging = epgIngestMessaging;
            _notificationManager = notificationManager;
            _crudMessageService = crudMessageService;
        }

        public async Task FinalizeEpgIngest(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
        {
            try
            {
                Logger.Debug($"Starting IngestFinalizer BulkUploadId:[{bulkUpload.Id}]");

                var indexManager = _indexManagerFactory.GetIndexManager(serviceEvent.GroupId);
                var isRefreshSuccess = indexManager.ForceRefreshEpgV2Index(serviceEvent.TargetIndexName);

                if (!isRefreshSuccess)
                {
                    Logger.Error($"BulkUploadId [{bulkUpload.Id}], targetIndexName:[{serviceEvent.TargetIndexName}] > index refresh failed");
                }

                var newStatus = SetOkayStatusToAllResults(serviceEvent, bulkUpload, relevantResults);

                // Need to refresh the data from the bulk upload object after updating with the validation result
                bulkUpload = BulkUploadMethods.GetBulkUploadData(bulkUpload.GroupId, bulkUpload.Id);

                // All separated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the changes.
                if (BulkUpload.IsProcessCompletedByStatus(newStatus))
                {
                    var bulkUploadJobData = bulkUpload.JobData as BulkUploadIngestJobData;
                    Logger.Debug($"BulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}], Final part of bulk is marked, status is: [{newStatus}], finalizing bulk object");
                    bool finalizeResult = indexManager.FinalizeEpgV2Indices(bulkUploadJobData.DatesOfProgramsToIngest.ToList());

                    if (!finalizeResult)
                    {
                        Logger.Error($"BulkUploadId [{bulkUpload.Id}], targetIndexName:[{serviceEvent.TargetIndexName}]] > index set refresh to -1 failed ]");
                        throw new Exception("Could not set index refresh interval");
                    }

                    InvalidateEpgAssets(bulkUpload);
                    BulkUploadResultsDictionary bulkUploadResultsDictionaries = bulkUpload.ConstructResultsDictionary();
                    var operations = CalculateOperations(bulkUpload, bulkUploadResultsDictionaries);
                    UpdateRecordings(bulkUpload, operations);
                    await PublishProgramAssetMessages(bulkUpload.GroupId, operations);

                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(bulkUpload, newStatus);
                    if (result.IsOkStatusCode()) TrySendIngestCompleted(bulkUpload, newStatus);
                    Logger.Info($"BulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}]] > Ingest Validation for entire bulk is completed, status:[{newStatus}]");
                    if (newStatus == BulkUploadJobStatus.Success)
                    {
                        // DatesOfProgramsToIngest are ordered ascending [min..max]. 
                        // DatesOfProgramsToIngest - these are dates for ALL channels
                        // and potentially specific channel could have no updates on some dates,
                        // but we still notify that it was changed in date range [min..max]. could be better.
                        var minDate = bulkUploadJobData.DatesOfProgramsToIngest.First().StartOfDay();
                        var maxDate = bulkUploadJobData.DatesOfProgramsToIngest.Last().EndOfDay();
                        foreach ((var epgChannelId, var externalIdToProgram) in bulkUploadResultsDictionaries)
                        {
                            var linearAssetId = externalIdToProgram.Values.First().LiveAssetId;

                            _notificationManager.ChannelWasUpdated(
                                serviceEvent.RequestId,
                                bulkUpload.GroupId,
                                bulkUpload.UpdaterId,
                                linearAssetId,
                                epgChannelId,
                                minDate,
                                maxDate,
                                bulkUploadJobData.DisableEpgNotification);
                        }
                    }
                }

                SendIngestPartCompletedEvent(bulkUpload, relevantResults);

                Logger.Info($"BulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}]] > Ingest Validation for part of the bulk is completed, status:[{newStatus}]");
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

        private BulkUploadJobStatus SetOkayStatusToAllResults(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
        {
            var bulkUploadResultsOfCurrentDate = relevantResults.SelectMany(channel => channel.Value).Select(prog => prog.Value).ToList();
            bulkUploadResultsOfCurrentDate.ForEach(r => r.Status = BulkUploadResultStatus.Ok);

            BulkUploadManager.UpdateBulkUploadResults(bulkUploadResultsOfCurrentDate, out BulkUploadJobStatus newStatus);
            Logger.Info($"updated result bulkUploadId: [{bulkUpload.Id}] targetIndexName:[{serviceEvent.TargetIndexName}], countOfUpdates:[{bulkUploadResultsOfCurrentDate.Count}], results updated in CB, calculated status [{newStatus}]");
            return newStatus;
        }

        private void InvalidateEpgAssets(BulkUpload bulkUpload)
        {
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
        private Operations CalculateOperations(BulkUpload bulkUpload, BulkUploadResultsDictionary result)
        {
            // _bulkUpload.AddedObjects don't have EpgIds, because they were added in BulkUploadTransformationHandler, before inserting to DB
            // so we take EpgIds by EpgExternalIds
            var addedEpgIds = bulkUpload.AddedObjects.Where(_ => !_.IsAutoFill).Select(o => result[o.ChannelId][o.EpgExternalId].ObjectId.Value).ToArray();

            // Formulas:
            // AddedObjects = really-added
            // really-updated = AffectedObjects ∪ UpdatedObjects
            // DeletedObjects = really-deleted ∪ really-updated
            var deletedObjects = bulkUpload.DeletedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);
            var updatedObjects = bulkUpload.UpdatedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);
            var affectedObjects = bulkUpload.AffectedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);

            var reallyUpdated = affectedObjects.Union(updatedObjects).ToArray();
            var reallyDeleted = deletedObjects.Except(reallyUpdated).ToArray();

            Logger.Info($"reallyUpdated: {string.Join(",", reallyUpdated)}");
            Logger.Info($"reallyDeleted: {string.Join(",", reallyDeleted)}");

            return new Operations
            {
                AddedEpgIds = addedEpgIds,
                UpdatedEpgIds = reallyUpdated,
                DeletedEpgIds = reallyDeleted
            };
        }

        private void UpdateRecordings(BulkUpload bulkUpload, Operations operations)
        {
            UpdateRecordings(bulkUpload, operations.DeletedEpgIds, eAction.Delete);
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
            public long[] DeletedEpgIds;
        }

        private void TrySendIngestCompleted(BulkUpload bulkUpload, BulkUploadJobStatus newStatus)
        {
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

        private void SendIngestPartCompletedEvent(BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults)
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

        private Task PublishProgramAssetMessages(long groupId, Operations operations)
        {
            var createdEventsTask = _crudMessageService.PublishCreateEventsAsync(groupId, operations.AddedEpgIds);
            var updatedEventsTask = _crudMessageService.PublishUpdateEventsAsync(groupId, operations.UpdatedEpgIds);
            var deletedEventsTask = _crudMessageService.PublishDeleteEventsAsync(groupId, operations.DeletedEpgIds);

            return Task.WhenAll(createdEventsTask, updatedEventsTask, deletedEventsTask);
        }
    }
}