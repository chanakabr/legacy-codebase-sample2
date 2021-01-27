using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IngestHandler.Common
{
    public class EpgElasticUpdater
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ElasticSearchApi _elasticSearchClient = null;

        private readonly DateTime _dateOfProgramsToIngest;
        private readonly int _groupId;
        private readonly long _bulkUploadId;
        private readonly IDictionary<string, LanguageObj> _languages;
        private readonly ESSerializerV2 _serializer;
        private readonly LanguageObj _defaultLanguage;
        private readonly int bulkSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
        private readonly int sizeOfBulkDefaultValue = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();

        public EpgElasticUpdater(int groupId, long bulkUploadId, DateTime dateOfProgramsToIngest, IDictionary<string, LanguageObj> languages)
        {
            _elasticSearchClient = new ElasticSearchApi();
            _groupId = groupId;
            _bulkUploadId = bulkUploadId;
            _dateOfProgramsToIngest = dateOfProgramsToIngest;
            _languages = languages;
            _serializer = new ESSerializerV2();
            _defaultLanguage = languages.Values.First(l => l.IsDefault);

            // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
            bulkSize = bulkSize == 0 ? sizeOfBulkDefaultValue : bulkSize > sizeOfBulkDefaultValue ? sizeOfBulkDefaultValue : bulkSize;
        }

        public void Update(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, string epgIndexName)
        {
            var isOpc = GroupSettingsManager.IsOpc(_groupId);
            var metasToPad = GetMetasToPad(_groupId, isOpc);

            DeleteProgramsFromIndex(crudOperations.ItemsToDelete, epgIndexName);
            var programsToIndex = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems).ToList();
            UpsertProgramsToDraftIndex(programsToIndex, epgIndexName, isOpc, metasToPad);
        }

        private void UpsertProgramsToDraftIndex(IList<EpgProgramBulkUploadObject> calculatedPrograms, string draftIndexName, bool isOpc, HashSet<string> metasToPad)
        {
            var retryCount = 5;
            var policy = RetryPolicy.Handle<Exception>()
            .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
            {
                _logger.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
            });

            policy.Execute(() =>
            {
                var bulkRequests = new List<ESBulkRequestObj<string>>();
                try
                {
                    var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
                    foreach (var program in programTranslationsToIndex)
                    {
                        program.PadMetas(metasToPad);
                        var suffix = program.Language == _defaultLanguage.Code ? "" : program.Language;
                        var language = _languages[program.Language];

                        // Serialize EPG object to string
                        string serializedEpg = HanlderSerializedEpg(isOpc, program, suffix);
                        var epgType = GetTanslationType(IndexManager.EPG_INDEX_TYPE, language);

                        var totalMinutes = GetTTLMinutes(program);
                        // TODO: what should we do if someone trys to ingest something to the past ... :\
                        totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

                        var ttl = string.Format("{0}m", totalMinutes);

                        var bulkRequest = new ESBulkRequestObj<string>()
                        {
                            docID = program.EpgID.ToString(),
                            document = serializedEpg,
                            index = draftIndexName,
                            Operation = eOperation.index,
                            routing = _dateOfProgramsToIngest.Date.ToString("yyyyMMdd") /*program.StartDate.ToUniversalTime().ToString("yyyyMMdd")*/,
                            type = epgType,
                            ttl = ttl
                        };

                        bulkRequests.Add(bulkRequest);

                        // If we exceeded maximum size of bulk 
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequests.Count > 0)
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }
                finally
                {
                    if (bulkRequests != null && bulkRequests.Any())
                    {
                        _logger.Debug($"Clearing bulk requests");
                        bulkRequests.Clear();
                    }
                }
            });

        }

        private string HanlderSerializedEpg(bool isOpc, EpgCB program, string suffix)
        {

            try
            {
                return _serializer.SerializeEpgObject(program, suffix, isOpc);
            }
            catch (Exception e)
            {
                string msg = "";
                if (program != null && program.Crid != null)
                {
                    msg += $" program_crid: {program.Crid} ";
                    msg += $" program_epg_id: {program.EpgID} ";
                }

                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    msg += $" InnerException: {e.InnerException.Message} ";
                }

                _logger.Debug($"Error while calling SerializeEpgObject {msg} {e.Message}", e);
                throw;
            }

        }

        private void ExecuteAndValidateBulkRequests(List<ESBulkRequestObj<string>> bulkRequests)
        {
            // create bulk request now and clear list
            var invalidResults = _elasticSearchClient.CreateBulkRequest(bulkRequests);

            if (invalidResults != null && invalidResults.Count > 0)
            {
                foreach (var item in invalidResults)
                {
                    _logger.Error($"Could not add EPG to ES index. GroupID={_groupId} epgId={item.Key} error={item.Value}");
                }
            }

            if (invalidResults.Any())
            {
                throw new Exception($"Failed to upsert [{invalidResults.Count}] documents");
            }

            bulkRequests.Clear();
        }

        public void DeleteProgramsFromIndex(IList<EpgProgramBulkUploadObject> programsToDelete, string epgIndexName)
        {
            if (programsToDelete.Count() == 0) { return; }

            var programIds = programsToDelete.Select(program => program.EpgId);
            var channelIds = programsToDelete.Select(x => x.ChannelId).Distinct().ToList();
            var externalIds = programsToDelete.Select(program => program.EpgExternalId).Distinct().ToList();

            // We will retry deletion until the sum of all deleted programs is equal to the total docs deleted, this is becasue
            // there is an issue in elastic 2.3 where we cannot be sure it will find the item to delete
            // right after re-index.
            var totalDocumentsToDelete = programsToDelete.Count * _languages.Count;
            var totalDocumentsDeleted = 0;
            _logger.Debug($"Update elasticsearch index completed, delteting required docuements. documents.leng:[{programsToDelete.Count}]");
            if (programIds.Any())
            {
                var retryCount = 5;
                var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    _logger.Warn($"delete attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                });

                var deleteQuery = GetElasticsearchQueryForEpgIDs(programIds, externalIds ?? new List<string>(), channelIds);
                policy.Execute(() =>
                {
                    _elasticSearchClient.DeleteDocsByQuery(epgIndexName, "", ref deleteQuery, out var deletedDocsCount);
                    totalDocumentsDeleted += deletedDocsCount;
                    if (totalDocumentsDeleted < programIds.Count())
                    {
                        // throw new Exception($"requested to delete {programIds.Count()} programs but actually deleted so far {totalDocumentsDeleted} program ids are: {string.Join(",", programIds)}");
                    }
                });
            }
        }

        /// <summary>
        /// TODO: DO THIS
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        private HashSet<string> GetMetasToPad(int groupId, bool isOpc)
        {
            CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(_groupId, out var catalogGroupCache);
            var groupManager = new GroupManager();
            var group = groupManager.GetGroup(_groupId);
            IndexManager.GetMetasAndTagsForMapping(_groupId, isOpc, out var metas, out var tags, out var metasTopad, _serializer, group, catalogGroupCache, isEpg: true);
            return metasTopad;
        }

        private static string GetTanslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            else
            {
                return string.Concat(type, "_", language.Code);
            }
        }

        private double GetTTLMinutes(EpgCB epg)
        {
            return Math.Ceiling((epg.EndDate.AddDays(BulkUploadMethods.EXPIRY_DATE_DELTA) - DateTime.UtcNow).TotalMinutes);
        }

        public static string GetElasticsearchQueryForEpgIDs(IEnumerable<ulong> programIds, IEnumerable<string> externalIds, List<int> channelIds)
        {
            // Build query for getting programs
            var query = new FilteredQuery(true);
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 1;
            query.ReturnFields.Clear();

            var composite = new FilterCompositeType(CutWith.OR);

            // build terms query: epg_id IN (1, 2, 3 ... bulkSize)

            var epgIdTerms = ESTerms.GetSimpleNumericTerm("epg_id", programIds);

            composite.AddChild(epgIdTerms);

            //external id must be in one of the given channels
            var externalIdsComposite = new FilterCompositeType(CutWith.AND);
            var epgExternalIdTerms = ESTerms.GetSimpleStringTerm("epg_identifier", externalIds);
            var epgChannelIdTerms = ESTerms.GetSimpleStringTerm("epg_channel_id", channelIds);
            externalIdsComposite.AddChild(epgExternalIdTerms);
            externalIdsComposite.AddChild(epgChannelIdTerms);

            composite.AddChild(externalIdsComposite);
            filter.FilterSettings = composite;
            query.Filter = filter;

            var searchQuery = query.ToString();
            return searchQuery;
        }
    }
}
