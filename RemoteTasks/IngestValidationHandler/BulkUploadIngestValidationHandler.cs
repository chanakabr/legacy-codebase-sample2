using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using CouchbaseManager;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using EpgBL;
using EventBus.Abstraction;
using IngestHandler.Common;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestValidationHandler
{
    public class BulkUploadIngestValidationHandler : IServiceEventHandler<BulkUploadIngestValidationEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private BulkUploadIngestValidationEvent _EventData;

        private readonly ElasticSearchApi _ElasticSearchClient = null;
        private readonly RetryPolicy _IngestRetryPolicy;
        private const string INDEX_REFRESH_INTERVAL = "10s";
        private BulkUpload _BulkUploadObject = null;

        /// <summary>
        /// This list contains all existing affected programs that were updated due
        /// to overlap policy and were cut to fit the new ingested programs
        /// </summary>
        private List<EpgProgramBulkUploadObject> _AffectedPrograms;
        private TvinciEpgBL _EpgBL;

        public BulkUploadIngestValidationHandler()
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _IngestRetryPolicy = GetRetrayPolicy<Exception>();
        }

        public Task Handle(BulkUploadIngestValidationEvent eventData)
        {
            try
            {
                _Logger.Debug($"Starting BulkUploadIngestValidationHandler  requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}]");

                _EventData = eventData;
                _EpgBL = new TvinciEpgBL(_EventData.GroupId);
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(_EventData.GroupId, _EventData.BulkUploadId);
                _AffectedPrograms = _BulkUploadObject.AffectedObjects?.Cast<EpgProgramBulkUploadObject>()?.ToList();

                var dailyEpgIndexName = IndexManager.GetDailyEpgIndexName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
                var isRefreshSuccess = IndexManager.ForceRefresh(dailyEpgIndexName);
                if (!isRefreshSuccess)
                {
                    _Logger.Error($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > index refresh failed");
                    // todo: retry ? 
                }

                SetStatusToAllCurrentResults(BulkUploadResultStatus.Ok);
                var bulkUploadResultsOfCurrentDate = _EventData.Results.Values.SelectMany(r => r.Values);
                BulkUploadManager.UpdateBulkUploadResults(bulkUploadResultsOfCurrentDate, out BulkUploadJobStatus newStatus);
                _Logger.Debug($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}], results updated in CB, calculated status [{newStatus}]");

                // Need to refresh the data from the bulk upload object after updating with the validation result
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(eventData.GroupId, eventData.BulkUploadId);
                

                // All seperated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the changes.
                if (BulkUpload.IsProcessCompletedByStatus(newStatus))
                {
                    _Logger.Debug($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}], Final part of bulk is marked, status is: [{newStatus}], finlizing bulk object");
                    if (newStatus == BulkUploadJobStatus.Success)
                    {
                        InvalidateEpgAssets();
                        BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Success);
                        _IngestRetryPolicy.Execute(() =>
                        {
                            var isSetRefreshSuccess = _ElasticSearchClient.UpdateIndexRefreshInterval(dailyEpgIndexName, INDEX_REFRESH_INTERVAL);
                            if (!isSetRefreshSuccess)
                            {
                                _Logger.Error($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > index set refresh to -1 failed [{isSetRefreshSuccess}], dailyEpgIndexName [{dailyEpgIndexName}]");
                                throw new Exception("Could not set index refresh interval");
                            }
                        });                        
                    }
                    else
                    {
                        // If job is not success we will have to set the final status to the relevent failed\fatal
                        BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, newStatus);
                    }

                    _Logger.Info($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}] > Ingest Validation for entire bulk is completed, status:[{newStatus}]");
                }

                _Logger.Info($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}] > Ingest Validation for part of the bulk is completed, status:[{newStatus}]");

            }
            catch (Exception ex)
            {
                try
                {
                    _Logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}]", ex);
                    _BulkUploadObject.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _BulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest validation, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Fatal);
                    _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}], update result status [{result.Status}].", ex);

                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"Error while trying to update bulk upload with failed status from ingestValidation, trying one last time...", innerEx);
                }

                throw;
            }

            return Task.CompletedTask;
        }


        private RetryPolicy GetRetrayPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
            .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
            {
                _Logger.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
            });
        } 
        public void InvalidateEpgAssets()
        {
            var affectedProgramIds = _AffectedPrograms?.Select(p => (long)p.EpgId);
            var ingestedProgramIds = _BulkUploadObject.Results.Where(r => r.ObjectId.HasValue).Select(r => r.ObjectId.Value);
            var programIdsToInvalidate = affectedProgramIds?.Any() == true
                ? ingestedProgramIds.Concat(affectedProgramIds)
                : ingestedProgramIds;

            var isOPC = GroupSettingsManager.IsOpc(_EventData.GroupId);
            foreach (var progId in programIdsToInvalidate)
            {

                string invalidationKey = isOPC
                    ? LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.EPG.ToString(), progId)
                    : LayeredCacheKeys.GetEpgInvalidationKey(_EventData.GroupId, progId);

                var invalidationResult = LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                if (!invalidationResult)
                {
                    _Logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", progId, eAssetType.PROGRAM.ToString(), invalidationKey);
                }

                _Logger.Debug($"SetInvalidationKey: [{invalidationKey}] done with result: [{invalidationResult}]");
            }
        }

        private void SetStatusToAllCurrentResults(BulkUploadResultStatus statusToSet)
        {
            foreach (var prog in _EventData.EPGs)
            {
                if (prog.EpgExternalId != null && _EventData.Results.ContainsKey(prog.ChannelId) && _EventData.Results[prog.ChannelId].ContainsKey(prog.EpgExternalId))
                {
                    var resultObj = _EventData.Results[prog.ChannelId][prog.EpgExternalId];
                    resultObj.ObjectId = (long)prog.EpgId;
                    resultObj.Status = statusToSet;
                    if (statusToSet != BulkUploadResultStatus.Ok)
                    {
                        resultObj.AddError(eResponseStatus.Error, "Failed elasticsearch index validation");
                    }
                }
            }
        }
    }
}
