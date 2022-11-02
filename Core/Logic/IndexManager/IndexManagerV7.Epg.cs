using ApiLogic.Api.Managers;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.Transaction;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using BoolQuery = Nest.BoolQuery;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog
{
    // TODO: For now it contains new methods of epg v3 but before merge we will move all epg related methods here...
    public partial class IndexManagerV7
    {
        const string EPG_V3_CLEANUP_QUERY = "{\"query\":{\"bool\":{\"should\":[{\"bool\":{\"must\":[{\"has_parent\":{\"parent_type\":\""+ NESTEpgTransaction.RELATION_NAME + "\",\"query\":{\"match_all\":{}}}},{\"term\":{\""+ ESUtils.ES_DOCUMENT_TRANSACTIONAL_STATUS_FIELD_NAME+"\":\"DELETING\"}}]}},{\"bool\":{\"must\":[{\"match\":{\"transaction\":\""+NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME+"\"}},{\"bool\":{\"must_not\":{\"has_child\":{\"type\":[\""+ NestEpg.RELATION_NAME + "\"],\"query\":{\"match_all\":{}}}}}}]}}]}}}";
        public void SetupEpgV3Index()
        {
            var aliasName = NamingHelper.GetEpgIndexAlias(_partnerId);
            var isIndexExist = _elasticClient.Indices.Exists(aliasName);
            if (isIndexExist?.Exists != true)
            {
                var indexName = $"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3";
                log.Info($"EPG v3 creating new index with name:{indexName}");
                AddEmptyIndex(indexName, EpgFeatureVersion.V3);
                AddEpgIndexAlias(indexName, isWriteIndex:true);
            }
        }

        public void ApplyEpgCrudOperationWithTransaction(string transactionId, List<EpgCB> programsToIndex, List<EpgCB> programsToDelete)
        {
            if (!programsToIndex.Any() && !programsToDelete.Any()) { return; }

            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            var bulkSize = GetBulkSize();
            var languages = GetLanguages().ToDictionary(k => k.Code);

            var policy = IndexManagerCommonHelpers.GetRetryPolicy<Exception>(5);
            var bulkRequests = new List<NestEsBulkRequest<NestEpg>>();

            policy.Execute(() =>
            {
                try
                {
                    foreach (var prog in programsToIndex)
                    {
                        var nestEsBulkRequest = MapEpgCbToEpgV3EsNestBulkRequest(transactionId, epgAlias, languages, eTransactionOperation.INSERTING, prog);
                        bulkRequests.Add(nestEsBulkRequest);

                        if (bulkRequests.Count >= bulkSize)
                        {
                            // exec and validate also clears the bulkRequests list
                            _ = ExecuteAndValidateBulkRequests(bulkRequests);
                        }
                    }


                    foreach (var prog in programsToDelete)
                    {
                        var nestEsBulkRequest = MapEpgCbToEpgV3EsNestBulkRequest(transactionId, epgAlias, languages, eTransactionOperation.DELETING, prog);
                        bulkRequests.Add(nestEsBulkRequest);

                        if (bulkRequests.Count >= bulkSize)
                        {
                            // exec and validate also clears the bulkRequests list
                            _ = ExecuteAndValidateBulkRequests(bulkRequests);
                        }
                    }

                    if (bulkRequests.Any())
                    {
                        // exec and validate also clears the bulkRequests list
                        _ = ExecuteAndValidateBulkRequests(bulkRequests);
                    }
                }
                finally
                {
                    if (bulkRequests.Any())
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
            var transactionDoc = new NESTEpgTransaction { Transaction = JoinField.Root<NESTEpgTransaction>() };
            var indexResp = _elasticClient.Index(transactionDoc, i => i.Index(epgIndex).Id(transactionId).Routing(linearChannelId));
            if (!indexResp.IsValid) { throw new Exception($"error while indexing transaction document {transactionId}, channel:{linearChannelId}, err:{indexResp.ServerError}"); }
        }

        public void CleanupEpgV3Index()
        {
            var epgIndex = NamingHelper.GetEpgIndexAlias(_partnerId);
            var deleteResponse = _elasticClient.DeleteByQuery<NestTag>(request => request
                .Index(epgIndex)
                .Query(q => q.Raw(EPG_V3_CLEANUP_QUERY)));

            if (!deleteResponse.IsValid)
            {
                log.Error($"Failed performing delete query {deleteResponse.DebugInformation}");
            }
        }

        private NestEsBulkRequest<NestEpg> MapEpgCbToEpgV3EsNestBulkRequest(string transactionId, string epgAlias, Dictionary<string, LanguageObj> languages, eTransactionOperation transactionOperation, EpgCB prog)
        {
            var expiry = GetEpgExpiry(prog);
            var progLang = languages[prog.Language];
            var epg = NestDataCreator.GetEpg(prog, progLang.ID, withRouting: true, IsOpc(), expiry);
            epg.DocumentId = prog.DocumentId;
            epg.DocumentTransactionalStatus = transactionOperation.ToString();
            epg.Transaction = JoinField.Link<NestEpg>(transactionId);

            var nestEsBulkRequest = GetEpgBulkRequest(epgAlias, epg);
            nestEsBulkRequest.Routing = epg.ChannelID.ToString();
            return nestEsBulkRequest;
        }

        private QueryContainer WrapFilterWithCommittedOnlyTransactionsForEpgV3(string epgV3IndexName, QueryContainer originalQuery)
        {
            var epgIndexTerm = new TermQuery { Field = "_index", Value = epgV3IndexName };
            var hasTransactionParentDocument = new HasParentQuery
            {
                ParentType = Infer.Relation<NESTEpgTransaction>(),
                Query = new MatchAllQuery(),
            };

            var insertingStatusTerm = new TermQuery
            {
                Field = Infer.Field<NestEpg>(f => f.DocumentTransactionalStatus),
                Value = eTransactionOperation.INSERTING.ToString(),
            };

            var deletingStatusTerm = new TermQuery
            {
                Field = Infer.Field<NestEpg>(f => f.DocumentTransactionalStatus),
                Value = eTransactionOperation.DELETING.ToString(),
            };

            var insertingAndCommitted = insertingStatusTerm & hasTransactionParentDocument;
            var deletingAndNotCommitted = deletingStatusTerm & !hasTransactionParentDocument;
            var allCommittedDocuments = insertingAndCommitted | deletingAndNotCommitted;
            var epgv3CommittedOnly = allCommittedDocuments & epgIndexTerm;
            var epgV3Final = epgv3CommittedOnly | !epgIndexTerm;
            return epgV3Final & originalQuery;
        }

        private QueryContainer WrapQueryIfEpgV3Feature(QueryContainer query)
        {
            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(_partnerId);
            if (epgFeatureVersion == EpgFeatureVersion.V3)
            {
                var epgV3IndexName = GetEpgV3IndexName();
                query = WrapFilterWithCommittedOnlyTransactionsForEpgV3(epgV3IndexName, query);
            }

            return query;
        }

        private string GetEpgV3IndexName()
        {
            var epgV3IndexNameKey = $"{_partnerId}_epg_v3_index_name_cache_key";
            var epgV3IndexName = MemoryCache.Default.Get(epgV3IndexNameKey)?.ToString();
            if (string.IsNullOrEmpty(epgV3IndexName))
            {
                var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
                var epgAliasIndices = _elasticClient.GetIndicesPointingToAlias(epgAlias);
                epgV3IndexName = epgAliasIndices.First();
                MemoryCache.Default.Set(
                    new CacheItem(epgV3IndexNameKey, epgV3IndexName),
                    new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60) }
                );
            }

            return epgV3IndexName;
        }
    }
}
