using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.GroupManagers;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IngestHandler.Common
{
    public class UpdateClonedIndex
    {
        public static readonly string DEFAULT_INDEX_MAPPING_TYPE = "epg";

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ElasticSearchApi _ElasticSearchClient = null;

        private DateTime _DateOfProgramsToIngest;
        private int _GroupId;
        private long _BulkUploadId;
        private IDictionary<string, LanguageObj> _Languages;

        public UpdateClonedIndex(int groupId, long bulkUploadId, DateTime dateOfProgramsToIngest, IDictionary<string, LanguageObj> languages)
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _GroupId = groupId;
            _BulkUploadId = bulkUploadId;
            _DateOfProgramsToIngest = dateOfProgramsToIngest;
            _Languages = languages;
        }

        public void Update(IList<EpgProgramBulkUploadObject> calculatedPrograms, IList<EpgProgramBulkUploadObject> programsToDelete)
        {
            var bulkSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
            var index = BulkUploadMethods.GetIngestDraftTargetIndexName(_GroupId, _BulkUploadId, _DateOfProgramsToIngest);
            var bulkRequests = new List<ESBulkRequestObj<string>>();
            var serializer = new ESSerializerV2();
            var isOpc = GroupSettingsManager.IsOpc(_GroupId);
            var metasToPad = GetMetasToPad(_GroupId);

            var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
            foreach (var program in programTranslationsToIndex)
            {
                program.PadMetas(metasToPad);
                var suffix = program.Language;
                var language = _Languages[program.Language];

                // Serialize EPG object to string
                var serializedEpg = serializer.SerializeEpgObject(program, suffix, isOpc);
                var epgType = GetTanslationType(DEFAULT_INDEX_MAPPING_TYPE, language);

                var totalMinutes = GetTTLMinutes(program);
                // TODO: what should we do if someone trys to ingest something to the past ... :\
                totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

                var ttl = string.Format("{0}m", totalMinutes);

                var bulkRequest = new ESBulkRequestObj<string>()
                {
                    docID = program.EpgID.ToString(),
                    document = serializedEpg,
                    index = index,
                    Operation = eOperation.index,
                    routing = program.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                    type = epgType,
                    ttl = ttl
                };

                bulkRequests.Add(bulkRequest);

                // If we exceeded maximum size of bulk 
                if (bulkRequests.Count >= bulkSize)
                {
                    // create bulk request now and clear list
                    var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            _Logger.Error($"Could not add EPG to ES index. GroupID={_GroupId} epgId={item.Key} error={item.Value}");
                        }
                    }

                    bulkRequests.Clear();
                }
            }

            var programIds = programsToDelete.Select(program => program.EpgId);
            _Logger.Debug($"Update elasticsearch index completed, delteting required docuements. documents.leng:[{programsToDelete.Count}]");
            if (programIds.Any())
            {
                var deleteQuery = GetElasticsearchQueryForEpgIDs(programIds);
                _ElasticSearchClient.DeleteDocsByQuery(index, "", ref deleteQuery);
            }

            // If we have anything left that is less than the size of the bulk
            if (bulkRequests.Count > 0)
            {
                var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var item in invalidResults)
                    {
                        _Logger.Error($"Could not add EPG to ES index. GroupID={_GroupId} epgId={item.Key} error={item.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// TODO: DO THIS
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        private HashSet<string> GetMetasToPad(int groupId)
        {
            return new HashSet<string>();
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
    }
}
