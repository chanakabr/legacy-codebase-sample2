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

                var indexIsValid = ValidateClonedIndex(eventData);
                _Logger.Debug($"Index validation done with result:[{indexIsValid}]");

                GenericResponse<BulkUpload> bulkUploadResultAfterUpdate = null;

                if (indexIsValid)
                {
                    UpdateBulkUploadResults(eventData.Results, eventData.EPGs);
                    SwitchAliases();
                    BulkUploadManager.UpdateBulkUploadResults(eventData.Results.Values);
                    bulkUploadResultAfterUpdate = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Success);

                    // Update edgs if there are any updates to be made dure to overlap
                    if (eventData.EdgeProgramsToUpdate.Any())
                    {
                        await BulkUploadMethods.UpdateCouchbase(eventData.EdgeProgramsToUpdate, eventData.GroupId);
                        var updater = new UpdateClonedIndex(eventData.GroupId, eventData.BulkUploadId, eventData.DateOfProgramsToIngest, eventData.Languages);
                        updater.Update(eventData.EdgeProgramsToUpdate, new List<EpgProgramBulkUploadObject>());
                    }

                    InvalidateEpgAssets(eventData.EPGs.Concat(eventData.EdgeProgramsToUpdate));
                }
                else
                {
                    BulkUploadManager.UpdateBulkUploadResults(eventData.Results.Values);
                    bulkUploadResultAfterUpdate = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Failed);
                }

                // fire ps event
                if (bulkUploadResultAfterUpdate.Object.IsProcessCompleted)
                {
                    _Logger.DebugFormat($"Firing PS event: '{0}'", event_name);
                    _BulkUploadObject.Notify(eKalturaEventTime.After, event_name);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{eventData.RequestId}], BulkUploadId:[{eventData.BulkUploadId}].", ex);
                throw;
            }
        }

        private bool ValidateClonedIndex(BulkUploadIngestValidationEvent eventDate)
        {
            // Wait time is 2 sec + 50ms for every program that was indexed
            // TODO: make configurable
            var delayMsBeforeValidation = 2000 + (eventDate.EPGs.Count * 10);
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
                var searchQuery = GetElasticsearchQueryForEpgIDs(eventDate.EPGs.Select(program => program.EpgId));

                var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

                var jsonResult = JObject.Parse(searchResult);
                var tempToken = jsonResult.SelectToken("hits.total");
                int totalItems = tempToken?.Value<int>() ?? 0;

                if (totalItems != eventDate.EPGs.SelectMany(p => p.EpgCbObjects).Count())
                {
                    isValid = false;
                }


                if (!isValid)
                {
                    _Logger.Warn($"Missing program from ES index.");
                    throw new Exception("Missing program from ES index");
                }
            });

            result = isValid;

            return result;
        }

        public static void InvalidateEpgAssets(IEnumerable<EpgProgramBulkUploadObject> programs)
        {
            foreach (var prog in programs)
            {
                string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.EPG.ToString(), (long)prog.EpgId);

                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    _Logger.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after EpgIngest", prog.EpgId, eAssetType.PROGRAM.ToString(), invalidationKey);
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

        private void UpdateBulkUploadResults(Dictionary<string, BulkUploadProgramAssetResult> results, List<EpgProgramBulkUploadObject> epgs)
        {
            foreach (var prog in epgs)
            {
                if (prog.EpgExternalId != null && results.ContainsKey(prog.EpgExternalId))
                {
                    var resultObj = results[prog.EpgExternalId];
                    resultObj.ObjectId = (long)prog.EpgId;
                    resultObj.Status = BulkUploadResultStatus.Ok;
                    // TODO: allow updating results in bulk
                    //BulkUploadManager.UpdateBulkUploadResult(_EventData.GroupId, _BulkUploadObject.Id, resultObj.Index, Status.Ok, resultObj.ObjectId, resultObj.Warnings);
                }
            }
        }

        /// <summary>
		/// switch aliases - 
		/// delete epg_203_20190422 for epg_203_20190422_old_bulk_upload_id
		/// add epg_203_20190422 for epg_203_20190422_current_bulk_upload_id
		/// </summary>
		/// <param name="bulkUploadId"></param>
		/// <param name="dateOfIngest"></param>
		private void SwitchAliases()
        {
            string currentProgramsAlias = BulkUploadMethods.GetIngestCurrentProgramsAliasName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
            string globalAlias = _EpgBL.GetProgramIndexAlias();


            // Should only be one but we will loop anyway ...
            var previousIndices = _ElasticSearchClient.GetAliases(currentProgramsAlias);
            _Logger.Debug($"Removing alias:[{currentProgramsAlias}, {globalAlias}] from:[{string.Join(",", previousIndices)}].");
            foreach (var index in previousIndices)
            {
                _ElasticSearchClient.RemoveAlias(index, globalAlias);
                _ElasticSearchClient.RemoveAlias(index, currentProgramsAlias);
            }

            string newIndex = BulkUploadMethods.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);
            _Logger.Debug($"Adding alias:[{currentProgramsAlias}, {globalAlias}] To:[{string.Join(",", newIndex)}].");
            _ElasticSearchClient.AddAlias(newIndex, currentProgramsAlias);
            _ElasticSearchClient.AddAlias(newIndex, globalAlias);
        }
    }
}
