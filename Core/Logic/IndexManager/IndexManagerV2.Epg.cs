using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Transaction;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog
{
    // TODO: For now it contains new methods of epg v3 but before merge we will move all epg related methods here...
    public partial class IndexManagerV2
    {
        public void SetupEpgV3Index()
        {
            var aliasName = NamingHelper.GetEpgIndexAlias(_partnerId);
            var indexName = $"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3";
            var epgIndices = _elasticSearchApi.ListIndicesByAlias(aliasName);

            // if the epg alias already holds an index we should not setup a new index
            if (epgIndices.Count == 0)
            {
                log.Info($"EPG v3 creating new index with name:{indexName}");
                AddEmptyIndex(indexName);
                log.Info($"EPG v3 adding mapping to new index with name:{indexName}");
                AddEpgMappings(indexName, EpgFeatureVersion.V3);
                log.Info($"EPG v3 adding alias to new index with name:{indexName}");
                AddEpgAlias(indexName);
            }
        }

        public void ApplyEpgCrudOperationWithTransaction(string transactionId, List<EpgCB> programsToIndex, List<EpgCB> programsToDelete)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            var bulkSize = GetBulkSizeForUpsertPrograms();
            var policy = GetRetryPolicyForUpsertPrograms();
            var metasToPad = GetMetasToPad();

            var languages = GetLanguages().ToDictionary(k => k.Code);
            var defaultLanguage = GetDefaultLanguage();

            policy.Execute(() =>
            {
                var bulkRequests = new List<ESBulkRequestObj<string>>();
                try
                {
                    foreach (var prog in programsToIndex)
                    {
                        var bulkRequest = MapEpgCBToEsTransactionBulkRequest(transactionId, eTransactionOperation.INSERTING, prog, epgAlias, languages, defaultLanguage, metasToPad);
                        bulkRequests.Add(bulkRequest);
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                            bulkRequests.Clear();
                        }
                    }
                    
                    foreach (var prog in programsToDelete)
                    {
                        var bulkRequest = MapEpgCBToEsTransactionBulkRequest(transactionId, eTransactionOperation.DELETING, prog, epgAlias, languages, defaultLanguage, metasToPad);
                        bulkRequests.Add(bulkRequest);
                        if (bulkRequests.Count >= bulkSize)
                        {
                            ExecuteAndValidateBulkRequests(bulkRequests);
                            bulkRequests.Clear();
                        }
                    }

                    // If we have anything left that is less than the size of the bulk
                    if (bulkRequests.Any())
                    {
                        ExecuteAndValidateBulkRequests(bulkRequests);
                        bulkRequests.Clear();
                    }
                }
                finally
                {
                    if (bulkRequests != null && bulkRequests.Any())
                    {
                        log.Debug($"Clearing bulk requests");
                        bulkRequests.Clear();
                    }
                }
            });

        }

        public void CommitEpgCrudTransaction(string transactionId, long linearChannelId)
        {
            var epgIndex = NamingHelper.GetEpgIndexAlias(_partnerId);
            _elasticSearchApi.InsertRecord(epgIndex, NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME, transactionId, "{}", linearChannelId.ToString());
        }

        private ESBulkRequestObj<string> MapEpgCBToEsTransactionBulkRequest(string transactionId, eTransactionOperation transactionOperation, EpgCB program, string indexName, IDictionary<string, LanguageObj> languages, LanguageObj defaultLanguage, HashSet<string> metasToPad)
        {
            program.PadMetas(metasToPad);
            var suffix = program.Language == defaultLanguage.Code ? "" : program.Language;
            var language = languages[program.Language];

            // Serialize EPG object to string
            string serializedEpg = TryGetSerializedEpg(isOpc(), program, suffix, transactionOperation);
            var epgType = GetTranslationType(IndexManagerV2.EPG_INDEX_TYPE, language);

            var totalMinutes = _ttlService.GetEpgTtlMinutes(program);
            totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

            var bulkRequest = new ESBulkRequestObj<string>()
            {
                docID = program.DocumentId,
                ParentDocumentID = transactionId,
                document = serializedEpg,
                index = indexName,
                Operation = eOperation.index,
                routing = program.ChannelID.ToString(),
                type = epgType,
                ttl = $"{totalMinutes}m"
            };
            return bulkRequest;
        }
    }
}
