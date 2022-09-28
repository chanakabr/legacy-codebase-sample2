using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.NestData;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using ElasticSearch.Searcher;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog
{
    public partial class IndexManagerV7
    {
        private const string EPG_V3_REINDEX_MIGRATION_PAINLESS_SCRIPT = @"
ctx._source['__documentTransactionalStatus']='INSERTING';
ctx._routing = ctx._source['epg_channel_id'];
def parentId = '0_'+ ctx._source['epg_channel_id'];
def transactionDoc = ['name':'epg', 'parent':parentId];
ctx._source['transaction'] = transactionDoc;
";
        private const string EPG_V3_REINDEX_ROLLBACK_PAINLESS_SCRIPT = @"
ctx._source.remove('__documentTransactionalStatus');
ctx._routing = ctx._source['date_routing'];
ctx._source.remove('transaction');

";
        public void MigrateEpgToV3(int batchSize, EpgFeatureVersion originalEpgVersion)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"getting distinct list of channels for migration:{epgAlias}");
            var distinctChannelIds = GetAllEpgChannelIds(epgAlias);

            var newEpgV3IndexName = $"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            log.Info($"EPG v3 creating new index and mappings with name:{newEpgV3IndexName}");
            AddEmptyIndex(newEpgV3IndexName, EpgFeatureVersion.V3);

            var res = ReindexEpgDocuments(epgAlias, newEpgV3IndexName);
            if (!res) { throw new Exception($"error while trying to migrate indices for partner:[{_partnerId}]"); }

            log.Info("removing alias from existing indices");
            var removeAliasResp = _elasticClient.Indices.BulkAlias(alias => alias.Remove(a => a.Alias(epgAlias).Index("*")));
            if (!removeAliasResp.IsValid) { throw new Exception($"error while trying to remove epg alias from current indices. {removeAliasResp.DebugInformation}"); }

            log.Info("setting alias to the new epg v3 index");
            var putaliasResp = _elasticClient.Indices.PutAlias(newEpgV3IndexName, epgAlias);
            if (!putaliasResp.IsValid) { throw new Exception($"error while trying to put alias to new epg v3 index. {putaliasResp.DebugInformation}"); }

            log.Info("committing channel transaction documents for migration");
            distinctChannelIds.ForEach(epgChannelId => CommitEpgCrudTransaction($"0_{epgChannelId}", int.Parse(epgChannelId)));
            log.Info("turning on epg v3 feature");
            var epgV3confFeauterEnabled = new EpgV3PartnerConfiguration { IsEpgV3Enabled = true };
            EpgPartnerConfigurationManager.Instance.SetEpgV3Configuration(_partnerId, epgV3confFeauterEnabled);


            log.Info("invalidate epg partner configuration cache");
            var epgV2InvalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(_partnerId);
            var epgV3InvalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(_partnerId);
            _layeredCache.SetInvalidationKey(epgV2InvalidationKey);
            _layeredCache.SetInvalidationKey(epgV3InvalidationKey);

            log.Info("rebuilding percolators");
            var channelIds = new HashSet<int>();
            if (!IsOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);

        }

        private bool ReindexEpgDocuments(string epgAlias, string newEpgV3IndexName)
        {
            var isIndexExist = _elasticClient.Indices.Exists(epgAlias);
            if (isIndexExist != null && !isIndexExist.Exists)
            {
                log.Info($"index: {epgAlias} does not exist, skipping reindex, returning true");
                return true;
            }

            var res = _elasticClient.ReindexOnServer(r => r
               .Source(s => s.Index(epgAlias))
               .Destination(d => d.Index(newEpgV3IndexName))
               .Script(s => s.Source(EPG_V3_REINDEX_MIGRATION_PAINLESS_SCRIPT))
               .WaitForCompletion());

            if (!res.IsValid) { log.Error($"error while trying to migrate indices for partner:[{_partnerId}], debug info:[{res.DebugInformation}]"); }

            return res.IsValid;
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
                AddEmptyIndex(epgV2IndexName, EpgFeatureVersion.V2);
                log.Info($"created epg v2 index:[{epgV2IndexName}]");

                // required to avoid an issue with re-indexing docs with negative ttl value
                var reindexV3ToV2Filter = new FilteredQuery(true);

                // all programs that have their start date in the 
                var minStartDate = epgDate.Date;
                var maxEndDate = epgDate.Date.AddDays(1);


                log.Info($"starting reindex from:{epgAlias} to:{epgV2IndexName}");
                var res = _elasticClient.ReindexOnServer(r => r
                   .Source(s => s
                       .Index(epgAlias)
                       .Query<NestEpg>(q => q
                            .DateRange(range => range.Field(f => f.StartDate)
                                .GreaterThanOrEquals(minStartDate)
                                .LessThan(maxEndDate)
                            )
                       )
                   )
                   .Destination(d => d.Index(epgV2IndexName))
                   .Script(s => s.Source(EPG_V3_REINDEX_ROLLBACK_PAINLESS_SCRIPT))
                   .WaitForCompletion());

                if (!res.IsValid) { throw new Exception($"error while trying to rollback indices for partner:[{_partnerId}], debug info:[{res.DebugInformation}]"); }

            }

            log.Info("removing alias from existing indices");
            var removeAliasResp = _elasticClient.Indices.BulkAlias(alias => alias.Remove(a => a.Alias(epgAlias).Index("*")));
            if (!removeAliasResp.IsValid) { throw new Exception($"error while trying to remove epg alias from current indices. {removeAliasResp.DebugInformation}"); }

            log.Info("setting alias to the new epg v2 indices");

            datesToEpgV2IndexNamesMap.Values.ToList().ForEach(idxName => {
                var putaliasResp = _elasticClient.Indices.PutAlias(idxName, epgAlias);
                if (!putaliasResp.IsValid) { throw new Exception($"error while trying to put alias to new epg v3 index. {putaliasResp.DebugInformation}"); }
            });


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
            if (!IsOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);
        }

        public void RollbackEpgV3ToV1(int batchSize)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);
            log.Info($"getting epg date range:{epgAlias}");


            var epgV1IndexName = SetupEpgIndex(DateTime.UtcNow, isRecording: false);
            log.Info($"created new epg v1 index:{epgV1IndexName}");

            var res = _elasticClient.ReindexOnServer(r => r
               .Source(s => s.Index(epgAlias))
               .Destination(d => d.Index(epgV1IndexName))
               .Script(s => s.Source(EPG_V3_REINDEX_ROLLBACK_PAINLESS_SCRIPT))
               .WaitForCompletion());

            if (!res.IsValid) { throw new Exception($"error while trying to rollback indices for partner:[{_partnerId}], debug info:[{res.DebugInformation}]"); }


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
            if (!IsOpc()) { channelIds = GetGroupManager().channelIDs; }
            AddChannelsPercolatorsToIndex(channelIds, null);
        }


        private List<string> GetAllEpgChannelIds(string epgAlias)
        {
            var isIndexExist = _elasticClient.Indices.Exists(epgAlias);
            if (isIndexExist != null && !isIndexExist.Exists)
            {
                log.Info($"index: {epgAlias} does not exist, returning empty list");
                return new List<string>();
            }

            const string CHANNEL_AGG_KEY = "channels";
            var searchResult = _elasticClient.Search<NestEpg>(s => s
                .Index(epgAlias)
                .Query(q =>
                    q.MatchAll()
                )
                .Aggregations(a =>
                    a.Terms(CHANNEL_AGG_KEY, terms => terms
                    .Field(field => field.ChannelID)))
            );

            if (!searchResult.IsValid) { throw new Exception("failed search for distinct channel ids"); }

            var channelIds = searchResult.Aggregations.Terms(CHANNEL_AGG_KEY).Buckets.Select(b => b.Key).ToList();
            return channelIds;
        }

        private (DateTimeOffset minStart, DateTimeOffset maxStart) GetDateRangeAllEpgPrograms(string epgAlias)
        {
            const string MAX_START_AGG_KEY = "max_start";
            const string MIN_START_AGG_KEY = "min_start";

            var searchResult = _elasticClient.Search<NestEpg>(s => s
                .Index(epgAlias)
                .Query(q =>
                    q.MatchAll()
                )
                .Aggregations(a =>
                    a
                    .Max(MAX_START_AGG_KEY, m => m.Field(field => field.StartDate))
                    .Min(MIN_START_AGG_KEY, m => m.Field(field => field.StartDate))
                )
            );

            if (!searchResult.IsValid) { throw new Exception("failed search for min / max start dates"); }

            var maxStartStr = searchResult.Aggregations.Max(MAX_START_AGG_KEY).ValueAsString;
            var minStartStr = searchResult.Aggregations.Min(MIN_START_AGG_KEY).ValueAsString;
            if (string.IsNullOrEmpty(minStartStr) || string.IsNullOrEmpty(maxStartStr))
            {
                log.Warn($"could not calculate min and max start dates, assuming index is empty, returning zero dattimes");
                return (new DateTimeOffset(), new DateTimeOffset());
            }
            var maxStart = DateTimeOffset.Parse(maxStartStr);
            var minStart = DateTimeOffset.Parse(minStartStr);
            return (minStart, maxStart);
        }
    }
}
