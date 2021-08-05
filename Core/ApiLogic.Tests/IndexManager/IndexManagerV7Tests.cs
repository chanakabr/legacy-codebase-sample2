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
using ChannelsSchema;
using Elasticsearch.Net;
using ElasticSearch.Utilities;
using Nest;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Policy = Polly.Policy;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.Response;

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
        private Mock<IChannelQueryBuilder> _mockChannelQueryBuilder;
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
                    new TtlService(),
                    _mockWatchRuleManager.Object,
                    _mockChannelQueryBuilder.Object
                );
        }
        
        private static string GetAnalyzerFromMockTcm()
        {
            return "\"eng_index_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\": [\"asciifolding\",\"lowercase\",\"eng_ngram_filter\"],\"char_filter\":[\"html_strip\"]}, \"eng_search_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\": [\"asciifolding\",\"lowercase\"],\"char_filter\":[\"html_strip\"]},\"eng_autocomplete_analyzer\":{\"type\":\"custom\",\"tokenizer\":\"whitespace\",\"filter\": [\"asciifolding\",\"lowercase\",\"eng_edgengram_filter\"],\"char_filter\":[\"html_strip\"]}, \"eng_autocomplete_search_analyzer\":{\"type\": \"custom\",\"tokenizer\": \"whitespace\",\"filter\": [\"asciifolding\",\"lowercase\"],\"char_filter\": [\"html_strip\"]}";
        }
        
        private string GetFilterFromMockTcm()
        {
            return "\"eng_ngram_filter\":{\"type\":\"nGram\",\"min_gram\":2,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}, \"eng_edgengram_filter\":{\"type\":\"edgeNGram\",\"min_gram\":1,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}";
        }
        
        private static void SetIndexRefreshTime(string index, Time refreshInterval, IElasticClient elasticClient)
        {
            var updateDisableIndexRefresh = new UpdateIndexSettingsRequest(index);
            updateDisableIndexRefresh.IndexSettings = new DynamicIndexSettings();
            updateDisableIndexRefresh.IndexSettings.RefreshInterval = refreshInterval;
            var updateSettingsResult = elasticClient.Indices.UpdateSettings(updateDisableIndexRefresh);
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
            _mockChannelQueryBuilder = _mockRepository.Create<IChannelQueryBuilder>();
            _mockElasticSearchCommonUtils = _mockRepository.Create<IElasticSearchCommonUtils>();
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(_mockElasticSearchCommonUtils.Object, ApplicationConfiguration.Current);
        }

        [Test]
        public void TestEpgv2Index()
        {
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var dateOfProgramsToIngest = DateTime.Now.AddDays(-1);

            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);
            var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));

            _mockElasticSearchCommonUtils.Setup(x => x.GetTcmValue(
                It.Is<string>(a => a.ToLower().Contains("filter"))
            )).Returns(GetFilterFromMockTcm());

            _mockElasticSearchCommonUtils.Setup(x => x.GetTcmValue(
                It.Is<string>(a => a.ToLower().Contains("analyzer"))
            )).Returns(GetAnalyzerFromMockTcm());

            var index = indexManager.SetupEpgV2Index(DateTime.Today, policy);
            Assert.IsNotEmpty(index);

            var refreshInterval = new Time(TimeSpan.FromSeconds(1));
            SetIndexRefreshTime(index, refreshInterval, elasticClient);
            var languageObjs = new List<LanguageObj>() { language }.ToDictionary(x => x.Code);

            //test upsert

            //Create 2 EPG bulk objects

            //EPG1
            var today = DateTime.Now;
            var epgCb = IndexManagerMockDataCreator.GeRandomEpgCb(today);
            var epgCbObjects = new List<EpgCB>();
            epgCbObjects.Add(epgCb);
            var epgBulk1 = GetEpgProgramBulkUploadObject(partnerId, dateOfProgramsToIngest, epgCb, epgCbObjects);

            //EPG2
            var tomorrow = today.AddDays(1);
            var epgCb2 = IndexManagerMockDataCreator.GeRandomEpgCb(tomorrow, name: "la movie2", description: "this is the movie description2");
            var epgCbObjects2 = new List<EpgCB>();
            epgCbObjects2.Add(epgCb2);
            var epgBulk2 = GetEpgProgramBulkUploadObject(partnerId, dateOfProgramsToIngest.AddDays(1), epgCb2, epgCbObjects2);

            //EPG3
            var epgCb3 = IndexManagerMockDataCreator.GeRandomEpgCb(tomorrow.AddDays(2), name: "la movie2", description: "this is the movie description2");
            var epgCbObjects3 = new List<EpgCB>();
            epgCbObjects3.Add(epgCb3);
            var epgBulk3 = GetEpgProgramBulkUploadObject(partnerId, dateOfProgramsToIngest.AddDays(2), epgCb3, epgCbObjects3);

            crudOperations.ItemsToAdd.Add(epgBulk1);
            crudOperations.ItemsToAdd.Add(epgBulk2);
            crudOperations.ItemsToAdd.Add(epgBulk3);

            var programsToIndex = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
                .ToList();

            //call upsert
            indexManager.UpsertProgramsToDraftIndex(programsToIndex, index, dateOfProgramsToIngest, language, languageObjs);

            indexManager.DeleteProgramsFromIndex(programsToIndex, index, languageObjs);

            var res = indexManager.FinalizeEpgV2Index(DateTime.Now);
            Assert.IsTrue(res);

            res = indexManager.FinalizeEpgV2Indices(new List<DateTime>() { DateTime.Today, DateTime.Now.AddDays(-1) }, policy);
            Assert.IsTrue(res);
        }

        private static EpgProgramBulkUploadObject GetEpgProgramBulkUploadObject(int partnerId, DateTime dateOfProgramsToIngest,
            EpgCB epgCb, List<EpgCB> epgCbObjects)
        {
            var epgItem = new EpgProgramBulkUploadObject()
            {
                GroupId = partnerId,
                StartDate = dateOfProgramsToIngest,
                EpgId = epgCb.EpgID,
                EpgCbObjects = epgCbObjects,
                EpgExternalId = $"1{epgCb.EpgID}"
            };
            return epgItem;
        }

        [Test]
        public void TestMedia()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupMediaIndex();

            Assert.IsNotEmpty(indexName);

            indexManager.PublishMediaIndex(indexName, true, true);

            // test switch alias
            indexName = indexManager.SetupMediaIndex();
            indexManager.PublishMediaIndex(indexName, true, true);

            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var randomMedia2 = IndexManagerMockDataCreator.GetRandomMedia(partnerId);

            randomMedia.m_sName = "upsert_test";
            var dictionary = new Dictionary<int, ApiObjects.SearchObjects.Media>() { };
            dictionary[language.ID] = randomMedia;
            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID, It.IsAny<CatalogGroupCache>()))
                .Returns(dictionary);
            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia2.m_nMediaID, It.IsAny<CatalogGroupCache>()))
                .Returns(new Dictionary<int, ApiObjects.SearchObjects.Media>()
                {
                    { language.ID, randomMedia2 }
                });


            var upsertMedia = indexManager.UpsertMedia(randomMedia.m_nMediaID);
            Assert.True(upsertMedia);
            
            indexManager.UpsertMedia(randomMedia2.m_nMediaID);

            var policy = Policy.HandleResult<List<SearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));
            var updateDates = policy.Execute(() => indexManager.GetAssetsUpdateDate(eObjectType.Media, new List<int>() { randomMedia.m_nMediaID }));

            Assert.IsNotEmpty(updateDates);
            Assert.AreEqual(randomMedia.m_nMediaID, updateDates[0].assetID);
            Assert.AreEqual(randomMedia.m_sUpdateDate, updateDates[0].UpdateDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT));

            int totalItems = 0;
            var updateDates2 = indexManager.GetAssetsUpdateDates(new List<Core.Catalog.Response.UnifiedSearchResult>()
            {
                new Core.Catalog.Response.UnifiedSearchResult()
                {
                    AssetId = randomMedia.m_nMediaID.ToString(),
                    AssetType = eAssetTypes.MEDIA
                },
                new Core.Catalog.Response.UnifiedSearchResult()
                {
                    AssetId = randomMedia2.m_nMediaID.ToString(),
                    AssetType = eAssetTypes.MEDIA
                }
            }, ref totalItems, 10, 0);
        }

        [Test]
        public void TestRecording()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupEpgIndex(true);

            Assert.IsNotEmpty(indexName);

            bool publishResult = indexManager.PublishEpgIndex(indexName, true, true, true);
            Assert.IsTrue(publishResult);
        }

        [Test]
        public void TestEpg()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);
            ulong epgId = (ulong)(1 + new Random().Next(10000));

            var indexName = indexManager.SetupEpgIndex(false);

            Assert.IsNotEmpty(indexName);

            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(partnerId);
            Dictionary<ulong, Dictionary<string, EpgCB>> epgs = new Dictionary<ulong, Dictionary<string, EpgCB>>();
            epgs[epgId] = new Dictionary<string, EpgCB>();
            epgs[epgId][language.Code] = new EpgCB()
            {
                EpgID = epgId,
                Name = $"test epg {epgId}",
                IsActive = true,
                ChannelID = randomChannel.m_nChannelID,
                StartDate = DateTime.UtcNow.AddHours(-1),
                EndDate = DateTime.UtcNow.AddHours(1),
                Status = 1,
                UpdateDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                EpgIdentifier = $"1{epgId}",
                Crid = $"2{epgId}",
                GroupID = partnerId,
                ParentGroupID = partnerId,
                Language = language.Code

            };

            _mockCatalogCache.Setup(x => x.GetLinearChannelSettings(It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(new Dictionary<string, ApiObjects.Catalog.LinearChannelSettings>());
            indexManager.AddEPGsToIndex(indexName, false, epgs, new Dictionary<long, List<int>>(), null);
            bool publishResult = indexManager.PublishEpgIndex(indexName, false, true, true);
            Assert.IsTrue(publishResult);

            bool deleteResult = indexManager.DeleteProgram(new List<long>() { Convert.ToInt64(epgId) }, null);
            Assert.IsTrue(deleteResult);
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
        public void TestSocialStatisticsData()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var stat1 = IndexManagerMockDataCreator.GetRandomSocialActionStat(partnerId);

            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();

            var res = indexManager.InsertSocialStatisticsData(stat1);
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
            Assert.IsTrue(deleteSocialAction);
        }

        [Test]
        public void TestTags()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var language2 = new ApiObjects.LanguageObj()
            {
                ID = _random.Next(1000) + 1,
                Code = "he",
                Name = "Hebrew",
                IsDefault = false,
            };
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language, language2 }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupTagsIndex();
            Assert.IsNotEmpty(indexName);
            var randomTag = IndexManagerMockDataCreator.GetRandomTag(language.ID);
            indexManager.InsertTagsToIndex(indexName, new List<TagValue>() { randomTag });

            var publishResult = indexManager.PublishTagsIndex(indexName, true, true);
            Assert.IsTrue(publishResult);

            var secondTag  = IndexManagerMockDataCreator.GetRandomTag(language2.ID);
            var updateResult = indexManager.UpdateTag(secondTag);
            Assert.AreEqual((int)ApiObjects.Response.eResponseStatus.OK, updateResult.Code);
        }

        [Test]
        public void TestIp2Country()
        {
            //var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var indexManager = GetIndexV7Manager(0);
            string indexName = indexManager.SetupIPToCountryIndex();
            string israel = "Israel";
            string usa = "USA";
            int usaId = 321;

            List<IPV4> ipv4 = new List<IPV4>()
            {
                new IPV4("1", 123, "il", israel, 26, 50),
                new IPV4("2", 123, "il", israel, 260, 500),
                new IPV4("3", usaId, "us", usa, 620, 625),
            };

            var tuple = IpToCountryHandler.GetIpRangesByNetworkStatic("2a00:801:300:d000::/52");
            List<IPV6> ipv6 = new List<IPV6>()
            {
               new IPV6(tuple, 1, "GB", "United Kingdom")
            };

            bool insertResult = indexManager.InsertDataToIPToCountryIndex(indexName, ipv4, ipv6);
            Assert.IsTrue(insertResult);

            bool publishResult = indexManager.PublishIPToCountryIndex(indexName);
            Assert.IsTrue(publishResult);

            bool searchSuccess = false;
            var policy = Policy.HandleResult<ApiObjects.Country>(x => x == null).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            // israel
            var country = policy.Execute(() => indexManager.GetCountryByIp("0.0.0.40", out searchSuccess));
            Assert.IsTrue(searchSuccess);
            Assert.IsNotNull(country);
            Assert.AreEqual(israel, country.Name);

            // still israel
            country = indexManager.GetCountryByIp("0.0.1.40", out searchSuccess);

            Assert.IsTrue(searchSuccess);
            Assert.IsNotNull(country);
            Assert.AreEqual(israel, country.Name);

            var country2 = indexManager.GetCountryByCountryName(usa.ToLower());
            Assert.IsNotNull(country2);
            Assert.AreEqual(country2.Id, usaId);

            // no country
            country = indexManager.GetCountryByIp("1.2.3.4", out searchSuccess);

            Assert.IsTrue(searchSuccess);
            Assert.IsNull(country);

            // no country - ipv6
            country = indexManager.GetCountryByIp("2001:0db8:85a3:0000:0000:8a2e:0370:7334", out searchSuccess);

            Assert.IsTrue(searchSuccess);
            Assert.IsNull(country);
        }

        [Test]
        public void TestChannelPercolator()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);

            var channel = IndexManagerMockDataCreator.GetRandomChannel(partnerId);
            channel.filterQuery = "name!~'aa'";
            channel.m_nChannelTypeID = (int)ChannelType.KSQL;
            channel.AssetUserRuleId = null;

            bool out1;
            Type out2;

            QueryContainerDescriptor<object> queryContainerDescriptor = new QueryContainerDescriptor<object>();

            var percolatdQuery = new PercolatedQuery()
            {
                Query = queryContainerDescriptor.Term(term => term.Field("is_active").Value(true))
            };

            _mockChannelManager.Setup(setup => setup
                    .GetGroupChannels(partnerId))
                .Returns(new List<Channel>() { channel });
            _mockCatalogManager.Setup(setup => setup
                    .GetUnifiedSearchKey(partnerId, It.IsAny<string>(), out out1, out out2))
                .Returns<int, string, bool, Type>((one, two, three, four) => new HashSet<string>() { two });
            _mockChannelQueryBuilder.Setup(s => s
                .GetChannelQuery(
                    It.IsAny<ESMediaQueryBuilder>(),
                    It.IsAny<ESUnifiedQueryBuilder>(),
                    It.IsAny<Channel>()))
                .Returns(percolatdQuery)
            ;

            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupMediaIndex();

            bool addResult = indexManager.AddChannelsPercolatorsToIndex(new HashSet<int>() { channel.m_nChannelID }, indexName);

            Assert.IsTrue(addResult);

            indexManager.PublishMediaIndex(indexName, true, true);

            var deleteResult = indexManager.DeleteChannelPercolator(new List<int>() { channel.m_nChannelID });
            Assert.IsTrue(deleteResult);
        }
        
        
        [Test]
        public void TestChannelMeteDataCrud()
        {
            /*var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var indexV7Manager = GetIndexV7Manager(randomPartnerId);
            string index = indexV7Manager.SetupChannelMetadataIndex();
            indexV7Manager.AddChannelsMetadataToIndex(index,new List<Channel>(){new Channel(){m_sName = "test chanlel"}});*/
            
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(randomPartnerId);

            var channelIndexName = indexManager.SetupChannelMetadataIndex();
            indexManager.AddChannelsMetadataToIndex(channelIndexName, new List<Channel>() { randomChannel });
            indexManager.PublishChannelsMetadataIndex(channelIndexName, true, true);

            var response = new GenericResponse<Channel>();
            response.Object = randomChannel;
            response.SetStatus(eResponseStatus.OK);

            _mockChannelManager
                .Setup(x => x.GetChannelById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<long>()))
                .Returns(response);

            var total = 0;
            var channelSearchDefinitions = new ChannelSearchDefinitions()
            {
                GroupId = randomPartnerId,
                PageIndex = 0,
                PageSize = 1,
                ExactSearchValue = randomChannel.m_sName
            };

            var searchPolicy = Policy.HandleResult<List<int>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var channels = searchPolicy.Execute(() =>
            {
                return indexManager.SearchChannels(channelSearchDefinitions, ref total);
            });

            Assert.AreEqual(1, total);
            Assert.IsNotNull(channels);
            Assert.IsNotEmpty(channels);
            Assert.AreEqual(randomChannel.m_nChannelID, channels[0]);

            // now let's combine with percolators - need media + epg indices for that

            var mediaIndexName = indexManager.SetupMediaIndex();
            indexManager.PublishMediaIndex(mediaIndexName, true, true);
            var epgIndexName = indexManager.SetupEpgIndex(false);
            indexManager.PublishEpgIndex(epgIndexName, false, true, true);

            var secondRandomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var upsertResult = indexManager.UpsertChannel(secondRandomChannel.m_nChannelID, secondRandomChannel);

            channelSearchDefinitions.ExactSearchValue = secondRandomChannel.m_sName;

            channels = searchPolicy.Execute(() =>
            {
                return indexManager.SearchChannels(channelSearchDefinitions, ref total);
            });

            Assert.AreEqual(1, total);
            Assert.IsNotNull(channels);
            Assert.IsNotEmpty(channels);
            Assert.AreEqual(secondRandomChannel.m_nChannelID, channels[0]);

            indexManager.DeleteChannel(secondRandomChannel.m_nChannelID);

            var deletePolicy = Policy.HandleResult<List<int>>(x => x != null && x.Count > 0).WaitAndRetry(3,
                retryAttempt => TimeSpan.FromSeconds(1));

            channels = deletePolicy.Execute(() =>
            {
                return indexManager.SearchChannels(channelSearchDefinitions, ref total);
            });

            Assert.AreEqual(0, total);

        }

        [Test]
        public void TestEpgV2Crud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageRus = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            languageRus.Code = "rus";
            languageRus.Name = "russs";
            var languageObjs = new List<LanguageObj>() {language, languageRus}.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] {language, languageRus},
                ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(randomPartnerId);
            var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));


            //act
            var setupEpgV2Index = indexManager.SetupEpgV2Index(DateTime.Now, policy);

            var refreshInterval = new Time(TimeSpan.FromSeconds(1));
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            SetIndexRefreshTime(setupEpgV2Index, refreshInterval, elasticClient);
            //assert
            Assert.IsNotEmpty(setupEpgV2Index);

            var dateOfProgramsToIngest = DateTime.Now.AddDays(-1);
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var epgCbObjects = new List<EpgCB>();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var today = DateTime.Now;
            var epgCb = IndexManagerMockDataCreator.GeRandomEpgCb(today);
            epgCbObjects.Add(epgCb);
            var epgCb2 = IndexManagerMockDataCreator.GeRandomEpgCb(today);
            epgCb2.Language = "rus";
            epgCb2.EpgID = epgCb.EpgID;

            epgCbObjects.Add(epgCb2);

            var epgId = epgCb.EpgID;

            var epgItem = new EpgProgramBulkUploadObject()
            {
                GroupId = randomPartnerId,
                StartDate = dateOfProgramsToIngest,
                EpgId = epgId,
                EpgCbObjects = epgCbObjects,
                EpgExternalId = $"{epgId}"
            };
            crudOperations.ItemsToAdd.Add(epgItem);


            indexManager.DeleteProgramsFromIndex(crudOperations.ItemsToDelete, setupEpgV2Index, languageObjs);

            var programsToIndex = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
                .ToList();

            indexManager.UpsertProgramsToDraftIndex(programsToIndex, setupEpgV2Index,
                dateOfProgramsToIngest, language, languageObjs);

            var searchPolicy = Policy.HandleResult<List<string>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var epgCbDocumentIdsByEpgId = searchPolicy.Execute(() =>
            {
                return indexManager.GetEpgCBDocumentIdsByEpgId(new[] {(long) epgId}, languageObjs.Values);
            });

            Assert.Contains(epgId.ToString(),epgCbDocumentIdsByEpgId,"Expected document id and epg id to be the same");
        }
    }
}