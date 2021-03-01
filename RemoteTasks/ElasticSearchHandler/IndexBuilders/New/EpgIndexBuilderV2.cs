using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ElasticSearch.Utilities;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilderV2 : AbstractIndexBuilder
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ITtlService _ttlService = new TtlService();

        #region Data Members

        private int epgCbBulkSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.EpgPageSize.Value;
        protected int sizeOfBulk = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;
        protected int sizeOfBulkDefaultValue = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.GetDefaultValue();
        protected bool shouldAddRouting = true;
        protected Dictionary<long, List<int>> linearChannelsRegionsMapping;

        #endregion

        #region Ctor

        public EpgIndexBuilderV2(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV2();
            linearChannelsRegionsMapping = new Dictionary<long, List<int>>();
        }

        #endregion

        public override bool BuildIndex()
        {

            ContextData cd = new ContextData();
            CatalogGroupCache catalogGroupCache;
            Group group;
            List<LanguageObj> languages;
            GroupManager groupManager;
            bool doesGroupUsesTemplates;
            LanguageObj defaultLanguage;
            try
            {
                GetGroupAndLanguages(out catalogGroupCache, out group, out languages, out groupManager, out doesGroupUsesTemplates, out defaultLanguage);
            }
            catch (Exception e)
            {
                log.Error("Erorr while getting groups and languages", e);
                return false;
            }

            // If request doesn't have start date, use [NOW - 7 days] as default
            if (!this.StartDate.HasValue)
            {
                this.StartDate = DateTime.UtcNow.Date.AddDays(-7);
            }

            // If request doesn't have end date, use [NOW + 7 days] as default
            if (!this.EndDate.HasValue)
            {
                this.EndDate = DateTime.UtcNow.Date.AddDays(7);
            }

            // prevent from size of bulk to be more than the default value of 500 (currently as of 23.06.20)
            sizeOfBulk = sizeOfBulk == 0 ? sizeOfBulkDefaultValue : sizeOfBulk > sizeOfBulkDefaultValue ? sizeOfBulkDefaultValue : sizeOfBulk;
            // Default size of epg cb bulk size
            epgCbBulkSize = epgCbBulkSize == 0 ? 1000 : epgCbBulkSize;

            var newIndexName = GetNewIndexName();

            try
            {
                MetasToPad = CreateNewIndex(groupId, catalogGroupCache, group, languages, defaultLanguage, newIndexName);
            }
            catch (Exception e)
            {
                log.Error("Error while building new index", e);
                return false;
            }

            log.DebugFormat("Start populating epg index = {0}", newIndexName);

            #region Get Linear Channels Regions

            if (doesGroupUsesTemplates ? catalogGroupCache.IsRegionalizationEnabled : group.isRegionalizationEnabled)
            {
                linearChannelsRegionsMapping = RegionManager.GetLinearMediaRegions(groupId);
            }

            #endregion

            PopulateIndex(newIndexName, group);

            #region Switch Index

            log.DebugFormat("Finished populating epg index = {0}", newIndexName);

            string originalIndex = GetAlias();
            bool indexExists = api.IndexExists(originalIndex);

            if (this.SwitchIndexAlias || !indexExists)
            {
                string groupAlias = GetAlias();
                List<string> oldIndices = api.GetAliases(groupAlias);

                bool switchSuccess = api.SwitchIndex(newIndexName, groupAlias, oldIndices, null);

                if (!switchSuccess)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, groupAlias);
                    return false;
                }

                if (this.DeleteOldIndices && oldIndices.Count > 0)
                {
                    api.DeleteIndices(oldIndices);
                }
            }

            #endregion

            return true;
        }



        private void GetGroupAndLanguages(out CatalogGroupCache catalogGroupCache, out Group group, out List<LanguageObj> languages, out GroupManager groupManager, out bool doesGroupUsesTemplates, out LanguageObj defaultLanguage)
        {
            catalogGroupCache = null;
            group = null;
            languages = null;
            groupManager = new GroupManager();
            doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            defaultLanguage = null;

            if (doesGroupUsesTemplates)
            {
                // TODO: verify that we need or not to invalidate the group cache before we get the group to get the latest
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    throw new Exception($"failed to get catalogGroupCache for groupId: {groupId} when calling BuildIndex");
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
                defaultLanguage = catalogGroupCache.GetDefaultLanguage();
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    throw new Exception($"failed to get group for groupId: {groupId} when calling BuildIndex");
                }

                languages = group.GetLangauges();
                defaultLanguage = group.GetGroupDefaultLanguage();
            }
        }


        #region Private and protected Methods

        protected virtual void InsertChannelsQueries(GroupManager groupManager, Group group, string newIndexName, bool doesGroupUsesTemplates)
        {
            if (doesGroupUsesTemplates || group.channelIDs != null)
            {
                List<KeyValuePair<int, string>> channelRequests = new List<KeyValuePair<int, string>>();
                try
                {
                    List<Channel> allChannels = new List<Channel>();
                    if (doesGroupUsesTemplates)
                    {
                        allChannels = ChannelManager.GetGroupChannels(groupId);
                    }
                    else
                    {
                        allChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);
                    }

                    if (allChannels == null || allChannels.Count == 0)
                    {
                        log.ErrorFormat(string.Format("Didn't find any channels to index. total channels={0}", allChannels.Count));
                        return;
                    }

                    log.Info(string.Format("Start indexing channels. total channels={0}", allChannels.Count));

                    var unifiedQueryBuilder = new ESUnifiedQueryBuilder(null, groupId);

                    foreach (Channel currentChannel in allChannels)
                    {
                        if (currentChannel == null)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        // if group uses templates - index inactive channel as well
                        if (!doesGroupUsesTemplates && currentChannel.m_nIsActive != 1)
                        {
                            log.ErrorFormat("BuildChannelQueries - All channels list has null or in-active channel, continuing");
                            continue;
                        }

                        string channelQuery = string.Empty;

                        if ((currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                           (currentChannel.m_nChannelTypeID == (int)ChannelType.Manual && doesGroupUsesTemplates && currentChannel.AssetUserRuleId > 0))
                        {
                            // Only if it this channel is relevant to EPG, build its query
                            if (currentChannel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                            {
                                UnifiedSearchDefinitions definitions = IndexManager.BuildSearchDefinitions(currentChannel, false);

                                definitions.shouldSearchMedia = false;

                                unifiedQueryBuilder.SearchDefinitions = definitions;
                                channelQuery = unifiedQueryBuilder.BuildSearchQueryString();
                            }
                        }

                        if (!string.IsNullOrEmpty(channelQuery))
                        {
                            channelRequests.Add(new KeyValuePair<int, string>(currentChannel.m_nChannelID, channelQuery));

                            if (channelRequests.Count > 50)
                            {
                                api.CreateBulkIndexRequest(newIndexName, ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE, channelRequests);
                                channelRequests.Clear();
                            }
                        }
                    }

                    if (channelRequests.Count > 0)
                    {
                        api.CreateBulkIndexRequest(newIndexName, ElasticSearch.Common.Utils.ES_PERCOLATOR_TYPE, channelRequests);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Caught exception while indexing channels. Ex={0};Stack={1}", ex.Message, ex.StackTrace));
                }
            }
        }

        protected virtual string GetIndexType()
        {
            return IndexManager.EPG_INDEX_TYPE;
        }

        protected virtual string GetIndexType(ApiObjects.LanguageObj language)
        {
            return (language.IsDefault) ? IndexManager.EPG_INDEX_TYPE : string.Concat(IndexManager.EPG_INDEX_TYPE, "_", language.Code);
        }

        protected virtual string GetAlias()
        {
            return ElasticSearchTaskUtils.GetEpgGroupAliasStr(groupId);
        }

        protected virtual void PopulateIndex(string newIndexName, Group group)
        {
            DateTime tempDate = StartDate.Value;

            while (tempDate <= this.EndDate.Value)
            {
                PopulateEpgIndex(newIndexName, IndexManager.EPG_INDEX_TYPE, tempDate, group);
                tempDate = tempDate.AddDays(1);
            }
        }

        protected virtual string SerializeEPGObject(EpgCB epg, string suffix = null, bool doesGroupUsesTemplates = false)
        {
            return serializer.SerializeEpgObject(epg, suffix, doesGroupUsesTemplates);
        }

        protected virtual string GetNewIndexName()
        {
            return ElasticSearchTaskUtils.GetNewEpgIndexStr(groupId);
        }

        protected virtual HashSet<string> CreateNewIndex(int groupId, CatalogGroupCache catalogGroupCache, Group group, IEnumerable<LanguageObj> languages, LanguageObj defaultLanguage, string newIndexName)
        {
            return IndexManager.CreateNewEpgIndex(groupId, catalogGroupCache, group, languages, defaultLanguage, newIndexName);
        }

        protected void PopulateEpgIndex(string index, string type, DateTime date, Group group)
        {
            try
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;
                Dictionary<ulong, Dictionary<string, EpgCB>> programs = new Dictionary<ulong, Dictionary<string, EpgCB>>();
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling PopulateEpgIndex", groupId);
                        return;
                    }          
                }

                // Get EPG objects from CB
                programs = GetEpgPrograms(groupId, date, epgCbBulkSize);

                if (programs != null && programs.Count > 0)
                {
                    log.DebugFormat($"found {programs.Count} epgs for day {date}");
                    AddEPGsToIndex(index, type, programs, group, doesGroupUsesTemplates, catalogGroupCache);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed when populating epg index. index = {0}, type = {1}, date = {2}, message = {3}, st = {4}",
                    index, type, date, ex.Message, ex.StackTrace);

                throw ex;
            }
        }

        protected virtual void AddEPGsToIndex(string index, string type, Dictionary<ulong, Dictionary<string, EpgCB>> programs, Group group, bool doesGroupUsesTemplates, CatalogGroupCache catalogGroupCache)
        {
            // Basic validation
            if (programs == null || programs.Count == 0)
            {
                log.ErrorFormat("AddEPGsToIndex {0}/{1} for group {2}: programs is null or empty!", index, type, this.groupId);
                return;
            }

            // save current value to restore at the end
            int currentDefaultConnectionLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
            try
            {
                int numOfBulkRequests = 0;
                Dictionary<int, List<ESBulkRequestObj<ulong>>> bulkRequests = 
                    new Dictionary<int, List<ESBulkRequestObj<ulong>>>() { { numOfBulkRequests, new List<ESBulkRequestObj<ulong>>() } };

                // GetLinear Channel Values 
                var programsList = new List<EpgCB>();

                foreach (Dictionary<string, EpgCB> programsValues in programs.Values)
                {
                    programsList.AddRange(programsValues.Values);
                }

                ElasticSearchTaskUtils.GetLinearChannelValues(programsList, groupId);

                // used only to support linear media id search on elastic search
                List<string> epgChannelIds = programsList.Select(item => item.ChannelID.ToString()).ToList<string>();
                Dictionary<string, LinearChannelSettings> linearChannelSettings = Core.Catalog.Cache.CatalogCache.Instance().GetLinearChannelSettings(groupId, epgChannelIds);

                // Run on all programs
                foreach (ulong epgID in programs.Keys)
                {
                    foreach (string languageCode in programs[epgID].Keys)
                    {
                        string suffix = null;

                        LanguageObj language = null;

                        if (!string.IsNullOrEmpty(languageCode))
                        {
                            if (doesGroupUsesTemplates)
                            {
                                language = catalogGroupCache.LanguageMapByCode.ContainsKey(languageCode) ? catalogGroupCache.LanguageMapByCode[languageCode] : null;
                            }
                            else
                            {
                                language = group.GetLanguage(languageCode);
                            }

                            // Validate language
                            if (language == null)
                            {
                                log.ErrorFormat("AddEPGsToIndex: Epg {0} has invalid language code {1}", epgID, languageCode);
                                continue;
                            }

                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }
                        }
                        else
                        {
                            language = doesGroupUsesTemplates ? catalogGroupCache.GetDefaultLanguage() : group.GetGroupDefaultLanguage();
                        }

                        EpgCB epg = programs[epgID][languageCode];

                        if (epg != null)
                        {
                            epg.PadMetas(MetasToPad);

                            // used only to currently support linear media id search on elastic search
                            if (linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                            {
                                epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].LinearMediaId;
                            }

                            if (epg.LinearMediaId > 0 && linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(epg.LinearMediaId))
                            {
                                epg.regions = linearChannelsRegionsMapping[epg.LinearMediaId];
                            }

                            // Serialize EPG object to string
                            string serializedEpg = SerializeEPGObject(epg, suffix, doesGroupUsesTemplates);
                            string epgType = ElasticSearchTaskUtils.GetTanslationType(type, language);
                            ulong documentId = GetDocumentId(epg);


                            var ttl = string.Empty;
                            var shouldSetTTL = ShouldSetTTL();
                            if (shouldSetTTL)
                            {
                                var totalMinutes = _ttlService.GetEpgTtlMinutes(epg);
                                ttl = $"{totalMinutes}m";
                            }

                            // If we exceeded the size of a single bulk reuquest then create another list
                            if (bulkRequests[numOfBulkRequests].Count >= sizeOfBulk)
                            {
                                numOfBulkRequests++;
                                bulkRequests.Add(numOfBulkRequests, new List<ESBulkRequestObj<ulong>>());
                            }

                            ESBulkRequestObj<ulong> bulkRequest = 
                                new ESBulkRequestObj<ulong>(documentId, index, epgType, serializedEpg, eOperation.index, epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"), ttl);
                            bulkRequests[numOfBulkRequests].Add(bulkRequest);
                        }
                    }
                }
                
                int maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                if (maxDegreeOfParallelism == 0)
                {
                    maxDegreeOfParallelism = 5;
                }

                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                ContextData contextData = new ContextData();
                System.Net.ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount;
                System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<ulong>>> failedBulkRequests = new System.Collections.Concurrent.ConcurrentBag<List<ESBulkRequestObj<ulong>>>();
                // Send request to elastic search in a different thread
                Parallel.ForEach(bulkRequests, options,(bulkRequest, state) =>
                {
                    contextData.Load();
                    List<ESBulkRequestObj<ulong>> invalidResults;
                    bool bulkResult = api.CreateBulkRequests(bulkRequest.Value, out invalidResults);

                    // Log invalid results
                    if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                    {
                        log.Warn($"Bulk request when indexing epg for partner {groupId} has invalid results. Will retry soon.");

                        // add entire failed retry requests to failedBulkRequests, will try again not in parallel (maybe ES is loaded)
                        failedBulkRequests.Add(invalidResults);
                    }
                });

                // retry on all failed bulk requests (this time not in parallel)
                if (failedBulkRequests.Count > 0)
                {
                    foreach (List<ESBulkRequestObj<ulong>> bulkRequest in failedBulkRequests)
                    {
                        List<ESBulkRequestObj<ulong>> invalidResults;
                        bool bulkResult = api.CreateBulkRequests(bulkRequest, out invalidResults);

                        // Log invalid results
                        if (!bulkResult && invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add EPG to ES index, additional retry will not be attempted. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                    groupId, IndexManager.EPG_INDEX_TYPE, item.docID, item.error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed during AddEPGsToIndex", ex);
            }
            finally
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = currentDefaultConnectionLimit;
            }
        }

        protected virtual bool ShouldSetTTL()
        {
            return true;
        }

        protected virtual ulong GetDocumentId(ulong epgId)
        {
            return epgId;
        }

        protected virtual ulong GetDocumentId(EpgCB epg)
        {
            return epg.EpgID;
        }

        #endregion
    }
}
