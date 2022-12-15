using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using ElasticSearch.Searcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog
{
    public partial class IndexManagerV2
    {
        private const string EPG_V3_ROLLBACK_REINDEX_SCRIPT_NAME = "epg_v3_reindex_rollback";
        private const string EPG_V3_MIGRATE_REINDEX_SCRIP_NAME = "epg_v3_reindex_migration";
        public void MigrateEpgToV3(int batchSize, EpgFeatureVersion originalEpgVersion)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);

            log.Info($"getting distinct list of channels for migration:{epgAlias}");
            var distinctChannelIds = GetAllEpgChannelIds(epgAlias);

            var newEpgV3IndexName = $"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3";
            log.Info($"EPG v3 creating new index with name:{newEpgV3IndexName}");
            AddEmptyIndex(newEpgV3IndexName, REFRESH_INTERVAL_FOR_EMPTY_EPG_V3_INDEX);
            log.Info($"EPG v3 adding mapping to new index with name:{newEpgV3IndexName}");
            AddEpgMappings(newEpgV3IndexName, EpgFeatureVersion.V3);

            // required to avoid an issue with re-indexing docs with negative ttl value
            bool isReindexSuccess = ReindexEpgDocuments(batchSize, epgAlias, newEpgV3IndexName);
            if (!isReindexSuccess) { throw new Exception("error while trying to reindex"); }

            log.Info("removing alias from existing indices");
            var existingEpgIndices = _elasticSearchApi.ListIndicesByAlias(epgAlias);
            existingEpgIndices.ForEach(i => _elasticSearchApi.RemoveAlias(i.Name, epgAlias));

            log.Info("setting alias to the new epg v3 index");
            _elasticSearchApi.AddAlias(newEpgV3IndexName, epgAlias);

            log.Info("turning on epg v3 feature");
            var epgV3confFeauterEnabled = new EpgV3PartnerConfiguration { IsEpgV3Enabled = true };
            EpgPartnerConfigurationManager.Instance.SetEpgV3Configuration(_partnerId, epgV3confFeauterEnabled);

            log.Info("invalidate epg partner configuration cache");
            var epgV2InvalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(_partnerId);
            var epgV3InvalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(_partnerId);
            _layeredCache.SetInvalidationKey(epgV2InvalidationKey);
            _layeredCache.SetInvalidationKey(epgV3InvalidationKey);

            log.Info("committing channel transaction documents for migration");
            distinctChannelIds.ForEach(epgChannelId => CommitEpgCrudTransaction($"0_{epgChannelId}", epgChannelId));


            log.Info("rebuilding percolators");
            var channelIds = new HashSet<int>();
            if (!isOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);

            var mediaIndex = NamingHelper.GetMediaIndexAlias(_partnerId);

            // insert a dummy transaction child typ to allow unified search to use "commited only" query addition
            var dummyTransactionChildMapping = "{\"_parent\":{\"type\":\"" + NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME + "\", \"fielddata\" : { \"loading\" : \"eager_global_ordinals\" }}}";
            var mappingResult = _elasticSearchApi.InsertMapping(mediaIndex, NamingHelper.EPG_V3_DUMMY_TRANSACTION_CHILD_DOCUMENT_TYPE_NAME, dummyTransactionChildMapping);
            if (!mappingResult) { throw new Exception("Could not create media index mapping of type dummy transaction child doc"); }
            // insert transaction type as dummy to allow unified search to use "commited only" query addition
            mappingResult = _elasticSearchApi.InsertMapping(mediaIndex, NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME, "{\"properties\": {}}");
            if (!mappingResult) { throw new Exception("Could not create media index  mapping of type transaction doc"); }

            var recordingIndex = NamingHelper.GetRecordingIndexAlias(_partnerId);
            mappingResult = _elasticSearchApi.InsertMapping(recordingIndex, NamingHelper.EPG_V3_DUMMY_TRANSACTION_CHILD_DOCUMENT_TYPE_NAME, dummyTransactionChildMapping);
            if (!mappingResult) { throw new Exception("Could not create recording index mapping of type dummy transaction child doc"); }
            // insert transaction type as dummy to allow unified search to use "commited only" query addition
            mappingResult = _elasticSearchApi.InsertMapping(recordingIndex, NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME, "{\"properties\": {}}");
            if (!mappingResult) { throw new Exception("Could not create recording index  mapping of type transaction doc"); }

        }

        private bool ReindexEpgDocuments(int batchSize, string source, string destination)
        {
            var indexExists = _elasticSearchApi.IndexExists(source);
            if (!indexExists)
            {
                log.Info($"source index:[{source}] not found, returning true");
                return true;
            }

            var ttlFilter = new FilteredQuery(true);
            var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, "0");
            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(ttlGtZero);
            ttlFilter.Filter = new QueryFilter() { FilterSettings = filterCompositeType };
            ttlFilter.ReturnFields.Clear();

            log.Info($"starting reindex from:{source} to:{destination}");
            var filterQuery = $"{{ {ttlFilter.Filter} }}";
            var isReindexSuccess = _elasticSearchApi.Reindex(source, destination, filterQuery, EPG_V3_MIGRATE_REINDEX_SCRIP_NAME, batchSize);
            log.Info($"reindex result: [{isReindexSuccess}]");
            return isReindexSuccess;
        }

        public void RollbackEpgV3ToV2(int batchSize)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"getting epg date range:{epgAlias}");

            var (minStart, maxStart) = GetDateRangeAllEpgPrograms(epgAlias);
            log.Info($"extracted minStart:[{minStart}] maxStart:[{maxStart}]");

            var countOfDates = (maxStart.Date - minStart.Date).Days + 1;
            var epgV2IndexDates = Enumerable.Range(0, countOfDates).Select(offset => minStart.AddDays(offset)).ToList();
            var datesToEpgV2IndexNamesMap = epgV2IndexDates.ToDictionary(k => k.Date.Date, v => _namingHelper.GetDailyEpgIndexName(_partnerId, v.Date));
            log.Info($"epg v2 indices to create:[{JsonConvert.SerializeObject(datesToEpgV2IndexNamesMap)}]");
            foreach (var dateIndexNamePair in datesToEpgV2IndexNamesMap)
            {
                var epgDate = dateIndexNamePair.Key;
                var epgV2IndexName = dateIndexNamePair.Value;
                AddEmptyIndex(epgV2IndexName, REFRESH_INTERVAL_FOR_EMPTY_EPG_V2_INDEX);
                AddEpgMappings(epgV2IndexName, EpgFeatureVersion.V2);
                log.Info($"created epg v2 index:[{epgV2IndexName}]");

                // required to avoid an issue with re-indexing docs with negative ttl value

                var reindexV3ToV2Filter = new FilteredQuery(true);

                // all programs that have their start date in the 
                var minStartDate = epgDate.Date;
                var maxEndDate = epgDate.Date.AddDays(1);
                var minimumRange = new ESRange(false, "start_date", eRangeComp.GTE, minStartDate.ToString(ESUtils.ES_DATE_FORMAT));
                var maximumRange = new ESRange(false, "start_date", eRangeComp.LT, maxEndDate.ToString(ESUtils.ES_DATE_FORMAT));
                var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, "0");

                var filterCompositeType = new FilterCompositeType(CutWith.AND);
                filterCompositeType.AddChild(minimumRange);
                filterCompositeType.AddChild(maximumRange);
                filterCompositeType.AddChild(ttlGtZero);

                reindexV3ToV2Filter.Filter = new QueryFilter() { FilterSettings = filterCompositeType };
                reindexV3ToV2Filter.ReturnFields.Clear();



                log.Info($"starting reindex from:{epgAlias} to:{epgV2IndexName}");
                var filterQuery = $"{{ {reindexV3ToV2Filter.Filter} }}";
                var isReindexSuccess = _elasticSearchApi.Reindex(epgAlias, epgV2IndexName, filterQuery, EPG_V3_ROLLBACK_REINDEX_SCRIPT_NAME, batchSize);
                log.Info($"reindex result: [{isReindexSuccess}]");
                if (!isReindexSuccess)
                {
                    throw new Exception("error while trying to reindex");
                }
            }

            log.Info("removing alias from existing indices");
            var existingEpgIndices = _elasticSearchApi.ListIndicesByAlias(epgAlias);
            existingEpgIndices.ForEach(i => _elasticSearchApi.RemoveAlias(i.Name, epgAlias));

            log.Info("setting alias to the new epg v2 indices");
            datesToEpgV2IndexNamesMap.Values.ToList().ForEach(idxName => _elasticSearchApi.AddAlias(idxName, epgAlias));


            log.Info("turning off epg v3 feature");
            var epgV3confFeauterDisabled = new EpgV3PartnerConfiguration { IsEpgV3Enabled = false };
            EpgPartnerConfigurationManager.Instance.SetEpgV3Configuration(_partnerId, epgV3confFeauterDisabled);

            log.Info("invalidate epg partner configuration cache");
            var epgV2InvalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(_partnerId);
            var epgV3InvalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(_partnerId);
            _layeredCache.SetInvalidationKey(epgV2InvalidationKey);
            _layeredCache.SetInvalidationKey(epgV3InvalidationKey);

            log.Info("rebuilding percolators");
            var channelIds = new HashSet<int>();
            if (!isOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);
        }

        public void RollbackEpgV3ToV1(int batchSize)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"getting epg date range:{epgAlias}");


            var epgV1IndexName = SetupEpgIndex(DateTime.UtcNow, isRecording: false);
            log.Info($"created new epg v1 index:{epgV1IndexName}");

            // required to avoid an issue with re-indexing docs with negative ttl value
            var ttlFilter = new FilteredQuery(true);
            var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, "0");
            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(ttlGtZero);
            ttlFilter.Filter = new QueryFilter() { FilterSettings = filterCompositeType };
            ttlFilter.ReturnFields.Clear();

            log.Info($"starting reindex from:{epgAlias} to:{epgV1IndexName}");
            var filterQuery = $"{{ {ttlFilter.Filter} }}";
            var isReindexSuccess = _elasticSearchApi.Reindex(epgAlias, epgV1IndexName, filterQuery, EPG_V3_ROLLBACK_REINDEX_SCRIPT_NAME, batchSize);
            log.Info($"reindex result: [{isReindexSuccess}]");
            if (!isReindexSuccess)
            {
                throw new Exception("error while trying to reindex");
            }

            log.Info($"publishing epg v1 index:{epgV1IndexName}");
            PublishEpgIndex(epgV1IndexName, isRecording: false, shouldSwitchIndexAlias: true, shouldDeleteOldIndices: false);

            log.Info("turning off epg v3 feature");
            var epgV3confFeauterDisabled = new EpgV3PartnerConfiguration { IsEpgV3Enabled = false };
            EpgPartnerConfigurationManager.Instance.SetEpgV3Configuration(_partnerId, epgV3confFeauterDisabled);

            log.Info("invalidate epg partner configuration cache");
            var epgV2InvalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(_partnerId);
            var epgV3InvalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(_partnerId);
            _layeredCache.SetInvalidationKey(epgV2InvalidationKey);
            _layeredCache.SetInvalidationKey(epgV3InvalidationKey);

            log.Info("rebuilding percolators");
            var channelIds = new HashSet<int>();
            if (!isOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);
        }

        private List<int> GetAllEpgChannelIds(string indexName)
        {
            var indexExists = _elasticSearchApi.IndexExists(indexName);
            if (!indexExists)
            {
                log.Info($"index:[{indexName}] not found, returning empty channelIds list");
                return new List<int>();
            }
            var channelIdAgg = new ESBaseAggsItem()
            {
                Field = "epg_channel_id",
                Name = "channels",
                Type = eElasticAggregationType.terms
            };

            var query = new FilteredQuery
            {
                ZeroSize = true,
                Query = new ESMatchAllQuery()
            };

            query.Aggregations.Add(channelIdAgg);
            var queryStr = query.ToString();

            var aggResult = _elasticSearchApi.Search(indexName, "", ref queryStr);
            var aggJObject = JObject.Parse(aggResult);

            var channelIds = aggJObject["aggregations"][channelIdAgg.Name]["buckets"].Select(b => b["key"].Value<int>());

            return channelIds.ToList();
        }

        private (DateTimeOffset minStart, DateTimeOffset maxStart) GetDateRangeAllEpgPrograms(string indexName)
        {
            var maxStartDateAgg = new ESBaseAggsItem()
            {
                Field = "start_date",
                Name = "max_start",
                Type = eElasticAggregationType.max
            };

            var minStartDateAgg = new ESBaseAggsItem()
            {
                Field = "start_date",
                Name = "min_start",
                Type = eElasticAggregationType.min,
            };

            var query = new FilteredQuery
            {
                ZeroSize = true,
                Query = new ESRange(false, "start_date", eRangeComp.GT, DateTime.UtcNow.AddDays(-60).ToESDateFormat())
            };

            query.Aggregations.Add(maxStartDateAgg);
            query.Aggregations.Add(minStartDateAgg);

            var queryStr = query.ToString();

            var aggResult = _elasticSearchApi.Search(indexName, "", ref queryStr);
            var aggJObject = JObject.Parse(aggResult);

            var minStart = aggJObject["aggregations"][minStartDateAgg.Name].Value<long?>("value");
            var maxEnd = aggJObject["aggregations"][maxStartDateAgg.Name].Value<long?>("value");

            if (!minStart.HasValue || !maxEnd.HasValue)
            {
                log.Warn($"could not calculate min and max start dates, assuming index is empty, returning zero dattimes");
                return (new DateTimeOffset(), new DateTimeOffset());
            }

            var minStartDate = DateTimeOffset.FromUnixTimeMilliseconds(minStart.Value);
            var maxStartDate = DateTimeOffset.FromUnixTimeMilliseconds(maxEnd.Value);
            return (minStartDate, maxStartDate);
        }
    }
}
