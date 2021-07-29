using ApiLogic.Catalog;
using ApiLogic.Tests.ConfigurationMocks;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using ElasticSearch.NEST;
using GroupsCacheManager;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.Tests.IndexManager.helpers;
using Utils = ElasticSearch.Common.Utils;
using Polly;
using ApiLogic.Tests.IndexManager.helpers;
using ApiObjects.BulkUpload;
using ApiObjects.Nest;
using Elasticsearch.Net;
using ElasticSearch.Utilities;
using Nest;
using Newtonsoft.Json;
using Policy = Polly.Policy;

namespace ApiLogic.Tests.IndexManager
{
    [TestFixture]
    public class IndexManagerV7Tests
    {
        private MockRepository _mockRepository;
        private Mock<IGroupManager> _mockGroupManager;
        private Mock<ICatalogManager> _mockCatalogManager;
        private Mock<IChannelManager> _mockChannelManager;
        private ESSerializerV2 _mockEsSerializerV2;
        private ElasticSearchApi _esApi;
        private ElasticSearchIndexDefinitions _elasticSearchIndexDefinitions;
        private Mock<ILayeredCache> _mockLayeredCache;
        private Mock<ICatalogCache> _mockCatalogCache;
        private Mock<IWatchRuleManager> _mockWatchRuleManager;
        private Mock<IElasticSearchCommonUtils> _mockElasticSearchCommonUtils;
        private static int _nextPartnerId = 10000;
        private Random _random;

        #region Helpers
        private IndexManagerV7 GetIndexV7Manager(int partnerId)
        {
            return new IndexManagerV7(partnerId,
                    NESTFactory.GetInstance(ApplicationConfiguration.Current),
                    ApplicationConfiguration.Current,
                    _mockGroupManager.Object,
                    _mockCatalogManager.Object,
                    _elasticSearchIndexDefinitions,
                    _mockChannelManager.Object,
                    _mockCatalogCache.Object,
                    new TtlService()
                );
        }
        #endregion

        [SetUp]
        public void SetUp()
        {
            _random = new Random();

            ApplicationConfiguration.InitDefaults();
            ApplicationConfiguration.Current._elasticSearchConfiguration = new MockElasticSearchConfiguration();

            _mockEsSerializerV2 = new ESSerializerV2();
            _mockRepository = new MockRepository(MockBehavior.Loose);
            _esApi = new ElasticSearchApi(ApplicationConfiguration.Current);
            _mockGroupManager = _mockRepository.Create<IGroupManager>();
            _mockCatalogManager = _mockRepository.Create<ICatalogManager>();
            _mockChannelManager = _mockRepository.Create<IChannelManager>();
            _mockCatalogCache = _mockRepository.Create<ICatalogCache>();
            _mockLayeredCache = _mockRepository.Create<ILayeredCache>();
            _mockWatchRuleManager = _mockRepository.Create<IWatchRuleManager>();
            _mockElasticSearchCommonUtils = _mockRepository.Create<IElasticSearchCommonUtils>();
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(_mockElasticSearchCommonUtils.Object, ApplicationConfiguration.Current);
        }


       
        [Test]
        public void TestAttr()
        {
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            
            IndexName indexName = $"{partnerId}_gil";
            var epgCb = new EpgCB();
            epgCb.Name = "la movie";
            epgCb.Language = "rus";
            epgCb.Description = "this is the movie description";
            var buildEpg = NestDataCreator.GetEpg(epgCb, 1, isOpc: true);            
            var indexResponse = elasticClient.Index(buildEpg, x => x.Index(indexName));
            var getResponse = elasticClient.Get<NestEpg>(indexResponse.Id,i=>i.Index(indexName)).Source;
        }

        [Test]
        public void TestSetupEPGV2Index()
        {
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var epgId = 1 + new Random().Next(1000);
            var epgCb = new EpgCB();
            epgCb.Name = "la movie";
            epgCb.Language = "en";
            epgCb.Description = "this is the movie description";
            epgCb.EpgID = (ulong) epgId;
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var dateOfProgramsToIngest = DateTime.Now.AddDays(-1);
            
            var epgCbObjects = new List<EpgCB>();
            epgCbObjects.Add(epgCb);
            

            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetRandomLanguage();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);
             var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));

            _mockElasticSearchCommonUtils.Setup(x => x.GetTcmValue(
                It.Is<string>(a => a.ToLower().Contains("filter"))
            ))
            .Returns("\"eng_ngram_filter\":{\"type\":\"nGram\",\"min_gram\":2,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}, \"eng_edgengram_filter\":{\"type\":\"edgeNGram\",\"min_gram\":1,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}");
            _mockElasticSearchCommonUtils.Setup(x => x.GetTcmValue(
                It.Is<string>(a => a.ToLower().Contains("analyzer"))
            ))
            .Returns("\"eng_index_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\": [\"asciifolding\",\"lowercase\",\"eng_ngram_filter\"],\"char_filter\":[\"html_strip\"]}, \"eng_search_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\": [\"asciifolding\",\"lowercase\"],\"char_filter\":[\"html_strip\"]},\"eng_autocomplete_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"whitespace\",\"filter\": [\"asciifolding\",\"lowercase\",\"eng_edgengram_filter\"],\"char_filter\":[\"html_strip\"]}, \"eng_autocomplete_search_analyzer\":{\"type\": \"custom\",\"tokenizer\": \"whitespace\",\"filter\": [\"asciifolding\",\"lowercase\"],\"char_filter\": [\"html_strip\"]}");

            var index = indexManager.SetupEpgV2Index(DateTime.Today, policy);
            Assert.IsNotEmpty(index);

            var refreshInterval = new Time(TimeSpan.FromSeconds(1));
            setIndexRefreshTime(index, refreshInterval, elasticClient);
            var languageObjs = new List<ApiObjects.LanguageObj>() { language }.ToDictionary(x => x.Code);
            
            //test upsert
            var epgItem = new EpgProgramBulkUploadObject()
            {
                GroupId = partnerId,
                StartDate = dateOfProgramsToIngest,
                EpgId = (ulong)epgId,
                EpgCbObjects = epgCbObjects,
                EpgExternalId = $"1{epgId}"
            };
            
            crudOperations.ItemsToAdd.Add(epgItem);
            var epgCbObjects2 = new List<EpgCB>();
            var epgCb2 = new EpgCB();
            epgCb2.Name = "la movie2";
            epgCb2.Language = "en";
            epgCb2.Description = "this is the movie description2";
            epgCb2.EpgID = (ulong) epgId+2;
            epgCbObjects.Add(epgCb2);
            
            var epgItem2 = new EpgProgramBulkUploadObject()
            {
                GroupId = partnerId,
                StartDate = dateOfProgramsToIngest,
                EpgId = (ulong)epgId,
                EpgCbObjects = epgCbObjects2,
                EpgExternalId = $"1{epgId}1"
            };
            
            crudOperations.ItemsToAdd.Add(epgItem2);
            var programsToIndex = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
                .ToList();
            
            indexManager.UpsertProgramsToDraftIndex(programsToIndex, index,dateOfProgramsToIngest, language, languageObjs);
            
            var res = indexManager.FinalizeEpgV2Index(DateTime.Now);
            Assert.IsTrue(res);

            res = indexManager.FinalizeEpgV2Indices(new List<DateTime>() {DateTime.Today, DateTime.Now.AddDays(-1)}, policy);
            Assert.IsTrue(res);                                    
        }

        private static void setIndexRefreshTime(string index, Time refreshInterval, IElasticClient elasticClient)
        {
            var updateDisableIndexRefresh = new UpdateIndexSettingsRequest(index);
            updateDisableIndexRefresh.IndexSettings = new DynamicIndexSettings();
            updateDisableIndexRefresh.IndexSettings.RefreshInterval = refreshInterval;
            var updateSettingsResult = elasticClient.Indices.UpdateSettings(updateDisableIndexRefresh);
        }

        [Test]
        public void TestMedia()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetRandomLanguage();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupMediaIndex();

            Assert.IsNotEmpty(indexName);

            indexManager.PublishMediaIndex(indexName, true, true);

            indexName = indexManager.SetupMediaIndex();
            indexManager.PublishMediaIndex(indexName, true, true);
        }

        [Test]
        public void TestRecording()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetRandomLanguage();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupEpgIndex(true);

            Assert.IsNotEmpty(indexName);

            bool publishResult = indexManager.PublishEpgIndex(indexName, true, true, true);
            Assert.IsTrue(publishResult);
        }

        [Test]
        public void TestBasics()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();
            Assert.IsTrue(result);
        }
        
        [Test]
        public void TestInsertSocialStatisticsData()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetRandomLanguage();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var stat1 = IndexManagerMockDataCreator.GetRandomSocialActionStat(partnerId);
            
            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();
            
            var res=indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(res);
            var socialSearch = new StatisticsActionSearchObj()
            {
                Action = stat1.Action,
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Date = stat1.Date
            };

            var deleteSocialAction = indexManager.DeleteSocialAction(socialSearch);
        }
    }
}