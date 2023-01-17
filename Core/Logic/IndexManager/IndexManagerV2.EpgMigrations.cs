using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ElasticSearch.Searcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Catalog.Searchers;
using Phx.Lib.Appconfig;
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
            var dummyTransactionChildMapping = "{\"_parent\":{\"type\":\"" + NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME + "\"}}";
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
            var ttlValue = DateTime.UtcNow.ToUtcUnixTimestampMilliseconds() + ApplicationConfiguration.Current.ElasticSearchHttpClientConfiguration.TimeOutInMiliSeconds.Value;
            var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, $"{ttlValue}");
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
                var epgV2IndexName = dateIndexNamePair.Value;

                #region Backing up indices and create new one for reindexing

                if (_elasticSearchApi.IndexExists(epgV2IndexName))
                {
                    var backupEpgV2IndexName = $"backup_{epgV2IndexName}";
                    AddEmptyIndex(backupEpgV2IndexName, REFRESH_INTERVAL_FOR_EMPTY_EPG_V2_INDEX);
                    AddEpgMappings(backupEpgV2IndexName, EpgFeatureVersion.V2);

                    var isBackupReindexSuccess = _elasticSearchApi.Reindex(epgV2IndexName, backupEpgV2IndexName, batchSize: batchSize);
                    if (isBackupReindexSuccess)
                    {
                        var originalEpgV2IndexDeleteResult = _elasticSearchApi.DeleteIndices(new List<string>
                        {
                            epgV2IndexName
                        });
                        if (!originalEpgV2IndexDeleteResult)
                        {
                            log.Warn($"Can not delete original index for EPGv2 ({epgV2IndexName})");
                        }
                    }
                    else
                    {
                        log.Warn($"There was a problem while backing up {epgV2IndexName} to {backupEpgV2IndexName}.");
                    }
                }
                else
                {
                    log.Warn($"There is nothing to backup, original EPGv2 index {epgV2IndexName} does not exist.");
                }

                #endregion

                var epgDate = dateIndexNamePair.Key;
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
                var ttlValue = DateTime.UtcNow.ToUtcUnixTimestampMilliseconds() + ApplicationConfiguration.Current.ElasticSearchHttpClientConfiguration.TimeOutInMiliSeconds.Value;
                var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, $"{ttlValue}");

                var filterCompositeType = new FilterCompositeType(CutWith.AND);
                filterCompositeType.AddChild(minimumRange);
                filterCompositeType.AddChild(maximumRange);
                filterCompositeType.AddChild(ttlGtZero);

                var generalFilter = new QueryFilter() { FilterSettings = filterCompositeType };
                Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, generalFilter, _elasticSearchApi);

                reindexV3ToV2Filter.Filter = generalFilter;
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

            #region Backup EPGv3 index

            log.Info($"Backing up EPGv3 indices");
            var backupEpgV3Index = $"backup_{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            AddEmptyIndex(backupEpgV3Index, REFRESH_INTERVAL_FOR_EMPTY_EPG_V3_INDEX);
            AddEpgMappings(backupEpgV3Index, EpgFeatureVersion.V3);

            var backupReindexResult = _elasticSearchApi.Reindex($"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3", backupEpgV3Index, batchSize: batchSize);
            if (!backupReindexResult)
            {
                log.Warn($"There was a problem while backing up EPGv3 index ({NamingHelper.GetEpgIndexAlias(_partnerId)}_v3) to ({backupEpgV3Index}).");
            }

            log.Info($"Removing EPGv3 indices.");
            var deleteIndicesResult= _elasticSearchApi.DeleteIndices(new List<string> {$"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3"});
            if (!deleteIndicesResult)
            {
                log.Error($"Can not remove EPGv3 indices ({NamingHelper.GetEpgIndexAlias(_partnerId)}_v3) for partner - {_partnerId}!");
                return;
            }

            #endregion

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
            var ttlValue = DateTime.UtcNow.ToUtcUnixTimestampMilliseconds() + ApplicationConfiguration.Current.ElasticSearchHttpClientConfiguration.TimeOutInMiliSeconds.Value;
            var ttlGtZero = new ESRange(true, "_ttl", eRangeComp.GT, $"{ttlValue}");
            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(ttlGtZero);
            var generalFilter = new QueryFilter() { FilterSettings = filterCompositeType };
            Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(_partnerId, generalFilter, _elasticSearchApi);

            log.Info($"starting reindex from:{epgAlias} to:{epgV1IndexName}");

            ttlFilter.Filter = generalFilter;
            ttlFilter.ReturnFields.Clear();

            var filterQuery = $"{{ {ttlFilter.Filter} }}";

            var isReindexSuccess = _elasticSearchApi.Reindex(epgAlias, epgV1IndexName, filterQuery, EPG_V3_ROLLBACK_REINDEX_SCRIPT_NAME, batchSize);
            log.Info($"reindex result: [{isReindexSuccess}]");
            if (!isReindexSuccess)
            {
                throw new Exception("error while trying to reindex");
            }

            log.Info($"publishing epg v1 index:{epgV1IndexName}");
            PublishEpgIndex(epgV1IndexName, isRecording: false, shouldSwitchIndexAlias: true, shouldDeleteOldIndices: false);

            #region Backup EPGv3 index

            log.Info($"Backing up EPGv3 indices");
            var backupEpgV3Index = $"backup_{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            AddEmptyIndex(backupEpgV3Index, REFRESH_INTERVAL_FOR_EMPTY_EPG_V3_INDEX);
            AddEpgMappings(backupEpgV3Index, EpgFeatureVersion.V3);
            var backupReindexResult = _elasticSearchApi.Reindex($"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3", backupEpgV3Index, batchSize: batchSize);
            if (!backupReindexResult)
            {
                log.Warn($"There was a problem while backing up EPGv3 index ({NamingHelper.GetEpgIndexAlias(_partnerId)}_v3) to ({backupEpgV3Index}).");
            }

            log.Info($"Removing EPGv3 indices.");
            var deleteIndicesResult= _elasticSearchApi.DeleteIndices(new List<string> {$"{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3"});
            if (!deleteIndicesResult)
            {
                log.Error($"Can not remove EPGv3 indices ({NamingHelper.GetEpgIndexAlias(_partnerId)}_v3) for partner - {_partnerId}!");
                return;
            }

            #endregion

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

        public void RollbackEpgV3ToV1WithoutReindexing(bool rollbackFromBackup, int batchSize)
        {
            // Predicate to extract only indices for EPGv1.
            bool OriginalIndicesPredicate(ESIndex i) => !i.Name.Contains("epg_v2") && !i.Name.Contains("epg_v3");
            RollbackEpgV3WithoutReindexingGeneric(OriginalIndicesPredicate, ExtractLatestEpgV1Index, rollbackFromBackup, batchSize);
        }

        public void RollbackEpgV3ToV2WithoutReindexing(bool rollbackFromBackup, int batchSize)
        {
            // Predicate to extract only indices for EPGv2.
            bool OriginalIndicesPredicate(ESIndex i) => i.Name.Contains("epg_v2");
            RollbackEpgV3WithoutReindexingGeneric(OriginalIndicesPredicate, indices => indices.Select(x => x.Name).ToArray(), rollbackFromBackup, batchSize);
        }

        private void RollbackEpgV3WithoutReindexingGeneric(Func<ESIndex, bool> originalIndicesPredicate, Func<ESIndex[], string[]> extractEpgIndices, bool rollbackFromBackup, int batchSize)
        {
            var epgAlias = NamingHelper.GetEpgIndexAlias(_partnerId);

            log.Info($"Retrieving original EPGv1/EPGv2 indices for partner {_partnerId}.");
            var partnerEpgIndices = _elasticSearchApi.ListIndices($"{(rollbackFromBackup ? "*": "")}{_partnerId}_epg_*");
            // This code is there just to avoid messing up with EPGv1 and EPGv2 indices for the same customer and acquiring the correct original indices.
            var originalIndicesPredicateExtended = rollbackFromBackup ?
                index => originalIndicesPredicate(index) && index.Name.Contains("backup") :
                originalIndicesPredicate;
            var partnerEpgOriginalIndicesForRollback = partnerEpgIndices.Where(originalIndicesPredicateExtended).ToArray();

            if (!partnerEpgOriginalIndicesForRollback.Any())
            {
                log.Error($"There are no indices to rollback for customer {_partnerId}!");
                return;
            }

            log.Info($"Retrieving current EPGv3/EPGv2 indices by alias ({epgAlias}) for partner {_partnerId}.");
            var partnerCurrentEpgV3OrEpgV2Indices = _elasticSearchApi.ListIndicesByAlias(epgAlias);
            if (!partnerCurrentEpgV3OrEpgV2Indices.Any())
            {
                log.Error($"There are no indices to rollback for customer {_partnerId}!");
                return;
            }

            // Extract the latest original EPGv1/EPGv2 index (if there are more than one).
            var originalEpgIndices = extractEpgIndices(partnerEpgOriginalIndicesForRollback);
            if (!originalEpgIndices.Any())
            {
                log.Error($"Can not find original EPGv1/EPGv2 index for partner {_partnerId}!");
                return;
            }

            if (rollbackFromBackup)
            {
                #region Restore from backup indices

                var createdIndices = new List<string>();
                foreach (var partnerEpgOriginalIndexForRollback in partnerEpgOriginalIndicesForRollback)
                {
                    // cut backup_ prefix
                    var currentEpgIndexName = partnerEpgOriginalIndexForRollback.Name.Substring(7, partnerEpgOriginalIndexForRollback.Name.Length - 7);
                    if (_elasticSearchApi.IndexExists(currentEpgIndexName))
                    {
                        var originalEpgV2IndexDeleteResult = _elasticSearchApi.DeleteIndices(new List<string>
                        {
                            currentEpgIndexName
                        });
                        if (!originalEpgV2IndexDeleteResult)
                        {
                            log.Warn($"Can not delete original index for EPGv2 ({currentEpgIndexName})");
                        }
                    }

                    AddEmptyIndex(currentEpgIndexName, REFRESH_INTERVAL_FOR_EMPTY_EPG_V2_INDEX);
                    AddEpgMappings(currentEpgIndexName, EpgFeatureVersion.V2);

                    var isBackupReindexSuccess = _elasticSearchApi.Reindex(partnerEpgOriginalIndexForRollback.Name, currentEpgIndexName, batchSize: batchSize);
                    if (!isBackupReindexSuccess)
                    {
                        log.Warn($"There was a problem while backing up {partnerEpgOriginalIndexForRollback.Name} to {currentEpgIndexName}.");
                        continue;
                    }

                    createdIndices.Add(currentEpgIndexName);
                }

                #endregion

                // in case backup indices count are less than current indices, we should remove the rest
                var partnerCurrentIndicesToDelete = partnerCurrentEpgV3OrEpgV2Indices.Select(x => x.Name).Except(createdIndices).ToArray();
                if (partnerCurrentIndicesToDelete.Any())
                {
                    _elasticSearchApi.DeleteIndices(partnerCurrentIndicesToDelete.Select(x => x).ToList());
                }

                // original indices to backup should be reassigned once we realize them now
                originalEpgIndices = createdIndices.ToArray();
            }

            var currentEpgIndicesNames = string.Join(",", partnerCurrentEpgV3OrEpgV2Indices.Select(x => x.Name).ToArray());
            // in case we're rolling back, removing all current indices will clean the alias already
            if (!rollbackFromBackup)
            {
                log.Info($"Clear current alias from indices ({currentEpgIndicesNames}).");
                foreach (var partnerEpgV3Index in partnerCurrentEpgV3OrEpgV2Indices)
                {
                    var result = _elasticSearchApi.RemoveAlias(partnerEpgV3Index.Name, epgAlias);
                    if (!result)
                    {
                        log.Warn($"Error while deleting index ({partnerEpgV3Index.Name}) from alias ({epgAlias})!");
                    }
                }
            }

            var originalEpgIndicesNames = string.Join(",", originalEpgIndices);
            log.Info($"Adding original EPGv1/EPGv2 index/indices ({originalEpgIndicesNames}) to alias ({epgAlias}).");
            foreach (var originalEpgIndex in originalEpgIndices)
            {
                var aliasAddResult = _elasticSearchApi.AddAlias(originalEpgIndex, epgAlias);
                if (!aliasAddResult)
                {
                    log.Error($"Can not set alias ({epgAlias}) based on original EPGv1 index ({originalEpgIndicesNames}) for partner - {_partnerId}!");
                    return;
                }
            }

            // we've already deleted indexes in case of rolling back to backup
            if (!rollbackFromBackup)
            {
                #region Backup EPGv3 index

                log.Info($"Backing up EPGv3 indices ({currentEpgIndicesNames})");
                var backupEpgV3Index = $"backup_{NamingHelper.GetEpgIndexAlias(_partnerId)}_v3_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
                AddEmptyIndex(backupEpgV3Index, REFRESH_INTERVAL_FOR_EMPTY_EPG_V3_INDEX);
                AddEpgMappings(backupEpgV3Index, EpgFeatureVersion.V3);
                var epgV3Index = partnerCurrentEpgV3OrEpgV2Indices.FirstOrDefault(x => x.Name.Contains("v3"));
                var backupReindexResult = _elasticSearchApi.Reindex(epgV3Index.Name, backupEpgV3Index, batchSize: batchSize);
                if (!backupReindexResult)
                {
                    log.Warn($"There was a problem while backing up EPGv3 index ({epgV3Index.Name}) to ({backupEpgV3Index}).");
                }

                log.Info($"Removing EPGv3 indices ({currentEpgIndicesNames})");
                var deleteIndicesResult= _elasticSearchApi.DeleteIndices(partnerCurrentEpgV3OrEpgV2Indices.Select(x => x.Name).ToList());
                if (!deleteIndicesResult)
                {
                    log.Error($"Can not remove EPGv3 indices ({currentEpgIndicesNames}) for partner - {_partnerId}!");
                    return;
                }

                #endregion
            }

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

        private static string[] ExtractLatestEpgV1Index(ESIndex[] partnerEpgV1Indices)
        {
            var epgV1Index = partnerEpgV1Indices.Length == 1
                ? partnerEpgV1Indices.First().Name
                : partnerEpgV1Indices.OrderByDescending(i =>
                    {
                        DateTime.TryParseExact(i.Name.Split('_').Last(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                        return date;
                    })
                    .First()
                    .Name;
            return string.IsNullOrEmpty(epgV1Index) ? new string[] { } : new[] {epgV1Index};
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
