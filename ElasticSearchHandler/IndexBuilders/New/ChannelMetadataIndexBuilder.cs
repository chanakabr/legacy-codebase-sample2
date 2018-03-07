using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticSearch.Common;
using ElasticsearchTasksCommon;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json.Linq;

namespace ElasticSearchHandler.IndexBuilders
{
    public class ChannelMetadataIndexBuilder : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string CHANNEL = "channel";
        protected const string VERSION = "2";

        public ChannelMetadataIndexBuilder(int groupId) : base(groupId)
        {
            serializer = new ESSerializerV2();
        }

        public override bool BuildIndex()
        {
            bool result = false;
            ContextData cd = new ContextData();
            string newIndexName = ElasticSearchTaskUtils.GetNewChannelMetadataIndexName(groupId);

            // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
            int numOfShards = TVinciShared.WS_Utils.GetTcmIntValue("ES_NUM_OF_SHARDS");
            int numOfReplicas = TVinciShared.WS_Utils.GetTcmIntValue("ES_NUM_OF_REPLICAS");
            int sizeOfBulk = TVinciShared.WS_Utils.GetTcmIntValue("ES_BULK_SIZE");
            int maxResults = TVinciShared.WS_Utils.GetTcmIntValue("MAX_RESULTS");

            // Default for size of bulk should be 50, if not stated otherwise in TCM
            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 50;
            }

            // Default size of max results should be 100,000
            if (maxResults == 0)
            {
                maxResults = 100000;
            }

            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            // Check if group supports Templates
            if (doesGroupUsesTemplates)
            {
                CatalogGroupCache catalogGroupCache;

                try
                {
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed BuildIndex for channels metadatae of groupId: {0} because of CatalogGroupCache error", groupId), ex);
                    return false;
                }

                #region Build Index

                List<string> analyzers;
                List<string> filters;
                List<string> tokenizers;

                List<ApiObjects.LanguageObj> languages = new List<ApiObjects.LanguageObj>()
                {
                    catalogGroupCache.DefaultLanguage
                };

                GetAnalyzers(languages, out analyzers, out filters, out tokenizers);

                bool actionResult = api.BuildIndex(newIndexName, numOfShards, numOfReplicas,
                    analyzers, filters, tokenizers, maxResults);

                if (!actionResult)
                {
                    log.Error(string.Format("Failed creating index for index:{0}", newIndexName));
                    return actionResult;
                }

                #endregion
                
                #region Mapping
                // Mapping for each language
                foreach (ApiObjects.LanguageObj language in languages)
                {
                    string indexAnalyzer, searchAnalyzer;
                    string autocompleteIndexAnalyzer = null;
                    string autocompleteSearchAnalyzer = null;

                    // create names for analyzers to be used in the mapping later on
                    string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, VERSION);

                    if (ElasticSearchApi.AnalyzerExists(analyzerDefinitionName))
                    {
                        indexAnalyzer = string.Concat(language.Code, "_index_", "analyzer");
                        searchAnalyzer = string.Concat(language.Code, "_search_", "analyzer");

                        string analyzerDefinition = ElasticSearchApi.GetAnalyzerDefinition(analyzerDefinitionName);

                        if (analyzerDefinition.Contains("autocomplete"))
                        {
                            autocompleteIndexAnalyzer = string.Concat(language.Code, "_autocomplete_analyzer");
                            autocompleteSearchAnalyzer = string.Concat(language.Code, "_autocomplete_search_analyzer");
                        }
                    }
                    else
                    {
                        indexAnalyzer = "whitespace";
                        searchAnalyzer = "whitespace";
                        log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
                    }

                    string type = CHANNEL;
                    string suffix = null;
                    
                    // Ask serializer to create the mapping definitions string
                    string mapping = serializer.CreateChannelMapping(indexAnalyzer, searchAnalyzer, autocompleteIndexAnalyzer, autocompleteSearchAnalyzer, suffix);

                    bool mappingResult = api.InsertMapping(newIndexName, type, mapping.ToString());

                    // Most important is the mapping for the default language, we can live without the others...
                    if (language.IsDefault && !mappingResult)
                    {
                        actionResult = false;
                    }

                    if (!mappingResult)
                    {
                        log.Error(string.Concat("Could not create mapping of type channel for language ", language.Name));
                    }
                }

                #endregion

                #region Populate Index
                List<Channel> allChannels = null;
                if (doesGroupUsesTemplates)
                {
                    allChannels = ChannelManager.GetGroupChannels(groupId);
                }
                else
                {
                    GroupManager groupManager = new GroupManager();
                    groupManager.RemoveGroup(groupId);
                    Group group = groupManager.GetGroup(groupId);

                    allChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);
                }

                if (allChannels != null)
                {
                    List<ESBulkRequestObj<string>> bulkList = new List<ESBulkRequestObj<string>>();

                    // For each channel value
                    foreach (var channel in allChannels)
                    {   
                        string documentType = CHANNEL;

                        // Serialize channel and create a bulk request for it
                        string serializedChannel = serializer.SerializeChannelObject(channel);

                        bulkList.Add(new ESBulkRequestObj<string>()
                        {
                            docID = channel.m_nChannelID.ToString(),
                            index = newIndexName,
                            type = documentType,
                            document = serializedChannel
                        });

                        // If we exceeded the size of a single bulk reuquest
                        if (bulkList.Count >= sizeOfBulk)
                        {
                            // Send request to elastic search in a different thread
                            Task t = Task.Run(() =>
                            {
                                cd.Load();

                                var invalidResults = api.CreateBulkRequest(bulkList);

                                // Log invalid results
                                if (invalidResults != null && invalidResults.Count > 0)
                                {
                                    foreach (var item in invalidResults)
                                    {
                                        log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                            groupId, CHANNEL, item.Key, item.Value);
                                    }
                                }
                            });

                            t.Wait();
                            bulkList.Clear();
                        }
                    }

                    // If we have a final bulk pending
                    if (bulkList.Count > 0)
                    {
                        // Send request to elastic search in a different thread
                        Task t = Task.Run(() =>
                        {
                            cd.Load();
                            var invalidResults = api.CreateBulkRequest(bulkList);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var item in invalidResults)
                                {
                                    log.ErrorFormat("Error - Could not add channel to ES index. GroupID={0};Type={1};ID={2};error={3};",
                                        groupId, CHANNEL, item.Key, item.Value);
                                }
                            }
                        });
                        t.Wait();
                    }
                }
                #endregion

                #region Switch index alias + Delete old indices handling

                string alias = ElasticSearchTaskUtils.GetChannelMetadataIndexName(groupId);
                bool indexExists = api.IndexExists(alias);

                if (this.SwitchIndexAlias || !indexExists)
                {
                    List<string> oldIndices = api.GetAliases(alias);

                    Task<bool> taskSwitchIndex = Task<bool>.Factory.StartNew(() =>
                    {
                        cd.Load();
                        return api.SwitchIndex(newIndexName, alias, oldIndices);
                    });

                    taskSwitchIndex.Wait();

                    if (!taskSwitchIndex.Result)
                    {
                        log.ErrorFormat("Failed switching index for new index name = {0}, group alias = {1}", newIndexName, alias);
                        actionResult = false;
                    }

                    if (this.DeleteOldIndices && taskSwitchIndex.Result && oldIndices.Count > 0)
                    {
                        Task t = Task.Run(() =>
                        {
                            cd.Load();
                            api.DeleteIndices(oldIndices);
                        });

                        t.Wait();
                    }
                }

                #endregion

                result = true;
            }

            return result;
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
            }
        }
    }
}
