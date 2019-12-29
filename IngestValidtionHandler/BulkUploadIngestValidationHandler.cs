using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
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
using System.Threading.Tasks;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestValidtionHandler
{
    public class BulkUploadIngestValidationHandler : IServiceEventHandler<BulkUploadIngestValidationEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string event_name = "KalturaBulkUpload";

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
                _EpgBL = new TvinciEpgBL(eventData.GroupId);
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(eventData.GroupId, eventData.BulkUploadId);
                _AffectedPrograms = _BulkUploadObject.AffectedObjects?.Cast<EpgProgramBulkUploadObject>()?.ToList();

                var indexIsValid = ValidateClonedIndex(eventData);
                _Logger.Debug($"Index validation done with result:[{indexIsValid}]");

                GenericResponse<BulkUpload> bulkUploadResultAfterUpdate = null;

                if (indexIsValid)
                {
                    UpdateBulkUploadResults(eventData.Results, eventData.EPGs);
                    BulkUploadManager.UpdateBulkUploadResults(eventData.Results.Values.SelectMany(r => r.Values), out BulkUploadJobStatus newStatus);
                    UpdateBulkUploadStatus(_BulkUploadObject, newStatus);

                    // Update edgs if there are any updates to be made due to overlap
                    if (_AffectedPrograms?.Any() == true)
                    {
                        await BulkUploadMethods.UpdateCouchbase(_AffectedPrograms, eventData.GroupId);
                        var updater = new UpdateClonedIndex(eventData.GroupId, eventData.BulkUploadId, eventData.DateOfProgramsToIngest, eventData.Languages);
                        updater.Update(_AffectedPrograms, new List<EpgProgramBulkUploadObject>());
                    }

                }
                else
                {
                    BulkUploadManager.UpdateBulkUploadResults(eventData.Results.Values.SelectMany(r => r.Values), out BulkUploadJobStatus tmp);
                    bulkUploadResultAfterUpdate = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Failed);
                }

                // All seperated jobs of bulkUpload were completed, we are the last one, we need to switch the alias and commit the chanages.
                if (_BulkUploadObject.IsProcessCompleted)
                {
                    SwitchAliases();
                    InvalidateEpgAssets();
                    EmmitPSEvent(_BulkUploadObject);
                }

                TriggerElasticIndexCleanerForPartner(_BulkUploadObject, eventData);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}].", ex);
                throw;
            }
        }

        private void UpdateBulkUploadStatus(BulkUpload bulkUploadObject, BulkUploadJobStatus newStatus)
        {
            if (newStatus == BulkUploadJobStatus.Success)
            {
                BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, newStatus);
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

        private void EmmitPSEvent(BulkUpload bulkUploadResultAfterUpdate)
        {
            _Logger.DebugFormat($"Firing PS event: '{0}'", event_name);
            _BulkUploadObject.Notify(eKalturaEventTime.After, event_name);

        }

        private bool ValidateClonedIndex(BulkUploadIngestValidationEvent eventDate)
        {
            // Wait time is 2 sec + 50ms for every program that was indexed
            // TODO: make configurable
            var epgsCounts = eventDate.EPGs.Count;// + eventDate.EdgeProgramsToUpdate.Count;

            var delayMsBeforeValidation = 2000 + (epgsCounts * 10);
            var result = false;
            int retryCount = 5; // TODO: Tcm configuration?

            var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    _Logger.Warn($"Validation Attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                }
            );

            var index = BulkUploadMethods.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);

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
                    int totalItems = tempToken?.Value<int>() ?? 0;

                    if (totalItems != allEpgs.SelectMany(p => p.EpgCbObjects).Count())
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


            foreach (var progId in programIdsToInvalidate)
            {
                string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.EPG.ToString(), progId);

                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    _Logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", progId, eAssetType.PROGRAM.ToString(), invalidationKey);
                }

                _Logger.Debug($"SetInvalidationKey done with result:[{invalidationKey}]");
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

        private void UpdateBulkUploadResults(Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> results, List<EpgProgramBulkUploadObject> epgs)
        {
            foreach (var prog in epgs)
            {
                if (prog.EpgExternalId != null && results.ContainsKey(prog.ChannelId) && results[prog.ChannelId].ContainsKey(prog.EpgExternalId))
                {
                    var resultObj = results[prog.ChannelId][prog.EpgExternalId];
                    resultObj.ObjectId = (long)prog.EpgId;
                    resultObj.Status = BulkUploadResultStatus.Ok;
                    // TODO: allow updating results in bulk
                    //BulkUploadManager.UpdateBulkUploadResult(_EventData.GroupId, _BulkUploadObject.Id, resultObj.Index, Status.Ok, resultObj.ObjectId, resultObj.Warnings);
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
                    _Logger.Debug($"Removing alias:[{dateAlias}, {globalAlias}] from:[{string.Join(",", previousIndices)}].");
                    foreach (var oldIndex in previousIndices)
                    {
                        _ElasticSearchClient.RemoveAlias(oldIndex, globalAlias);
                        _ElasticSearchClient.RemoveAlias(oldIndex, dateAlias);
                    }
                }

                _Logger.Debug($"Adding alias:[{dateAlias}, {globalAlias}] To:[{newIndex}].");

                _ElasticSearchClient.AddAlias(newIndex.Name, dateAlias);
                _ElasticSearchClient.AddAlias(newIndex.Name, globalAlias);
            }
        }
    }
}
