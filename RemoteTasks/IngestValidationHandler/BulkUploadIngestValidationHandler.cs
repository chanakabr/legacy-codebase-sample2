using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
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
        private readonly CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

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
            _CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
        }

        public async Task Handle(BulkUploadIngestValidationEvent eventData)
        {
            try
            {
                _Logger.Debug($"Starting BulkUploadIngestValidationHandler  requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}]");

                _EventData = eventData;
                _EpgBL = new TvinciEpgBL(_EventData.GroupId);
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(_EventData.GroupId, _EventData.BulkUploadId);
                _AffectedPrograms = _BulkUploadObject.AffectedObjects?.Cast<EpgProgramBulkUploadObject>()?.ToList();

                var indexIsValid = ValidateClonedIndex(eventData);
                var statusToSetForResults = indexIsValid ? BulkUploadResultStatus.Ok : BulkUploadResultStatus.Error;
                _Logger.Debug($"Index validation done with result:[{indexIsValid}], setting results status:[{statusToSetForResults}]");
                SetStatusToAllCurrentResults(statusToSetForResults);

                BulkUploadManager.UpdateBulkUploadResults(_EventData.Results.Values.SelectMany(r => r.Values), out BulkUploadJobStatus newStatus);

                // Need to refresh the data from the bulk upload ibject after updating with the validation result
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(eventData.GroupId, eventData.BulkUploadId);

                // All seperated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the chanages.
                if (BulkUpload.IsProcessCompletedByStatus(newStatus))
                {
                    var retryCount = 5;
                    if (newStatus == BulkUploadJobStatus.Success)
                    {
                        var policy = RetryPolicy.Handle<Exception>()
                            .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                            {
                                // TODO: improve logging
                                _Logger.Warn($"SwitchAliases Attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                            }
                        );

                        policy.Execute(() =>
                        {
                            SwitchAliases();
                            InvalidateEpgAssets();
                            BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Success);
                        });

                    }
                    else
                    {
                        // If job is not success we will have to set the final status to the relevent failed\fatal
                        BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, newStatus);
                    }

                    try
                    {
                        TriggerElasticIndexCleanerForPartner(_BulkUploadObject, eventData);
                    }
                    catch (Exception e)
                    {
                        _Logger.Error($"Error while running ElasticIndexCleaner", e);
                    }
                }


            }
            catch (Exception ex)
            {
                try
                {
                    _BulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest validation, {ex.Message}");
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Fatal);
                    _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}].", ex);

                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"Error while trying to update bulk upload with failed status from ingestValidation");
                }

                throw;
            }
        }

        private static void TriggerElasticIndexCleanerForPartner(BulkUpload bulkUploadResultAfterUpdate, BulkUploadIngestValidationEvent eventData)
        {
            if (bulkUploadResultAfterUpdate.IsProcessCompleted)
            {
                var cleaner = new ElasticsearchIndexCleaner.IndexCleaner();
                cleaner.Clean(new[] { eventData.GroupId }, 1);
            }
        }

        private bool ValidateClonedIndex(BulkUploadIngestValidationEvent eventDate)
        {
            // Wait time is 1 sec + 50ms for every program that was indexed
            // TODO: make configurable
            var epgsCounts = eventDate.EPGs.Count;// + eventDate.EdgeProgramsToUpdate.Count;

            var delayMsBeforeValidation = 1000 + (epgsCounts * 10);
            Thread.Sleep(delayMsBeforeValidation);
            var result = false;
            int retryCount = 5; // TODO: Tcm configuration?

            var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    _Logger.Warn($"Validation Attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                }
            );

            var index = IndexManager.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);

            // Checking all languages by searhcing for all types
            var type = string.Empty;
            var isValid = true;

            policy.Execute(() =>
            {
                isValid = true;

                var allEpgs = eventDate.EPGs;//.Concat(eventDate.EdgeProgramsToUpdate);

                if (allEpgs.Any())
                {
                    var searchQuery = GetElasticsearchQueryForEpgIDs(allEpgs.Select(program => program.EpgId));

                    var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

                    var jsonResult = JObject.Parse(searchResult);
                    var tempToken = jsonResult.SelectToken("hits.total");
                    var totalItems = tempToken?.Value<int>() ?? 0;
                    var expectedCount = allEpgs.SelectMany(p => p.EpgCbObjects).Count();
                    if (totalItems != expectedCount)
                    {
                        isValid = false;
                    }

                    if (!isValid)
                    {
                        _Logger.Warn($"Missing program from ES index.");
                        throw new Exception("Missing program from ES index");
                    }
                }
            });

            result = isValid;

            return result;
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

        private static string GetElasticsearchQueryForEpgIDs(IEnumerable<ulong> programIds)
        {
            // Build query for getting programs
            var query = new FilteredQuery(true);
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 1;
            query.ReturnFields.Clear();

            var composite = new FilterCompositeType(CutWith.AND);

            // build terms query: epg_id IN (1, 2, 3 ... bulkSize)
            var terms = ESTerms.GetSimpleNumericTerm("epg_id", programIds);
            composite.AddChild(terms);

            filter.FilterSettings = composite;
            query.Filter = filter;

            var searchQuery = query.ToString();
            return searchQuery;
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

        /// <summary>
        /// This method should be called only when the full bulk upload ingest was proccessed.
        /// It will get all indices of current bulkUploadId and switch their respective aliase
        /// switch aliases - 
        /// delete epg_203_20190422 for epg_203_20190422_old_bulk_upload_id
        /// add epg_203_20190422 for epg_203_20190422_current_bulk_upload_id
        /// </summary>
        /// <param name="bulkUploadId"></param>
        /// <param name="dateOfIngest"></param>
        private void SwitchAliases()
        {
            // Get list of all indices of current bulk upload
            var allindicesOfCurrentBulk = _ElasticSearchClient.ListIndices($"{_BulkUploadObject.GroupId}_epg_v2_*_{_BulkUploadObject.Id}");

            foreach (var newIndex in allindicesOfCurrentBulk)
            {
                // remove the bulkUploadId suffix from the index
                var dateAlias = newIndex.Name.Remove(newIndex.Name.Length - (_BulkUploadObject.Id.ToString().Length + 1));
                var globalAlias = _EpgBL.GetProgramIndexAlias();

                // Should only be one but we will loop anyway ...
                var previousIndices = _ElasticSearchClient.GetAliases(dateAlias);
                if (previousIndices.Any())
                {
                    //todo handle on error we might have empty aliases
                    _Logger.Debug($"Removing alias:[{dateAlias}, {globalAlias}] from:[{string.Join(",", previousIndices)}].");
                    foreach (var oldIndex in previousIndices)
                    {
                        var isGlobalAliasRemoveSuccess = _ElasticSearchClient.RemoveAlias(oldIndex, globalAlias);
                        if (!isGlobalAliasRemoveSuccess) { throw new Exception($"Failed to remove globalAlias:[{globalAlias}] oldIndex:[{oldIndex}]"); }
                        var isDateAliasRemoveSuccess = _ElasticSearchClient.RemoveAlias(oldIndex, dateAlias);
                        if (!isDateAliasRemoveSuccess) { throw new Exception($"Failed to remove dateAlias:[{dateAlias}] oldIndex:[{oldIndex}]"); }

                        //tagging index to be deleted
                        var isAddedDeleteCandidate=_ElasticSearchClient.AddAlias(oldIndex, ESUtils.DELETE_CANDIDATE_ALIAS);
                        if (!isAddedDeleteCandidate) { throw new Exception($"Failed to add  {ESUtils.DELETE_CANDIDATE_ALIAS} alias {oldIndex}"); }

                    }

                }

                _Logger.Debug($"Adding alias:[{dateAlias}, {globalAlias}] To:[{newIndex}].");

                var isSetDateAliasSuccess = _ElasticSearchClient.AddAlias(newIndex.Name, dateAlias);
                if (!isSetDateAliasSuccess) { throw new Exception($"Failed to add dateAlias:[{dateAlias}] newIndex.Name:[{newIndex.Name}]"); }

                var isSetGlobalAliasSuccess = _ElasticSearchClient.AddAlias(newIndex.Name, globalAlias);
                if (!isSetGlobalAliasSuccess) { throw new Exception($"Failed to add globalAlias:[{globalAlias}] newIndex.Name:[{newIndex.Name}]"); }

            }
        }
    }
}
