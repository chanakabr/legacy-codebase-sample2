using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using ElasticSearch.Common;
using KLogMonitor;
using QueueWrapper;
using QueueWrapper.Enums;
using QueueWrapper.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Core.GroupManagers
{
    public class PartnerManager
    {
        private const string ROUTING_PARAMETER_KEY = "partner_id";
        private const string ES_VERSION = "2";
        private const string MEDIA_INDEX_MAP_TYPE = "media";
        private const string TAG_INDEX_MAP_TYPE = "tag";
        private const string EPG_INDEX_MAP_TYPE = "epg";
        private const string RECORDING_INDEX_MAP_TYPE = "recording";
        private const string CHANNEL_INDEX_MAP_TYPE = "channel";

        public const string LOWERCASE_ANALYZER =
            "\"lowercase_analyzer\": {\"type\": \"custom\",\"tokenizer\": \"keyword\",\"filter\": [\"lowercase\",\"asciifolding\"],\"char_filter\": [\"html_strip\"]}";

        public const string PHRASE_STARTS_WITH_FILTER =
            "\"edgengram_filter\": {\"type\":\"edgeNGram\",\"min_gram\":1,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}";

        public const string PHRASE_STARTS_WITH_ANALYZER =
            "\"phrase_starts_with_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\",\"edgengram_filter\", \"icu_folding\",\"icu_normalizer\",\"asciifolding\"]," +
            "\"char_filter\":[\"html_strip\"]}";

        public const string PHRASE_STARTS_WITH_SEARCH_ANALYZER =
            "\"phrase_starts_with_search_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\", \"icu_folding\",\"icu_normalizer\",\"asciifolding\"]," +
            "\"char_filter\":[\"html_strip\"]}";

        private static readonly Lazy<PartnerManager> LazyInstance = new Lazy<PartnerManager>(() => 
            new PartnerManager(PartnerDal.Instance,
                               RabbitConnection.Instance, 
                               ApplicationConfiguration.Current, 
                               UserManager.Instance, 
                               RabbitConfigDal.Instance, 
                               PricingDAL.Instance,
                               new ElasticSearchApi(),
                               ElasticSearchIndexDefinitions.Instance,
                               CatalogManager.Instance), 
            LazyThreadSafetyMode.PublicationOnly);
        public static PartnerManager Instance => LazyInstance.Value;

        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IPartnerDal _partnerDal;
        private readonly IPartnerRepository _pricingDal;
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IUserManager _userManager;
        private readonly IRabbitConfigDal _rabbitConfigDal;
        private readonly IElasticSearchApi _elasticSearchApi;
        private readonly ElasticSearchIndexDefinitions _indexDefinitions;
        private readonly ICatalogManager _catalogManager;

        private static readonly List<KeyValuePair<long, long>> usersModuleIdList = new List<KeyValuePair<long, long>>{
            new KeyValuePair<long, long>(1, 1), new KeyValuePair<long, long>(2, 1)};
        private static readonly List<KeyValuePair<long, long>> pricingModuleIdList = new List<KeyValuePair<long, long>>{
            new KeyValuePair<long, long>(1, 1), new KeyValuePair<long, long>(2, 1), new KeyValuePair<long, long>(3, 1),
        new KeyValuePair<long, long>(4, 1), new KeyValuePair<long, long>(5, 1), new KeyValuePair<long, long>(6, 1)};

        private static Dictionary<string, MappingAnalyzers> _mappingAnalyzers = new Dictionary<string, MappingAnalyzers>();

        public PartnerManager(IPartnerDal partnerDal, 
                              IRabbitConnection rabbitConnection, 
                              IApplicationConfiguration applicationConfiguration, 
                              IUserManager userManager, 
                              IRabbitConfigDal rabbitConfigDal, 
                              IPartnerRepository pricingDal,
                              IElasticSearchApi elasticSearchApi,
                              ElasticSearchIndexDefinitions indexDefinitions,
                              ICatalogManager catalogManager)
        {
            _partnerDal = partnerDal;
            _rabbitConnection = rabbitConnection;
            _applicationConfiguration = applicationConfiguration;
            _userManager = userManager;
            _rabbitConfigDal = rabbitConfigDal;
            _pricingDal = pricingDal;
            _elasticSearchApi = elasticSearchApi;
            _indexDefinitions = indexDefinitions;
            _catalogManager = catalogManager;
        }

        public GenericResponse<Partner> AddPartner(Partner partner, PartnerSetup partnerSetup, long updaterId)
        {
            var response = new GenericResponse<Partner>();
            // validate if partner already exist
            var existingPartners = GetPartners();
            if (existingPartners.HasObjects())
            {
                if (partner.Id.HasValue && existingPartners.Objects.Exists(_ => _.Id == partner.Id.Value))
                {
                    response.SetStatus(eResponseStatus.Error, $"Partner id:{partner.Id} already exist");
                    return response;
                }

                if (existingPartners.Objects.Exists(_ => _.Name == partner.Name))
                {
                    response.SetStatus(eResponseStatus.Error, $"Partner name:{partner.Name} already exist");
                    return response;
                }
            }

            var partnerId = _partnerDal.AddPartner(partner.Id, partner.Name, updaterId);
            if (partnerId <= 0) return response;
            partner.Id = partnerId;

            if (!(_partnerDal.SetupPartnerInUsersDb(partnerId, usersModuleIdList, updaterId) &&
                _pricingDal.SetupPartnerInPricingDb(partnerId, pricingModuleIdList, updaterId)))
            {
                response.SetStatus(eResponseStatus.Error, "Failed to create partner basic data");
                return response; // TODO rollback?
            }
             
            var userId = _userManager.AddAdminUser(partnerId, partnerSetup.AdminUsername, partnerSetup.AdminPassword);
            if (userId <= 0)
            {
                response.SetStatus(eResponseStatus.Error, $"Failed to add first admin user:{partnerSetup.AdminUsername} to partner");
                return response;
            }
             
            // TODO - WHEN ERRORS HANDLE SOMEHOW
            IterateRabbitQueues(partnerId, RoutingKeyQueueAction.Bind);

            response.SetStatus(eResponseStatus.OK);
            response.Object = partner;
            return response;
        }

        public GenericListResponse<Partner> GetPartners(List<long> partnerIds = null)
        {
            var partners = _partnerDal.GetPartners();

            if (partners.Count > 0)
            {
                if (partnerIds != null && partnerIds.Count > 0)
                {
                    partners = partners.FindAll(p => partnerIds.Contains(p.Id.Value));
                }
                
                return new GenericListResponse<Partner>(Status.Ok, partners) {TotalItems = partners.Count};
            }

            return new GenericListResponse<Partner>(Status.Ok, new List<Partner>(0));
        }

        public Status Delete(long updaterId, int id)
        {
            Status result = new Status();

            if (!_partnerDal.IsPartnerExists(id))
            {
                Log.Error($"Error while Delete Partner BasicData. updaterId: {updaterId}.");
                result.Set(eResponseStatus.PartnerDoesNotExist, $"Partner {id} does not exist");
                return result;
            }

            IterateRabbitQueues(id, RoutingKeyQueueAction.Unbind);

            if (!_pricingDal.DeletePartnerInPricingDb(id, updaterId))
            {
                Log.Error($"Error while delete partner pricing basicData. updaterId: {updaterId}.");
            }

            if (!_partnerDal.DeletePartnerInUsersDb(id, updaterId))
            {
                Log.Error($"Error while delete partner users basicData. updaterId: {updaterId}.");
            }

            if (!_partnerDal.DeletePartner(id, updaterId))
            {
                Log.Error($"Error while Delete Partner. updaterId: {updaterId}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            result.Set(eResponseStatus.OK);

            return result;
        }

        private void IterateRabbitQueues(long groupId, RoutingKeyQueueAction action)
        {
            // Get Queue list and routing key
            Dictionary<string, string> rabbitRoutingKetWithQueueNameDic = _rabbitConfigDal.GetRabbitRoutingBindings();
            if (rabbitRoutingKetWithQueueNameDic.Count < 1)
            {
                Log.Error("Failed to get Rabbit queue bindings from db");
                throw new Exception("CreateNewGroupRabbit error");
            }

            RabbitQueue rabbitQueue = new RabbitQueue(_applicationConfiguration);
            var configurationDataForInitialize = rabbitQueue.CreateRabbitConfigurationData();
            int retryCount = 0;

            if (configurationDataForInitialize == null)
            {
                Log.Error("Error while getting queue TCM configuration");
                throw new Exception("InitializeRabbitInstance error");
            }

            // Need to Initialize before so not all the parallel will try to Initialize at the same time 
            if (!_rabbitConnection.InitializeRabbitInstance(configurationDataForInitialize, QueueAction.Ack, ref retryCount, out var connection) && connection != null)
            {
                Log.Error("Error while initialize rabbit instance");
                throw new Exception("InitializeRabbitInstance error");
            }

            Parallel.ForEach(rabbitRoutingKetWithQueueNameDic.Keys, (queueName) =>
            {
                // in case value is split by ";" --> bind is needed for every routing key
                var routingKeys = rabbitRoutingKetWithQueueNameDic[queueName].Split(';');

                foreach (string routingKey in routingKeys)
                {
                    var configurationData = rabbitQueue.CreateRabbitConfigurationData();

                    if (configurationData == null)
                    {
                        Log.Error("Error while getting queue TCM configuration");
                        throw new Exception("GetRabbitConfigurationData error");
                    }

                    configurationData.QueueName = queueName;
                    configurationData.RoutingKey = routingKey.Replace(ROUTING_PARAMETER_KEY, groupId.ToString()).Trim();

                    if (_rabbitConnection.IterateRoutingKeyQueue(configurationData, action))
                    {
                        Log.Debug($"Succeeded to iterate Rabbit queue: {configurationData.RoutingKey}");
                    }
                    else
                    {
                        Log.Error($"Failed to iterate Rabbit queue: {configurationData.RoutingKey}");
                        throw new Exception("CreateNewGroupRabbit error");
                    }
                }
            });
        }

        public Status CreateIndexes(int groupId)
        {
            if (!_partnerDal.IsPartnerExists(groupId))
            {
                return new Status(eResponseStatus.PartnerDoesNotExist, $"Partner {groupId} does not exist");
            }

            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var  catalogGroupCache))
            {
                Log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling CreateIndexes", groupId);
                return Status.Error;
            }
            
            var serializer = new ESSerializerV2();
            if (!GetMetasAndTagsForMapping(serializer, catalogGroupCache, out var metas, out var tags, out var metasToPad))
            {
                Log.Error($"Failed GetMetasAndTagsForMapping while calling CreateIndexes");
                return Status.Error;
            }
            
            var defaultLanguage = catalogGroupCache.LanguageMapById.Values.FirstOrDefault(x => x.IsDefault);
            MappingAnalyzers defaultMappingAnalyzers = GetMappingAnalyzers(defaultLanguage);

            // get definitions of analyzers, filters and tokenizers
            GetAnalyzers(catalogGroupCache.LanguageMapById.Values, out var analyzers, out var filters, out var tokenizers);
            
            Task<Status>[] taskArray = { 
                Task<Status>.Factory.StartNew(() => CreateIndex(GetMediaIndexName(groupId), 
                                                                GetMediaIndexAlias(groupId), 
                                                                GetMediaIndexType,
                                                                (ma, type) => serializer.CreateMediaMapping(metas, tags, metasToPad, ma, defaultMappingAnalyzers))),
                
                Task<Status>.Factory.StartNew(() => CreateIndex(GetEpgIndexName(groupId), 
                                                                GetEpgIndexAlias(groupId), 
                                                                GetEpgIndexType,
                                                                (ma, type) => serializer.CreateEpgMapping(metas, tags, metasToPad, ma, defaultMappingAnalyzers, type, true))), // for now we support only in epg v2
                
                Task<Status>.Factory.StartNew(() => CreateIndex(GetRecordingIndexName(groupId), 
                                                                GetRecordingIndexAlias(groupId), 
                                                                GetRecordingIndexType,
                                                                (ma, type) => serializer.CreateEpgMapping(metas, tags, metasToPad, ma, defaultMappingAnalyzers, type, true))),
                
                Task<Status>.Factory.StartNew(() => CreateIndex(GetTagIndexName(groupId), 
                                                                GetTagIndexAlias(groupId), 
                                                                GetTagIndexType,
                                                                (ma, type) => serializer.CreateMetadataMapping(ma.normalIndexAnalyzer, ma.normalSearchAnalyzer, ma.autocompleteIndexAnalyzer, ma.autocompleteSearchAnalyzer, ma.suffix))),
                
                Task<Status>.Factory.StartNew(() => CreateIndex(GetChannelMetadataIndexName(groupId), 
                                                                GetChannelMetadataIndexAlias(groupId),
                                                                GetChannelMetadataIndexType,
                                                                (ma, type) => serializer.CreateChannelMapping(ma.normalIndexAnalyzer, ma.normalSearchAnalyzer, ma.autocompleteIndexAnalyzer, ma.autocompleteSearchAnalyzer, ma.suffix), 
                                                                GetChannelMetadataIndexSuffix)),
            };

            Status CreateIndex(string indexName, string alias, Func<LanguageObj, string> getIndexTypeFunc, MapAnalyzersFunc mapAnalyzersFunc, Func<LanguageObj, string> getIndexSuffixFunc = null)
            {
                var result = Status.Ok;

                if (_elasticSearchApi.IndexExists(indexName)) { return result; }
                bool actionResult = CreateEmptyIndex(indexName, analyzers, filters, tokenizers);
                if (!actionResult)
                {
                    result.Set(eResponseStatus.Error, $"Failed creating index [{indexName}]");
                    return result;
                }

                if (getIndexSuffixFunc == null)
                {
                    getIndexSuffixFunc = GetGeneralIndexSuffix;
                }

                if (!CreateIndexMappings(indexName, catalogGroupCache, getIndexTypeFunc, mapAnalyzersFunc, getIndexSuffixFunc))
                {
                    result.Set(eResponseStatus.Error, $"Failed creating index [{indexName}] mapping");
                    return result;
                }

                if (!CreateIndexAliasIfNotExist(indexName, alias))
                {
                    result.Set(eResponseStatus.Error, $"Failed creating index [{indexName}] alias [{alias}]");
                    return result;
                }
                return result;
            }

            Task.WaitAll(taskArray);
            var errorTasks = taskArray.Where(t => !t.Result.IsOkStatusCode()).ToList();
            return errorTasks.Count == 0 ? Status.Ok : Status.ErrorMessage(string.Join("; ", errorTasks.Select(_ => _.Result.Message)));
        }

        // TODO SUNNY/arthur  - ALL OF THIS region SHOULD BE PART OF new INDEX_MANAGER 
        #region Index creation

        private bool CreateEmptyIndex(string indexName, List<string> analyzers, List<string> filters, List<string> tokenizers)
        {
            // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
            int replicas = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfReplicas.Value;
            int shards = _applicationConfiguration.ElasticSearchHandlerConfiguration.NumberOfShards.Value;
            // Default size of max results should be 100,000
            var maxResults = _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value == 0 ? 100000 :
                _applicationConfiguration.ElasticSearchConfiguration.MaxResults.Value;

            var isIndexCreated = _elasticSearchApi.BuildIndex(indexName, shards, replicas, analyzers, filters, tokenizers, maxResults);

            return isIndexCreated;
        }

        private bool CreateIndexMappings(string indexName, CatalogGroupCache catalogGroupCache, Func<LanguageObj, string> getIndexTypeFunc, MapAnalyzersFunc mapAnalyzersFunc, Func<LanguageObj, string> getIndexSuffixFunc)
        {
            // Mapping for each language
            foreach (var language in catalogGroupCache.LanguageMapById.Values)
            {
                string type = getIndexTypeFunc(language);
                MappingAnalyzers specificMappingAnalyzers = GetMappingAnalyzers(language);
                specificMappingAnalyzers.suffix = getIndexSuffixFunc(language);

                // Ask serializer to create the mapping definitions string
                string mapping = mapAnalyzersFunc(specificMappingAnalyzers, type);
                //string mapping = createMappingFunc(serializer, mappingAnlyzers);
                bool mappingResult = _elasticSearchApi.InsertMapping(indexName, type, mapping.ToString());

                // Most important is the mapping for the default language, we can live without the others...
                if (language.IsDefault && !mappingResult)
                {
                    return false;
                }
                if (!mappingResult)
                {
                    Log.Error($"Could not create mapping of type {type} for language {language.Name}");
                }
            }

            return true;
        }

        private bool CreateIndexAliasIfNotExist(string index, string alias)
        {
            if (_elasticSearchApi.IndexExists(alias)) { return true; }
            return _elasticSearchApi.AddAlias(index, alias);
        }

        private bool GetMetasAndTagsForMapping(BaseESSeralizer serializer, CatalogGroupCache catalogGroupCache,
            out Dictionary<string, KeyValuePair<eESFieldType, string>> metas, out List<string> tags, out HashSet<string> metasToPad, bool isEpg = false)
        {
            bool result = true;
            tags = new List<string>();
            metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();

            // Padded with zero prefix metas to sort numbers by text without issues in elastic (Brilliant!)
            metasToPad = new HashSet<string>();
            try
            {
                HashSet<string> topicsToIgnore = CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                tags = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => x.Value.ContainsKey(MetaType.Tag.ToString()) && !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();

                foreach (KeyValuePair<string, Dictionary<string, Topic>> topics in catalogGroupCache.TopicsMapBySystemNameAndByType)
                {
                    //TODO anat ask Ira
                    if (topics.Value.Keys.Any(x => x != MetaType.Tag.ToString() && x != MetaType.ReleatedEntity.ToString()))
                    {
                        string nullValue = string.Empty;
                        eESFieldType metaType;
                        MetaType topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                        serializer.GetMetaType(topicMetaType, out metaType, out nullValue);

                        if (topicMetaType == MetaType.Number && !metasToPad.Contains(topics.Key.ToLower()))
                        {
                            metasToPad.Add(topics.Key.ToLower());
                        }

                        if (!metas.ContainsKey(topics.Key.ToLower()))
                        {
                            metas.Add(topics.Key.ToLower(), new KeyValuePair<eESFieldType, string>(isEpg ? eESFieldType.STRING : metaType, nullValue));
                        }
                        else
                        {
                            Log.Error($"Duplicate topic found for name {topics.Key.ToLower()}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An Exception was occurred in GetMetasAndTagsForMapping. details:{ex}.");
                return false;
            }

            return result;
        }

        private MappingAnalyzers GetMappingAnalyzers(LanguageObj language)
        {
            if (_mappingAnalyzers.TryGetValue(language.Code, out MappingAnalyzers specificMappingAnlyzers))
            {
                return specificMappingAnlyzers;
            }

            specificMappingAnlyzers = new MappingAnalyzers();
            // create names for analyzers to be used in the mapping later on
            string analyzerDefinitionName = ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, ES_VERSION);

            if (_indexDefinitions.AnalyzerExists(analyzerDefinitionName))
            {
                specificMappingAnlyzers.normalIndexAnalyzer = string.Concat(language.Code, "_index_", "analyzer");
                specificMappingAnlyzers.normalSearchAnalyzer = string.Concat(language.Code, "_search_", "analyzer");

                string analyzerDefinition = _indexDefinitions.GetAnalyzerDefinition(analyzerDefinitionName);

                if (analyzerDefinition.Contains("autocomplete"))
                {
                    specificMappingAnlyzers.autocompleteIndexAnalyzer = string.Concat(language.Code, "_autocomplete_analyzer");
                    specificMappingAnlyzers.autocompleteSearchAnalyzer = string.Concat(language.Code, "_autocomplete_search_analyzer");
                }

                if (analyzerDefinition.Contains("dbl_metaphone"))
                {
                    specificMappingAnlyzers.phoneticIndexAnalyzer = string.Concat(language.Code, "_index_dbl_metaphone");
                    specificMappingAnlyzers.phoneticSearchAnalyzer = string.Concat(language.Code, "_search_dbl_metaphone");
                }
            }
            else
            {
                specificMappingAnlyzers.normalIndexAnalyzer = "whitespace";
                specificMappingAnlyzers.normalSearchAnalyzer = "whitespace";
                Log.Error(string.Format("could not find analyzer for language ({0}) for mapping. whitespace analyzer will be used instead", language.Code));
            }

            specificMappingAnlyzers.suffix = null;

            if (!language.IsDefault)
            {
                specificMappingAnlyzers.suffix = language.Code;
            }

            _mappingAnalyzers[language.Code] = specificMappingAnlyzers;

            return specificMappingAnlyzers;
        }

        private void GetAnalyzers(IEnumerable<LanguageObj> languages, out List<string> analyzers, out List<string> filters, out List<string> tokenizers, bool autocomplete = true)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (languages != null)
            {
                foreach (LanguageObj language in languages)
                {
                    string analyzer = _indexDefinitions.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, ES_VERSION));
                    string filter = _indexDefinitions.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, ES_VERSION));
                    string tokenizer = _indexDefinitions.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, ES_VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        Log.Error($"analyzer for language {language.Code} doesn't exist");
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

                if (autocomplete)
                {
                    filters.Add(PHRASE_STARTS_WITH_FILTER);
                    analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                    analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);
                }
            }
        }

        delegate string MapAnalyzersFunc(MappingAnalyzers mappingAnalyzers, string indexType);

        #region Index type

        private string GetMediaIndexType(LanguageObj language)
        {
            string type = MEDIA_INDEX_MAP_TYPE;
            if (!language.IsDefault)
            {
                type = $"{MEDIA_INDEX_MAP_TYPE}_{language.Code}";
            }
            return type;
        }

        private string GetEpgIndexType(LanguageObj language)
        {
            string indexTypePrefix = EPG_INDEX_MAP_TYPE;
            if (language.IsDefault) { return indexTypePrefix; }
            else { return $"{indexTypePrefix}_{language.Code}"; }
        }

        private string GetRecordingIndexType( LanguageObj language)
        {
            string indexTypePrefix = RECORDING_INDEX_MAP_TYPE;
            if (language.IsDefault) { return indexTypePrefix; }
            else { return $"{indexTypePrefix}_{language.Code}"; }
        }

        private string GetTagIndexType(LanguageObj language)
        {
            string type = TAG_INDEX_MAP_TYPE;
            if (!language.IsDefault)
            {
                type = string.Concat(TAG_INDEX_MAP_TYPE, "_", language.Code);
            }
            return type;
        }

        private string GetChannelMetadataIndexType(LanguageObj language)
        {
            return CHANNEL_INDEX_MAP_TYPE;
        }

        #endregion

        #region Index suffix

        private string GetGeneralIndexSuffix(LanguageObj language)
        {
            string suffix = null;
            if (!language.IsDefault)
            {
                suffix = language.Code;
            }
            return suffix;
        }

        private string GetChannelMetadataIndexSuffix(LanguageObj language)
        {
            return null;
        }

        #endregion

        #region Index name

        private string GetMediaIndexName(int groupId)
        {
            return $"{groupId}_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        private string GetEpgIndexName(int groupId)
        {
            var indexDate = DateTime.UtcNow;
            string dateString = indexDate.Date.ToString(ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT);
            return $"{groupId}_epg_v2_{dateString}";
        }

        private string GetTagIndexName(int groupId)
        {
            return $"{groupId}_metadata_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        private string GetChannelMetadataIndexName(int groupId)
        {
            return $"{groupId}_channel_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        private string GetRecordingIndexName(int groupId)
        {
            return $"{groupId}_recording_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        }

        #endregion

        #region Index alias

        private string GetMediaIndexAlias(int groupId)
        {
            return groupId.ToString();
        }

        private string GetEpgIndexAlias(int groupId)
        {
            return $"{groupId}_epg";
        }

        private string GetTagIndexAlias(int groupId)
        {
            return $"{groupId}_metadata";
        }

        private string GetChannelMetadataIndexAlias(int groupId)
        {
            return $"{groupId}_channel";
        }

        private string GetRecordingIndexAlias(int groupId)
        {
            return $"{groupId}_recording";
        }

        #endregion

        #endregion
    }
}