using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using ElasticSearch.Common;
using EpgBL;
using IngestHandler.Common;
using KLogMonitor;
using Polly;
using Polly.Retry;

namespace IngestHandler
{
    public class IngestFinalizerConfig
    {
        public int GroupId { get; set; }
        public long BulkUploadId { get; set; }
        public List<EpgProgramBulkUploadObject> EPGs { get; set; }

        public DateTime DateOfProgramsToIngest { get; set; }
        public IDictionary<string, LanguageObj> Languages { get; set; }
        public Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> Results { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(EPGs)}={EPGs}, {nameof(DateOfProgramsToIngest)}={DateOfProgramsToIngest}, {nameof(Languages)}={Languages}, {nameof(Results)}={Results}, {nameof(BulkUploadId)}={BulkUploadId}, {nameof(GroupId)}={GroupId}}}";
        }
    }

    public class IngestFinalizer
    {
        private const string INDEX_REFRESH_INTERVAL = "10s";
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private IngestFinalizerConfig _config;

        private readonly ElasticSearchApi _elasticSearchClient = null;
        private readonly RetryPolicy _ingestRetryPolicy;
        private BulkUpload _bulkUploadObject = null;

        /// <summary>
        /// This list contains all existing affected programs that were updated due
        /// to overlap policy and were cut to fit the new ingested programs
        /// </summary>
        private List<EpgProgramBulkUploadObject> _AffectedPrograms;

        private TvinciEpgBL _EpgBL;
        private BulkUploadIngestJobData _bulkUploadJobData;

        public IngestFinalizer()
        {
            _elasticSearchClient = new ElasticSearchApi();
            _ingestRetryPolicy = GetRetryPolicy<Exception>();
        }

        public Task FinalizeEpgIngest(IngestFinalizerConfig eventData)
        {
            try
            {
                _Logger.Debug($"Starting IngestFinalizer BulkUploadId:[{eventData.BulkUploadId}]");

                _config = eventData;
                _EpgBL = new TvinciEpgBL(_config.GroupId);
                _bulkUploadObject = BulkUploadMethods.GetBulkUploadData(_config.GroupId, _config.BulkUploadId);
                _bulkUploadJobData = _bulkUploadObject.JobData as BulkUploadIngestJobData;
                _AffectedPrograms = _bulkUploadObject.AffectedObjects?.Cast<EpgProgramBulkUploadObject>()?.ToList();

                var dailyEpgIndexName = IndexManager.GetDailyEpgIndexName(_config.GroupId, _config.DateOfProgramsToIngest);
                var isRefreshSuccess = IndexManager.ForceRefresh(dailyEpgIndexName);
                if (!isRefreshSuccess)
                {
                    _Logger.Error($"BulkId [{_config.BulkUploadId}], Date:[{_config.DateOfProgramsToIngest}] > index refresh failed");
                }

                var newStatus = SetStatusToAllCurrentResults(BulkUploadResultStatus.Ok);

                // Need to refresh the data from the bulk upload object after updating with the validation result
                _bulkUploadObject = BulkUploadMethods.GetBulkUploadData(eventData.GroupId, eventData.BulkUploadId);

                // All separated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the changes.
                if (BulkUpload.IsProcessCompletedByStatus(newStatus))
                {
                    _Logger.Debug($"BulkUploadId: [{_config.BulkUploadId}] Date:[{_config.DateOfProgramsToIngest}], Final part of bulk is marked, status is: [{newStatus}], finlizing bulk object");
                    InvalidateEpgAssets();
                    RefreshAllIndexes();
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUploadObject, newStatus);
                    _Logger.Info($"BulkUploadId: [{_config.BulkUploadId}] Date:[{_config.DateOfProgramsToIngest}] > Ingest Validation for entire bulk is completed, status:[{newStatus}]");
                }

                _Logger.Info($"BulkUploadId: [{_config.BulkUploadId}] Date:[{_config.DateOfProgramsToIngest}] > Ingest Validation for part of the bulk is completed, status:[{newStatus}]");
            }
            catch (Exception ex)
            {
                try
                {
                    _Logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{_config.BulkUploadId}] Date:[{_config.DateOfProgramsToIngest}]", ex);
                    _bulkUploadObject.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _bulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest validation, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUploadObject, BulkUploadJobStatus.Fatal);
                    _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler BulkUploadId:[{eventData.BulkUploadId}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"Error while trying to update bulk upload with failed status from ingestValidation, trying one last time...", innerEx);
                }

                throw;
            }

            return Task.CompletedTask;
        }

        private void RefreshAllIndexes()
        {
            var indexes = _bulkUploadJobData.DatesOfProgramsToIngest.Select(x => IndexManager.GetDailyEpgIndexName(_config.GroupId, x));
            var foundIndexes = new List<string>();

            foreach (var indexName in indexes)
            {
                if (!_elasticSearchClient.IndexExists(indexName)) { continue; }

                foundIndexes.Add(indexName);
                _ingestRetryPolicy.Execute(() =>
                {
                    var isSetRefreshSuccess = _elasticSearchClient.UpdateIndexRefreshInterval(indexName, INDEX_REFRESH_INTERVAL);
                    if (!isSetRefreshSuccess)
                    {
                        _Logger.Error($"BulkId [{_config.BulkUploadId}], Date:[{_config.DateOfProgramsToIngest}] > index set refresh to -1 failed [{isSetRefreshSuccess}]]");
                        throw new Exception("Could not set index refresh interval");
                    }
                });
            }
        }


        private RetryPolicy GetRetryPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) => { _Logger.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex); });
        }

        public void InvalidateEpgAssets()
        {
            var affectedProgramIds = _AffectedPrograms?.Select(p => (long) p.EpgId);
            var ingestedProgramIds = _bulkUploadObject.Results.Where(r => r.ObjectId.HasValue).Select(r => r.ObjectId.Value) ?? new List<long>();
            var programIdsToInvalidate = affectedProgramIds?.Any() == true
                ? ingestedProgramIds.Concat(affectedProgramIds)
                : ingestedProgramIds;

            var isOPC = GroupSettingsManager.IsOpc(_config.GroupId);
            foreach (var progId in programIdsToInvalidate)
            {
                string invalidationKey = isOPC
                    ? LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.EPG.ToString(), progId)
                    : LayeredCacheKeys.GetEpgInvalidationKey(_config.GroupId, progId);

                var invalidationResult = LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                if (!invalidationResult)
                {
                    _Logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", progId, eAssetType.PROGRAM.ToString(), invalidationKey);
                }

                _Logger.Debug($"SetInvalidationKey: [{invalidationKey}] done with result: [{invalidationResult}]");
            }
        }

        private BulkUploadJobStatus SetStatusToAllCurrentResults(BulkUploadResultStatus statusToSet)
        {
            foreach (var prog in _config.EPGs)
            {
                if (prog.EpgExternalId != null && _config.Results.ContainsKey(prog.ChannelId) && _config.Results[prog.ChannelId].ContainsKey(prog.EpgExternalId))
                {
                    var resultObj = _config.Results[prog.ChannelId][prog.EpgExternalId];
                    resultObj.ObjectId = (long) prog.EpgId;
                    resultObj.Status = statusToSet;
                    if (statusToSet != BulkUploadResultStatus.Ok)
                    {
                        resultObj.AddError(eResponseStatus.Error, "Failed elasticsearch index validation");
                    }
                }
            }

            var bulkUploadResultsOfCurrentDate = _config.Results.Values.SelectMany(r => r.Values);
            BulkUploadManager.UpdateBulkUploadResults(bulkUploadResultsOfCurrentDate, out BulkUploadJobStatus newStatus);
            _Logger.Debug($"BulkUploadId: [{_config.BulkUploadId}] Date:[{_config.DateOfProgramsToIngest}], results updated in CB, calculated status [{newStatus}]");
            return newStatus;
        }
    }
}