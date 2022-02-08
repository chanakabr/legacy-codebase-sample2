using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using ElasticSearch.Common;
using IngestHandler.Common;
using Phx.Lib.Log;
using Polly;
using Polly.Retry;
using TVinciShared;

namespace IngestHandler
{
    public class IngestFinalizer
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        private BulkUpload _bulkUpload;
        private readonly IIndexManager _indexManager;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private readonly RetryPolicy _ingestRetryPolicy;
        private readonly BulkUploadResultsDictionary _relevantResults;
        private readonly string _targetIndexName;
        private readonly string _requestId;
        private readonly BulkUploadIngestJobData _bulkUploadJobData;
        private readonly EpgNotificationManager _notificationManager;


        public IngestFinalizer(BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults,
            string targetIndexName, string requestId, IIndexManager indexManager, IEpgIngestMessaging epgIngestMessaging)
        {
            _indexManager = indexManager;
            _epgIngestMessaging = epgIngestMessaging;
            _ingestRetryPolicy = GetRetryPolicy<Exception>();
            _bulkUpload = bulkUpload;
            _relevantResults = relevantResults;
            _bulkUploadJobData = bulkUpload.JobData as BulkUploadIngestJobData;
            _targetIndexName = targetIndexName;
            _requestId = requestId;
            _notificationManager = EpgNotificationManager.Instance();
        }

        public Task FinalizeEpgIngest()
        {
            try
            {
                _logger.Debug($"Starting IngestFinalizer BulkUploadId:[{_bulkUpload.Id}]");


                var isRefreshSuccess = _indexManager.ForceRefreshEpgV2Index(_targetIndexName);

                if (!isRefreshSuccess)
                {
                    _logger.Error($"BulkUploadId [{_bulkUpload.Id}], targetIndexName:[{_targetIndexName}] > index refresh failed");
                }

                var newStatus = SetOkayStatusToAllResults();

                // Need to refresh the data from the bulk upload object after updating with the validation result
                _bulkUpload = BulkUploadMethods.GetBulkUploadData(_bulkUpload.GroupId, _bulkUpload.Id);

                // All separated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the changes.
                if (BulkUpload.IsProcessCompletedByStatus(newStatus))
                {
                    _logger.Debug($"BulkUploadId: [{_bulkUpload.Id}] targetIndexName:[{_targetIndexName}], Final part of bulk is marked, status is: [{newStatus}], finlizing bulk object");
                    bool finalizeResult = _indexManager.FinalizeEpgV2Indices(_bulkUploadJobData.DatesOfProgramsToIngest.ToList());

                    if (!finalizeResult)
                    {
                        _logger.Error($"BulkUploadId [{_bulkUpload.Id}], targetIndexName:[{_targetIndexName}]] > index set refresh to -1 failed ]");
                        throw new Exception("Could not set index refresh interval");
                    }

                    InvalidateEpgAssets();
                    BulkUploadResultsDictionary bulkUploadResultsDictionaries = _bulkUpload.ConstructResultsDictionary();
                    var operations = CalculateOperations(bulkUploadResultsDictionaries);
                    UpdateRecordings(operations);

                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, newStatus);
                    if (result.IsOkStatusCode()) TrySendIngestCompleted(newStatus);
                    _logger.Info($"BulkUploadId: [{_bulkUpload.Id}] targetIndexName:[{_targetIndexName}]] > Ingest Validation for entire bulk is completed, status:[{newStatus}]");
                    if (newStatus == BulkUploadJobStatus.Success)
                    {
                        // DatesOfProgramsToIngest are ordered ascending [min..max]. 
                        // DatesOfProgramsToIngest - these are dates for ALL channels
                        // and potentially specific channel could have no updates on some dates,
                        // but we still notify that it was changed in date range [min..max]. could be better.
                        var minDate = _bulkUploadJobData.DatesOfProgramsToIngest.First().StartOfDay();
                        var maxDate = _bulkUploadJobData.DatesOfProgramsToIngest.Last().EndOfDay();
                        foreach ((var epgChannelId, var externalIdToProgram) in bulkUploadResultsDictionaries)
                        {
                            var linearAssetId = externalIdToProgram.Values.First().LiveAssetId;

                            _notificationManager.ChannelWasUpdated(
                                _requestId,
                                _bulkUpload.GroupId,
                                _bulkUpload.UpdaterId,
                                linearAssetId,
                                epgChannelId,
                                minDate,
                                maxDate,
                                _bulkUploadJobData.DisableEpgNotification);
                        }
                    }
                }

                SendIngestPartCompletedEvent();

                _logger.Info($"BulkUploadId: [{_bulkUpload.Id}] targetIndexName:[{_targetIndexName}]] > Ingest Validation for part of the bulk is completed, status:[{newStatus}]");
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{_bulkUpload.Id}] targetIndexName:[{_targetIndexName}]", ex);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest validation, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, BulkUploadJobStatus.Fatal);
                    if (result.IsOkStatusCode()) TrySendIngestCompleted(BulkUploadJobStatus.Fatal);
                    _logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler BulkUploadId:[{_bulkUpload.Id}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"Error while trying to update bulk upload with failed status from ingestValidation, trying one last time...", innerEx);
                }

                throw;
            }

            return Task.CompletedTask;
        }


        private RetryPolicy GetRetryPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) => { _logger.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex); });
        }



        private BulkUploadJobStatus SetOkayStatusToAllResults()
        {
            var bulkUploadResultsOfCurrentDate = _relevantResults.SelectMany(channel => channel.Value).Select(prog => prog.Value).ToList();
            bulkUploadResultsOfCurrentDate.ForEach(r => r.Status = BulkUploadResultStatus.Ok);

            BulkUploadManager.UpdateBulkUploadResults(bulkUploadResultsOfCurrentDate, out BulkUploadJobStatus newStatus);
            _logger.Info($"updated result bulkUploadId: [{_bulkUpload.Id}] targetIndexName:[{_targetIndexName}], countOfUpdates:[{bulkUploadResultsOfCurrentDate.Count}], results updated in CB, calculated status [{newStatus}]");
            return newStatus;
        }

        private void InvalidateEpgAssets()
        {
            var affectedProgramIds = _bulkUpload.AffectedObjects?.Select(p => (long)p.ObjectId);
            var ingestedProgramIds = _bulkUpload.Results.Where(r => r.ObjectId.HasValue).Select(r => r.ObjectId.Value) ?? new List<long>();
            var programIdsToInvalidate = affectedProgramIds?.Any() == true
                ? ingestedProgramIds.Concat(affectedProgramIds)
                : ingestedProgramIds;

            var isOPC = GroupSettingsManager.Instance.IsOpc(_bulkUpload.GroupId);
            foreach (var progId in programIdsToInvalidate)
            {
                string invalidationKey = isOPC
                    ? LayeredCacheKeys.GetAssetInvalidationKey(_bulkUpload.GroupId, eAssetTypes.EPG.ToString(), progId)
                    : LayeredCacheKeys.GetEpgInvalidationKey(_bulkUpload.GroupId, progId);

                var invalidationResult = LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                if (!invalidationResult)
                {
                    _logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", progId, eAssetType.PROGRAM.ToString(), invalidationKey);
                }

                _logger.Debug($"SetInvalidationKey: [{invalidationKey}] done with result: [{invalidationResult}]");
            }
        }

        // Logically hard part, which could have errors, should be tested
        // Logical trick: DeletedObjects - it's all EPGs, which should be removed from Elastic. Even just updated EPGs are added to DeletedObjects
        // Probably this calculation should be in BulkUploadTransformationHandler
        private Operations CalculateOperations(BulkUploadResultsDictionary result)
        {
            // _bulkUpload.AddedObjects don't have EpgIds, because they were added in BulkUploadTransformationHandler, before inserting to DB
            // so we take EpgIds by EpgExternalIds
            var addedEpgIds = _bulkUpload.AddedObjects.Where(_ => !_.IsAutoFill).Select(o => result[o.ChannelId][o.EpgExternalId].ObjectId.Value).ToArray();

            // Formulas:
            // AddedObjects = really-added
            // really-updated = AffectedObjects ∪ UpdatedObjects
            // DeletedObjects = really-deleted ∪ really-updated
            var deletedObjects = _bulkUpload.DeletedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);
            var updatedObjects = _bulkUpload.UpdatedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);
            var affectedObjects = _bulkUpload.AffectedObjects.Where(_ => !_.IsAutoFill).Select(_ => (long)_.ObjectId);

            var reallyUpdated = affectedObjects.Union(updatedObjects).ToArray();
            var reallyDeleted = deletedObjects.Except(reallyUpdated).ToArray();

            return new Operations
            {
                AddedEpgIds = addedEpgIds,
                UpdatedEpgIds = reallyUpdated,
                DeletedEpgIds = reallyDeleted
            };
        }

        private void UpdateRecordings(Operations operations)
        {
            UpdateRecordings(operations.DeletedEpgIds, eAction.Delete);
            UpdateRecordings(operations.AddedEpgIds, eAction.On);
            UpdateRecordings(operations.UpdatedEpgIds, eAction.Update);
        }

        private void UpdateRecordings(long[] epgIds, eAction action)
        {
            if (epgIds.Length == 0) return;

            var logSuffix = $"bulkUploadId:[{_bulkUpload.Id}] action:[{action}] EPGids:[{string.Join(',', epgIds)}]";
            _logger.Info($"Try to update recordings. {logSuffix}");
            try
            {
                Core.ConditionalAccess.Module.IngestRecording(_bulkUpload.GroupId, epgIds, action);
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to update recordings. {logSuffix}", exception);
            }
        }

        private class Operations
        {
            public long[] AddedEpgIds;
            public long[] UpdatedEpgIds;
            public long[] DeletedEpgIds;
        }
        
        private void TrySendIngestCompleted(BulkUploadJobStatus newStatus)
        {
            if (!BulkUpload.IsProcessCompletedByStatus(newStatus)) return;
            
            var updateDate = DateTime.UtcNow; // TODO looks like _bulUpload.UpdateDate is not updated in CB
            var parameters = new EpgIngestCompletedParameters
            {
                GroupId = _bulkUpload.GroupId,
                BulkUploadId = _bulkUpload.Id,
                Status = newStatus,
                Errors = _bulkUpload.Errors,
                CompletedDate = updateDate,
                UserId = _bulkUpload.UpdaterId,
                Results = _bulkUpload.Results
            };

            _epgIngestMessaging.EpgIngestCompleted(parameters);
        }

        private void SendIngestPartCompletedEvent()
        {
            var programIngestResults = _relevantResults.Values
                .SelectMany(x => x.Values)
                .ToArray();
            var hasMoreToIngest = !BulkUpload.IsProcessCompletedByStatus(_bulkUpload.Status);
            var parameters = new EpgIngestPartCompletedParameters
            {
                BulkUploadId = _bulkUpload.Id,
                GroupId = _bulkUpload.GroupId,
                HasMoreEpgToIngest = hasMoreToIngest,
                UserId = _bulkUpload.UpdaterId,
                Results = programIngestResults
            };

            _epgIngestMessaging.EpgIngestPartCompleted(parameters);
        }
    }
}