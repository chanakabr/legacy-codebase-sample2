using ApiObjects;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupsCacheManager;
using System.Threading.Tasks;
using EpgBL;
using KLogMonitor;
using System.Reflection;
using ElasticSearch.Searcher;
using ApiObjects.SearchObjects;
using ApiObjects.Response;
using System.Data;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly string EPG = "epg";
        protected const string VERSION = "2";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        protected int sizeOfBulk;
        protected bool shouldAddRouting = true;

        #endregion

        #region Ctor

        public EpgIndexBuilderV2(int groupID)
            : base(groupID)
        {
            serializer = new ESSerializerV2();
        }

        #endregion

        #region Override Methods

        public override bool BuildIndex()
        {
            bool success = false;

            ContextData cd = new ContextData();
            CatalogGroupCache catalogGroupCache = null;
            Group group = null;
            List<ApiObjects.LanguageObj> languages = null;
            GroupManager groupManager = new GroupManager();
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            ApiObjects.LanguageObj defaultLanguage = null;
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                    return false;
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
                defaultLanguage = catalogGroupCache.DefaultLanguage;
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    log.ErrorFormat("Couldn't load group in cache when building index for group {0}", groupId);
                    return success;
                }

                languages = group.GetLangauges();
                defaultLanguage = group.GetGroupDefaultLanguage();
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

            string groupAlias = GetAlias();
            string newIndexName = GetNewIndexName();

            List<string> analyzers;
            List<string> filters;
            List<string> tokenizers;

            GetAnalyzers(languages, out analyzers, out filters, out tokenizers);

            sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            int maxResults = TVinciShared.WS_Utils.GetTcmIntValue("MAX_RESULTS");

            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            success = api.BuildIndex(newIndexName, 0, 0, analyzers, filters, tokenizers, maxResults);

            if (!success)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return success;
            }

            MappingAnalyzers defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage, VERSION);

            #region create mapping
            foreach (ApiObjects.LanguageObj language in languages)
            {
                MappingAnalyzers specificMappingAnalyzers = GetMappingAnalyzers(language, VERSION);
                string specificType = GetIndexType(language);

                #region Join tags and metas of EPG and media to same mapping

                Dictionary<string, KeyValuePair<eESFieldType, string>> metas = null;
                List<string> tags = null;
                if (!ElasticSearchTaskUtils.GetMetasAndTagsForMapping(groupId, doesGroupUsesTemplates, ref metas, ref tags, serializer, group, catalogGroupCache))
                {
                    log.Error("Failed GetMetasAndTagsForMapping as part of BuildIndex");
                    return false;
                }

                #endregion

                string mappingString = serializer.CreateEpgMapping(metas, tags, specificMappingAnalyzers, defaultMappingAnalyzers, specificType, shouldAddRouting);
                bool mappingResult = api.InsertMapping(newIndexName, specificType, mappingString.ToString());
                
                if (language.IsDefault && !mappingResult)
                    success = false;

                if (!mappingResult)
                {
                    log.Error(string.Concat("Could not create mapping of type epg for language ", language.Name));
                }

            }
            #endregion

            if (!success)
            {
                log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                return success;
            }

            log.DebugFormat("Start populating epg index = {0}", newIndexName);

            PopulateIndex(newIndexName, group);

            #region insert channel queries

            InsertChannelsQueries(groupManager, group, newIndexName, doesGroupUsesTemplates);

            #endregion

            #region Switch Index

            log.DebugFormat("Finished populating epg index = {0}", newIndexName);

            string originalIndex = GetAlias();
            bool indexExists = api.IndexExists(originalIndex);

            if (this.SwitchIndexAlias || !indexExists)
            {
                List<string> oldIndices = api.GetAliases(groupAlias);

                success = api.SwitchIndex(newIndexName, groupAlias, oldIndices, null);

                if (!success)
                {
                    log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, groupAlias);
                }

                if (this.DeleteOldIndices && success && oldIndices.Count > 0)
                {
                    api.DeleteIndices(oldIndices);
                }
            }

            #endregion

            return success;
        }

        #endregion

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
                        if (currentChannel == null || currentChannel.m_nIsActive != 1)
                            continue;

                        string channelQuery = string.Empty;

                        if (currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL)
                        {
                            // Only if it this channel is relevant to EPG, build its query
                            if (currentChannel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                            {
                                UnifiedSearchDefinitions definitions = ElasticsearchTasksCommon.Utils.BuildSearchDefinitions(currentChannel, false);

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
            return EPG;
        }

        protected virtual string GetIndexType(ApiObjects.LanguageObj language)
        {
            return (language.IsDefault) ? EPG : string.Concat(EPG, "_", language.Code);
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
                PopulateEpgIndex(newIndexName, EPG, tempDate, group);
                tempDate = tempDate.AddDays(1);
            }
        }

        protected virtual string SerializeEPGObject(EpgCB epg, string suffix = null)
        {
            return serializer.SerializeEpgObject(epg, suffix);
        }

        protected virtual string GetNewIndexName()
        {
            return ElasticSearchTaskUtils.GetNewEpgIndexStr(groupId);
        }

        private void GetAnalyzers(List<ApiObjects.LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (ApiObjects.LanguageObj language in languages)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, VERSION));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, VERSION));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        log.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);

                // we always want "autocomplete" ability
                filters.Add(PHRASE_STARTS_WITH_FILTER);
                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);
                
            }
        }

        protected void PopulateEpgIndex(string index, string type, DateTime date, Group group)
        {
            try
            {
                bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;
                Dictionary<ulong, Dictionary<string, EpgCB>> programs = new Dictionary<ulong, Dictionary<string, EpgCB>>();
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling PopulateEpgIndex", groupId);
                        return;
                    }

                    // TODO - Lior get all epgs differently when and if we decide to use different object then today                    
                }

                // Get EPG objects from CB
                programs = GetEpgPrograms(groupId, date);

                AddEPGsToIndex(index, type, programs, group, doesGroupUsesTemplates, catalogGroupCache);
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

            List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();

            // GetLinear Channel Values 
            var programsList = new List<EpgCB>();
            
            foreach (Dictionary<string, EpgCB> programsValues in programs.Values)
            {
                programsList.AddRange(programsValues.Values);
            }

            ElasticSearchTaskUtils.GetLinearChannelValues(programsList, groupId);

            // Run on all programs
            foreach (ulong epgID in programs.Keys)
            {
                foreach (string languageCode in programs[epgID].Keys)
                {
                    string suffix = null;

                    LanguageObj language = null;

                    if (!string.IsNullOrEmpty(languageCode))
                    {
                        language = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapByCode[languageCode] : group.GetLanguage(languageCode);

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
                        language = doesGroupUsesTemplates ? catalogGroupCache.DefaultLanguage : group.GetGroupDefaultLanguage();
                    }

                    EpgCB epg = programs[epgID][languageCode];

                    if (epg != null)
                    {
                        // Serialize EPG object to string
                        string serializedEpg = SerializeEPGObject(epg, suffix);
                        string epgType = ElasticSearchTaskUtils.GetTanslationType(type, language);

                        bulkRequests.Add(new ESBulkRequestObj<ulong>()
                        {
                            docID = GetDocumentId(epg),
                            document = serializedEpg,
                            index = index,
                            Operation = eOperation.index,
                            routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                            type = epgType
                        });
                    }

                    // If we exceeded maximum size of bulk 
                    if (bulkRequests.Count >= sizeOfBulk)
                    {
                        // create bulk request now and clear list
                        var invalidResults = api.CreateBulkRequest(bulkRequests);

                        if (invalidResults != null && invalidResults.Count > 0)
                        {
                            foreach (var item in invalidResults)
                            {
                                log.ErrorFormat("Error - Could not add EPG to ES index. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                    groupId, EPG, item.Key, item.Value);
                            }
                        }

                        bulkRequests.Clear();
                    }
                }
            }

            // If we have anything left that is less than the size of the bulk
            if (bulkRequests.Count > 0)
            {
                var invalidResults = api.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var item in invalidResults)
                    {
                        log.ErrorFormat("Error - Could not add EPG to ES index. GroupID={0};Type={1};EPG_ID={2};error={3};",
                            groupId, EPG, item.Key, item.Value);
                    }
                }
            }
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
