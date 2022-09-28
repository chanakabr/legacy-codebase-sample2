using ApiLogic.Catalog;
using ApiLogic.Tests.ConfigurationMocks;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
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
using ApiLogic.EPG;
using ApiLogic.IndexManager;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.Tests.IndexManager.helpers;
using Utils = ElasticSearch.Common.Utils;
using Polly;
using ApiObjects.BulkUpload;
using ChannelsSchema;
using Elasticsearch.Net;
using ElasticSearch.Utilities;
using Nest;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Policy = Polly.Policy;
using ApiLogic.IndexManager.NestData;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.QueryBuilders.NestQueryBuilders;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.Epg;
using ApiObjects.Response;
using Core.Catalog.Response;
using TvinciCache;
using Catalog.Response;
using Core.GroupManagers;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace ApiLogic.Tests.IndexManager
{
    //[TestFixture]
    public class IndexManagerV7Tests
    {
        private MockRepository _mockRepository;
        private Mock<IGroupManager> _mockGroupManager;
        private Mock<ICatalogManager> _mockCatalogManager;
        private Mock<IChannelManager> _mockChannelManager;
        private ESSerializerV2 _mockEsSerializerV2;
        private ElasticSearchApi _esApi;
        private ElasticSearchIndexDefinitionsNest _elasticSearchIndexDefinitions;
        private Mock<ILayeredCache> _mockLayeredCache;
        private Mock<ICatalogCache> _mockCatalogCache;
        private Mock<IWatchRuleManager> _mockWatchRuleManager;
        private Mock<IChannelQueryBuilder> _mockChannelQueryBuilder;
        private Mock<IElasticSearchCommonUtils> _mockElasticSearchCommonUtils;
        private Mock<IGroupsFeatures> _mockGroupsFeatures;
        private Mock<ISortingService> _mockSortingService;
        private Mock<IStartDateAssociationTagsSortStrategy> _mockStartDateAssociationTagsSortStrategy;
        private Mock<IStatisticsSortStrategy> _mockStatisticsSortStrategy;
        private Mock<IEsSortingService> _mockEsSortingService;
        private Mock<ISortingAdapter> _mockSortingAdapter;
        private Mock<IUnifiedQueryBuilderInitializer> _mockQueryInitializer;

        private static int _nextPartnerId = 10000;
        private Random _random;
        private INamingHelper _mockNamingHelper;
        private IGroupSettingsManager _mockGroupSettingsManager;

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
                    _mockChannelQueryBuilder.Object,
                    _mockGroupsFeatures.Object,
                    _mockLayeredCache.Object,
                    _mockNamingHelper,
                    _mockGroupSettingsManager,
                    _mockSortingService.Object,
                    _mockStartDateAssociationTagsSortStrategy.Object,
                    _mockStatisticsSortStrategy.Object,
                    _mockSortingAdapter.Object,
                    _mockEsSortingService.Object,
                    _mockQueryInitializer.Object);
        }

        private INamingHelper GetMockNamingHelper()
        {
            var epgV2ConfigManagerMock = new Mock<IEpgPartnerConfigurationManager>(MockBehavior.Loose);
            epgV2ConfigManagerMock.Setup(m => m.GetEpgV2Configuration(It.IsAny<int>())).Returns(new EpgV2PartnerConfiguration()
            {
                IsEpgV2Enabled = true,
                FutureIndexCompactionStart = 7,
                PastIndexCompactionStart = 0,
            });
            return new NamingHelper(epgV2ConfigManagerMock.Object);
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


        //[SetUp]
        public void SetUp()
        {
            _random = new Random();

            ApplicationConfiguration.InitDefaults();
            ApplicationConfiguration.Current._elasticSearchConfiguration = new MockElasticSearchConfiguration();

            _mockEsSerializerV2 = new ESSerializerV2();
            _mockRepository = new MockRepository(MockBehavior.Loose);
            _esApi = new ElasticSearchApi(ApplicationConfiguration.Current);
            Assert.True(_esApi.HealthCheck()); // fail fast
            _mockGroupManager = _mockRepository.Create<IGroupManager>();
            _mockCatalogManager = _mockRepository.Create<ICatalogManager>();
            _mockChannelManager = _mockRepository.Create<IChannelManager>();
            _mockCatalogCache = _mockRepository.Create<ICatalogCache>();
            _mockLayeredCache = _mockRepository.Create<ILayeredCache>();
            _mockWatchRuleManager = _mockRepository.Create<IWatchRuleManager>();
            _mockChannelQueryBuilder = _mockRepository.Create<IChannelQueryBuilder>();
            _mockElasticSearchCommonUtils = _mockRepository.Create<IElasticSearchCommonUtils>();
            _mockGroupsFeatures = _mockRepository.Create<IGroupsFeatures>();
            _mockSortingService = _mockRepository.Create<ISortingService>();
            _mockQueryInitializer = _mockRepository.Create<IUnifiedQueryBuilderInitializer>();
            _mockStartDateAssociationTagsSortStrategy = _mockRepository.Create<IStartDateAssociationTagsSortStrategy>();
            _mockStatisticsSortStrategy = _mockRepository.Create<IStatisticsSortStrategy>();
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitionsNest(_mockElasticSearchCommonUtils.Object, ApplicationConfiguration.Current);
            
            var epgV2ConfigManagerMock = new Mock<IEpgPartnerConfigurationManager>(MockBehavior.Loose);
            epgV2ConfigManagerMock.Setup(m => m.GetEpgV2Configuration(It.IsAny<int>())).Returns(new EpgV2PartnerConfiguration()
            {
                // all tests assume they use epg v1 unless a method for epg v2 is explcitly called
                IsEpgV2Enabled = false,
                FutureIndexCompactionStart = 7,
                PastIndexCompactionStart = 0,
            });
            
            _mockNamingHelper = new NamingHelper(epgV2ConfigManagerMock.Object);
            _mockGroupSettingsManager = new GroupSettingsManager(_mockLayeredCache.Object, epgV2ConfigManagerMock.Object);
        }

        //[Test]
        public void TestEpgv2Index()
        {
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var dateOfProgramsToIngest = DateTime.UtcNow.AddDays(-1);

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

            var indexName = _mockNamingHelper.GetDailyEpgIndexName(partnerId, DateTime.Now);
            var index = indexManager.SetupEpgV2Index(indexName);
            Assert.IsNotEmpty(index);

            var refreshInterval = new Time(TimeSpan.FromSeconds(1));
            SetIndexRefreshTime(index, refreshInterval, elasticClient);
            var languageObjs = new List<LanguageObj>() { language }.ToDictionary(x => x.Code);

            //test upsert

            //Create 2 EPG bulk objects

            //EPG1
            var today = DateTime.UtcNow;
            var epgCb = IndexManagerMockDataCreator.GetRandomEpgCb(today);
            var epgCbObjects = new List<EpgCB>();
            epgCbObjects.Add(epgCb);
            var epgBulk1 = GetEpgProgramBulkUploadObject(partnerId, dateOfProgramsToIngest, epgCb, epgCbObjects);

            //EPG2
            var tomorrow = today.AddDays(1);
            var epgCb2 = IndexManagerMockDataCreator.GetRandomEpgCb(tomorrow, name: "la movie2", description: "this is the movie description2");
            var epgCbObjects2 = new List<EpgCB>();
            epgCbObjects2.Add(epgCb2);
            var epgBulk2 = GetEpgProgramBulkUploadObject(partnerId, dateOfProgramsToIngest.AddDays(1), epgCb2, epgCbObjects2);

            //EPG3
            var epgCb3 = IndexManagerMockDataCreator.GetRandomEpgCb(tomorrow.AddDays(2), name: "la movie2", description: "this is the movie description2");
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
            indexManager.UpsertPrograms(programsToIndex, index, language, languageObjs);

            indexManager.DeletePrograms(programsToIndex, index, languageObjs);

            var res = indexManager.ForceRefreshEpgIndex(indexName);
            Assert.IsTrue(res);

            res = indexManager.FinalizeEpgV2Indices(new List<DateTime>() { DateTime.Today, DateTime.UtcNow.AddDays(-1) });
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

        //[Test]
        public void TestMedia()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            // test switch alias
            indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);
            Media randomMedia, randomMedia2;
            var media = SetUpMockMedia(partnerId, language, 2);
            randomMedia = media[0];
            randomMedia2 = media[1];

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

            var channel = IndexManagerMockDataCreator.GetRandomChannel(partnerId);
            channel.filterQuery = "name!~'aa'";
            channel.m_nChannelTypeID = (int)ChannelType.KSQL;
            channel.AssetUserRuleId = null;

            bool out1;
            Type out2;

            QueryContainerDescriptor<object> queryContainerDescriptor = new QueryContainerDescriptor<object>();

            var percolateQuery = new NestPercolatedQuery()
            {
                Query = queryContainerDescriptor.Term(term => term.Field("is_active").Value(true)),
                ChannelId = channel.m_nChannelID
            };

            _mockChannelManager.Setup(setup => setup
                    .GetGroupChannels(partnerId))
                .Returns(new List<Channel>() { channel });
            _mockCatalogManager.Setup(setup => setup
                    .GetUnifiedSearchKey(partnerId, It.IsAny<string>()))
                .Returns<int, string>((one, two) => new HashSet<BooleanLeafFieldDefinitions>() { new BooleanLeafFieldDefinitions() { Field = two } });
            _mockChannelQueryBuilder.Setup(s => s
                    .GetChannelQuery(
                        It.IsAny<Channel>()))
                .Returns(percolateQuery);

            indexManager.SetupChannelPercolatorIndex(indexDate);
            var addResult = indexManager.AddChannelsPercolatorsToIndex(new HashSet<int>() { channel.m_nChannelID }, indexDate);
            Assert.IsTrue(addResult);

            indexManager.PublishChannelPercolatorIndex(indexDate, true, true);

            var searchPolicy = Policy.HandleResult<List<int>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var mediaChannels = searchPolicy.Execute(() =>
            {
                return indexManager.GetMediaChannels(randomMedia.m_nMediaID);
            });

            Assert.Contains(channel.m_nChannelID, mediaChannels);
            var mediaBelongToChannels =
                indexManager.DoesMediaBelongToChannels(new List<int>() { channel.m_nChannelID }, randomMedia.m_nMediaID);
            Assert.IsTrue(mediaBelongToChannels);

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

        private List<Media> SetUpMockMedia(int partnerId, LanguageObj language, int count = 2)
        {
            List<Media> media = new List<Media>();

            for (int i = 0; i < count; i++)
            {
                var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
                var dictionary = new Dictionary<int, ApiObjects.SearchObjects.Media>() { };
                dictionary[language.ID] = randomMedia;
                _mockCatalogManager
                    .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID))
                    .Returns(dictionary);

                media.Add(randomMedia);
            }

            return media;
        }

        //[Test]
        public void TestRecording()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, true);

            Assert.IsNotEmpty(indexName);

            bool publishResult = indexManager.PublishEpgIndex(indexName, true, true, true);
            Assert.IsTrue(publishResult);
        }

        //[Test]
        public void TestEpg()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);
            ulong epgId = (ulong)(1 + new Random().Next(10000));

            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

            Assert.IsNotEmpty(indexName);

            var startDate = DateTime.UtcNow.AddHours(-1);
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(partnerId);
            Dictionary<ulong, Dictionary<string, EpgCB>> epgs = new Dictionary<ulong, Dictionary<string, EpgCB>>();
            epgs[epgId] = new Dictionary<string, EpgCB>();
            epgs[epgId][language.Code] = new EpgCB()
            {
                EpgID = epgId,
                Name = $"test epg {epgId}",
                IsActive = true,
                ChannelID = randomChannel.m_nChannelID,
                StartDate = startDate,
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

            var searchPolicy = Policy.HandleResult<List<string>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var esOrderObjs = new List<ESOrderObj>();
            esOrderObjs.Add(new ESOrderObj() { m_eOrderDir = OrderDir.ASC, m_sOrderValue = "start_date" });
            esOrderObjs.Add(new ESOrderObj() { m_eOrderDir = OrderDir.DESC, m_sOrderValue = "end_date" });
            var channelPrograms = searchPolicy.Execute(() => indexManager.GetChannelPrograms(randomChannel.m_nChannelID,
               DateTime.UtcNow.AddDays(-2),
               DateTime.UtcNow.AddDays(3),
               esOrderObjs));
            Assert.AreEqual($"{epgId}", channelPrograms.FirstOrDefault());

            var partialUpdateResult = indexManager.UpdateEpgsPartial(new EpgPartialUpdate[]
            {
                new EpgPartialUpdate()
                {
                    EpgId = epgId,
                    Language = language.Code,
                    StartDate = startDate,
                    EpgPartial = new EpgPartial()
                    {
                        Regions = new int[] { 26 }
                    }
                }
            });

            Assert.IsTrue(partialUpdateResult);

            var searchPolicy2 = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));
            int totalItems = 0;

            var searchResults = searchPolicy2.Execute( () => 
                indexManager.UnifiedSearch(new UnifiedSearchDefinitions() {
                    groupId = partnerId,
                    pageSize = 500,
                    shouldSearchEpg = true,
                    epgDaysOffest = 7,
                    regionIds = new List<int>() { 26 }
                }, ref totalItems));

            Assert.IsNotNull(searchResults);
            Assert.IsNotEmpty(searchResults);
            Assert.AreEqual(1, searchResults.Count);
            Assert.AreEqual(epgId.ToString(), searchResults[0].AssetId);

            bool deleteResult = indexManager.DeleteProgram(new List<long>() { Convert.ToInt64(epgId) });
            Assert.IsTrue(deleteResult);
        }

        //[Test]
        public void TestSocialStatisticsData()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);

            #region Populate data

            var stat1 = IndexManagerMockDataCreator.GetRandomSocialActionStat(partnerId);

            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();

            var insertSocialStatisticsData = indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(insertSocialStatisticsData);

            insertSocialStatisticsData = indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(insertSocialStatisticsData);

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-1),
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Count = 2
            });

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Count = 6
            });

            int randomMediaId = _random.Next(1000) + 1000;
            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = randomMediaId,
                MediaType = stat1.MediaType,
                Count = 4
            });

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "rates",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Count = 1,
                RateValue = 4
            });

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "rates",
                Date = DateTime.UtcNow.AddHours(-3),
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Count = 1,
                RateValue = 5
            });

            #endregion

            var socialSearch = new StatisticsActionSearchObj()
            {
                Action = stat1.Action,
                GroupID = partnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Date = stat1.Date
            };

            var assetIDsToStatsMapping = new Dictionary<int, AssetStatsResult>();
            assetIDsToStatsMapping[stat1.MediaID] = new AssetStatsResult();
            assetIDsToStatsMapping[randomMediaId] = new AssetStatsResult();
            var endDate = DateTime.UtcNow.AddDays(1);
            var startDate = DateTime.UtcNow.AddDays(-2);
            var assetIDs = new List<int>() { stat1.MediaID, randomMediaId };
            var statsType = StatsType.MEDIA;

            var searchPolicy = Policy.HandleResult<Dictionary<int, AssetStatsResult>>(x =>
                x.Values == null || x.Values.Count == 0 ||
                (x.ContainsKey(stat1.MediaID) && x[stat1.MediaID].m_nLikes <= 0)).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var res = searchPolicy.Execute(() =>
            {
                indexManager.GetAssetStats(assetIDs, startDate, endDate, statsType, ref assetIDsToStatsMapping);
                return assetIDsToStatsMapping;
            });

            Assert.IsNotEmpty(res);
            Assert.AreEqual(2, res[stat1.MediaID].m_nLikes);
            Assert.AreEqual(8, res[stat1.MediaID].m_nViews);
            Assert.AreEqual(4.5, res[stat1.MediaID].m_dRate);

            var deleteSocialAction = indexManager.DeleteSocialAction(socialSearch);
            Assert.IsTrue(deleteSocialAction);

            var epgStatsMapping = new Dictionary<int, AssetStatsResult>() { { 123456, new AssetStatsResult() } };
            indexManager.GetAssetStats(new List<int>() { 123456 }, DateTime.MinValue, DateTime.MaxValue, StatsType.EPG, ref epgStatsMapping);
        }

        //[Test]
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

            var indexName = indexManager.SetupTagsIndex(DateTime.UtcNow);
            Assert.IsNotEmpty(indexName);
            var randomTag = IndexManagerMockDataCreator.GetRandomTag(language.ID);
            indexManager.InsertTagsToIndex(indexName, new List<TagValue>() { randomTag });

            var publishResult = indexManager.PublishTagsIndex(indexName, true, true);
            Assert.IsTrue(publishResult);

            var secondTag = IndexManagerMockDataCreator.GetRandomTag(language2.ID);
            var updateResult = indexManager.UpdateTag(secondTag);
            Assert.AreEqual((int)ApiObjects.Response.eResponseStatus.OK, updateResult.Code);

            var searchPolicy = Policy.HandleResult<List<TagValue>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            TagSearchDefinitions tagSearchDefinitions = new TagSearchDefinitions()
            {
                ExactSearchValue = randomTag.value,
                PageSize = 10,
                TopicId = randomTag.topicId,
                GroupId = partnerId,
            };

            var searchResult = searchPolicy.Execute(() => indexManager.SearchTags(tagSearchDefinitions, out int totalItems));

            Assert.IsNotEmpty(searchResult);
            Assert.AreEqual(randomTag.value, searchResult[0].value);

            tagSearchDefinitions = new TagSearchDefinitions()
            {
                AutocompleteSearchValue = randomTag.value.Substring(0, 3),
                TagIds = new List<long>() { randomTag.tagId },
                Language = language,
                PageSize = 10,
                TopicId = randomTag.topicId,
                GroupId = partnerId,
            };

            searchResult = searchPolicy.Execute(() => indexManager.SearchTags(tagSearchDefinitions, out int totalItems));

            Assert.IsNotEmpty(searchResult);
            Assert.AreEqual(randomTag.value, searchResult[0].value);
        }

        //[Test]
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

        //[Test]
        public void TestChannelPercolator()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);

            var channel = IndexManagerMockDataCreator.GetRandomChannel(partnerId);
            channel.filterQuery = "name!~'aa'";
            channel.m_nChannelTypeID = (int)ChannelType.KSQL;
            channel.AssetUserRuleId = null;

            QueryContainerDescriptor<object> queryContainerDescriptor = new QueryContainerDescriptor<object>();

            var percolatdQuery = new NestPercolatedQuery()
            {
                Query = queryContainerDescriptor.Term(term => term.Field("is_active").Value(true)),
                ChannelId = channel.m_nChannelID
            };

            _mockChannelManager.Setup(setup => setup
                    .GetGroupChannels(partnerId))
                .Returns(new List<Channel>() { channel });
            _mockCatalogManager.Setup(setup => setup
                    .GetUnifiedSearchKey(partnerId, It.IsAny<string>()))
                .Returns<int, string>((one, two) => new HashSet<BooleanLeafFieldDefinitions>() { new BooleanLeafFieldDefinitions() { Field = two } });
            _mockChannelQueryBuilder.Setup(s => s
                .GetChannelQuery(
                    It.IsAny<Channel>()))
                .Returns(percolatdQuery)
            ;

            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            indexManager.SetupChannelPercolatorIndex(indexDate);
            bool addResult = indexManager.AddChannelsPercolatorsToIndex(new HashSet<int>() { channel.m_nChannelID }, indexDate);

            Assert.IsTrue(addResult);

            indexManager.PublishMediaIndex(indexDate, true, true);
            indexManager.PublishChannelPercolatorIndex(indexDate, true, true);

            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var randomMedia2 = IndexManagerMockDataCreator.GetRandomMedia(partnerId);

            randomMedia.m_sName = "upsert_test";
            var dictionary = new Dictionary<int, ApiObjects.SearchObjects.Media>() { };
            dictionary[language.ID] = randomMedia;
            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID))
                .Returns(dictionary);
            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia2.m_nMediaID))
                .Returns(new Dictionary<int, ApiObjects.SearchObjects.Media>()
                {
                    { language.ID, randomMedia2 }
                });

            var upsertResult = indexManager.UpsertMedia(randomMedia.m_nMediaID);
            Assert.IsTrue(upsertResult);

            var searchPolicy = Policy.HandleResult<List<int>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var mediaChannels = searchPolicy.Execute(() => indexManager.GetMediaChannels(randomMedia.m_nMediaID));
            Assert.IsNotEmpty(mediaChannels);
            Assert.AreEqual(channel.m_nChannelID, mediaChannels.First());

            var deleteResult = indexManager.DeleteChannelPercolator(new List<int>() { channel.m_nChannelID });
            Assert.IsTrue(deleteResult);
        }

        //[Test]
        public void TestChannelMeteDataCrud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(randomPartnerId);

            var channelIndexName = indexManager.SetupChannelMetadataIndex(DateTime.UtcNow);
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

            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);
            var epgIndexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);
            indexManager.PublishEpgIndex(epgIndexName, false, true, true);

            var secondRandomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);

            QueryContainerDescriptor<object> queryContainerDescriptor = new QueryContainerDescriptor<object>();

            var percolateQuery = new NestPercolatedQuery()
            {
                Query = queryContainerDescriptor.Term(term => term.Field("is_active").Value(true)),
                ChannelId = secondRandomChannel.m_nChannelID
            };

            _mockChannelQueryBuilder.Setup(s => s
                    .GetChannelQuery(
                        It.IsAny<Channel>()))
                .Returns(percolateQuery);

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

        //[Test]
        public void TestEpgV2Crud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageRus = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            languageRus.Code = "rus";
            languageRus.Name = "russs";
            var languageObjs = new List<LanguageObj>() { language, languageRus }.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language, languageRus },
                ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(randomPartnerId);
            var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));


            //act
            var indexName = _mockNamingHelper.GetDailyEpgIndexName(randomPartnerId, DateTime.Now);
            var setupEpgV2Index = indexManager.SetupEpgV2Index(indexName);

            var refreshInterval = new Time(TimeSpan.FromSeconds(1));
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            SetIndexRefreshTime(setupEpgV2Index, refreshInterval, elasticClient);
            //assert
            Assert.IsNotEmpty(setupEpgV2Index);

            var dateOfProgramsToIngest = DateTime.UtcNow.AddDays(-1);
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var epgCbObjects = new List<EpgCB>();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var today = DateTime.UtcNow;
            var epgCb = IndexManagerMockDataCreator.GetRandomEpgCb(today);
            epgCbObjects.Add(epgCb);
            var epgCb2 = IndexManagerMockDataCreator.GetRandomEpgCb(today);
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

            var channelId = 123456;
            var epgCb3 = IndexManagerMockDataCreator.GetRandomEpgCb(DateTime.UtcNow.AddMinutes(-30));
            epgCb3.EndDate = DateTime.UtcNow.AddMinutes(30);
            epgCb3.ChannelID = channelId;
            var epgItem2 = new EpgProgramBulkUploadObject()
            {
                GroupId = randomPartnerId,
                StartDate = DateTime.Today,
                EpgId = epgCb3.EpgID,
                EpgCbObjects = new List<EpgCB>() { epgCb3 },
                EpgExternalId = $"{epgCb3.EpgID}"
            };
            crudOperations.ItemsToAdd.Add(epgItem2);

            indexManager.DeletePrograms(crudOperations.ItemsToDelete, setupEpgV2Index, languageObjs);

            var programsToIndex = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
                .ToList();

            indexManager.UpsertPrograms(programsToIndex, setupEpgV2Index, language, languageObjs);

            var searchPolicy = Policy.HandleResult<List<string>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var epgCbDocumentIdsByEpgId = searchPolicy.Execute(() =>
            {
                return indexManager.GetEpgCBDocumentIdsByEpgId(new[] { (long)epgId }, languageObjs.Values);
            });

            Assert.Contains(epgId.ToString(), epgCbDocumentIdsByEpgId, "Expected document id and epg id to be the same");


            var searchPolicy2 = Policy.HandleResult<IList<EpgProgramInfo>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var currentProgramsInfos = searchPolicy2.Execute(() => indexManager.GetCurrentProgramInfosByDate(channelId, DateTime.UtcNow, DateTime.UtcNow));
            Assert.IsNotNull(currentProgramsInfos);
            Assert.IsNotEmpty(currentProgramsInfos);
            Assert.AreEqual(1, currentProgramsInfos.Count);
            Assert.AreEqual(epgCb3.EpgIdentifier, currentProgramsInfos[0].EpgExternalId);
        }

        //[Test]
        public void TestEpgV1Crud()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageObjs = new List<ApiObjects.LanguageObj>() { language }.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);
            var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));
            ulong epgId = (ulong)(1 + new Random().Next(10000));

            string indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

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
                Language = "eng"
            };

            _mockCatalogCache.Setup(x => x.GetLinearChannelSettings(It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(new Dictionary<string, ApiObjects.Catalog.LinearChannelSettings>());
            indexManager.AddEPGsToIndex(indexName, false, epgs, new Dictionary<long, List<int>>(), null);
            indexManager.PublishEpgIndex(indexName, false, true, true);

            var searchPolicy = Policy.HandleResult<IList<EpgProgramBulkUploadObject>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var epgProgramBulkUploadObjects = searchPolicy.Execute(() => indexManager.GetCurrentProgramsByDate(randomChannel.m_nChannelID, DateTime.UtcNow.AddDays(-2),
               DateTime.UtcNow.AddDays(1)));
            Assert.IsNotEmpty(epgProgramBulkUploadObjects);
            Assert.AreEqual(epgId, epgProgramBulkUploadObjects[0].EpgId);
        }

        //[Test]
        public void TestUnifiedSearch()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            var media = SetUpMockMedia(partnerId, language, 10);

            foreach (var m in media)
            {
                indexManager.UpsertMedia(m.m_nMediaID);
            }
            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

            Assert.IsNotEmpty(indexName);

            indexManager.PublishEpgIndex(indexName, false, true, true);

            int totalCount = 0;
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions()
            {
                groupId = partnerId,
                pageSize = 500,
                shouldSearchMedia = true,
                filterPhrase = new BooleanLeaf("is_active", true, typeof(bool))
            };

            var searchPolicy = Policy
                .HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count < 10).WaitAndRetry(
                    5,
                    retryAttempt => TimeSpan.FromSeconds(1));

            var searchResults = searchPolicy.Execute(() => indexManager.UnifiedSearch(definitions, ref totalCount));

            Assert.AreEqual(10, totalCount,"total count is not as expected");
            Assert.AreEqual(10, searchResults.Count,"searchResults.Count  is not as expected");

            definitions = new UnifiedSearchDefinitions()
            {
                groupId = partnerId,
                pageSize = 26,
                shouldSearchMedia = true,
                filterPhrase = new BooleanPhrase(new List<BooleanPhraseNode>()
                {
                    new BooleanLeaf("is_active", true, typeof(bool)),
                    new BooleanLeaf("name", "TES", typeof(string), ComparisonOperator.Contains, true, true)
                    {
                        fieldType = eFieldType.LanguageSpecificField
                    }
                },
                eCutType.And)
            };

            searchResults = indexManager.UnifiedSearch(definitions, ref totalCount);

            Assert.AreEqual(10, totalCount);
            Assert.AreEqual(10, searchResults.Count);

            definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                filterPhrase = new BooleanPhrase(new List<BooleanPhraseNode>()
                {
                    new BooleanLeaf("is_active", true, typeof(bool)),
                    new BooleanLeaf("name", "ayy", typeof(string), ComparisonOperator.NotContains, true, true)
                    {
                        fieldType = eFieldType.LanguageSpecificField
                    }
                },
                eCutType.And),
                langauge = language,
                order = new OrderObj()
                {
                    m_eOrderBy = OrderBy.CREATE_DATE,
                    m_eOrderDir = OrderDir.DESC
                },
                parentMediaTypes = new Dictionary<int, int>() { { 1, 2 } },
                preference = "123456",
                shouldAddIsActiveTerm = true,
                shouldGetDomainsRecordings = true,
                shouldSearchEpg = true,
                shouldUseCatalogStartDateForMedia = true,
                shouldUseEndDateForEpg = true,
                shouldUseSearchEndDate = true,
                shouldUseStartDateForEpg = true,
                shouldUseStartDateForMedia = true,
            };

            searchResults = indexManager.UnifiedSearch(definitions, ref totalCount);

            Assert.AreEqual(10, totalCount);
            Assert.AreEqual(10, searchResults.Count);

            var groupBy = new GroupByDefinition()
            {
                Key = "name",
                Type = eFieldType.LanguageSpecificField
            };

            definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                filterPhrase = new BooleanPhrase(new List<BooleanPhraseNode>()
                {
                    new BooleanLeaf("is_active", true, typeof(bool)),
                    new BooleanLeaf("name", "ayy", typeof(string), ComparisonOperator.NotContains, true, true)
                },
                eCutType.And),
                langauge = language,
                order = new OrderObj()
                {
                    m_eOrderBy = OrderBy.CREATE_DATE,
                    m_eOrderDir = OrderDir.DESC
                },
                parentMediaTypes = new Dictionary<int, int>() { { 1, 2 } },
                preference = "123456",
                shouldAddIsActiveTerm = true,
                shouldGetDomainsRecordings = true,
                shouldSearchEpg = true,
                shouldUseCatalogStartDateForMedia = true,
                shouldUseEndDateForEpg = true,
                shouldUseSearchEndDate = true,
                shouldUseStartDateForEpg = true,
                shouldUseStartDateForMedia = true,
                groupBy = new List<GroupByDefinition>() { groupBy },
                distinctGroup = groupBy,
                topHitsCount = 1
            };

            searchResults = indexManager.UnifiedSearch(definitions, ref totalCount, out var aggs);

            Assert.AreEqual(10, totalCount);
            Assert.IsNotNull(aggs);
            Assert.IsNotEmpty(aggs);
            Assert.AreEqual(1, aggs.Count);
            Assert.AreEqual(10, aggs[0].totalItems);
        }


        //[Test]
        public void TestUnifiedSearchGroupByAndStuff()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            var media1 = SetUpMockMedia(partnerId, language, 10);
            var utcNow = DateTime.UtcNow;
            foreach (var m in media1)
            {
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media2 = SetUpMockMedia(partnerId, language, 6);

            foreach (var m in media2)
            {
                m.m_sName = "sunny one";
                
                m.m_sCreateDate = utcNow.AddDays(-1).ToString(Utils.ES_DATE_FORMAT);
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media3 = SetUpMockMedia(partnerId, language, 5);

            foreach (var m in media3)
            {
                m.m_sName = "sunny two";
                m.m_sCreateDate = utcNow.AddDays(-2).ToString(Utils.ES_DATE_FORMAT); 
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

            Assert.IsNotEmpty(indexName);

            indexManager.PublishEpgIndex(indexName, false, true, true);
            var groupBy = new GroupByDefinition()
            {
                Key = "name",
                Type = eFieldType.LanguageSpecificField
            };

            var definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                filterPhrase = new BooleanPhrase(new List<BooleanPhraseNode>()
                {
                    new BooleanLeaf("is_active", true, typeof(bool)),
                    new BooleanLeaf("name", "sun", typeof(string), ComparisonOperator.WordStartsWith, true, true)
                    {
                        fieldType = eFieldType.LanguageSpecificField
                    }
                },
                eCutType.And),
                langauge = language,
                order = new OrderObj()
                {
                    m_eOrderBy = OrderBy.CREATE_DATE,
                    m_eOrderDir = OrderDir.DESC
                },
                parentMediaTypes = new Dictionary<int, int>() { { 1, 2 } },
                preference = "123456",
                shouldAddIsActiveTerm = true,
                shouldGetDomainsRecordings = true,
                shouldSearchEpg = true,
                shouldUseCatalogStartDateForMedia = true,
                shouldUseEndDateForEpg = true,
                shouldUseSearchEndDate = true,
                shouldUseStartDateForEpg = true,
                shouldUseStartDateForMedia = true,
                groupBy = new List<GroupByDefinition>() { groupBy, new GroupByDefinition() { Key = "group_id", Value = "group_id", Type = eFieldType.Default } },
                distinctGroup = groupBy,
                topHitsCount = 1
            };

            int totalCount = 0;

            var searchPolicy = Policy.HandleResult<List<AggregationsResult>> (aggs =>
            {
                if (aggs == null) return true;
                if (aggs.Count < 1) return true;
                var firstAgg = aggs[0];
                if (firstAgg.field != groupBy.Key) return true;
                if (firstAgg.totalItems != 2) return true;
                if (firstAgg.results.Count != 2) return true;
                var firstResult = firstAgg.results[0];
                if (firstResult.value != "sunny one") return true;
                if (firstResult.count != 6) return true;
                if (firstResult.subs.Count != 1) return true;
                var firstSub = firstResult.subs[0];
                if (firstSub.field != "group_id") return true;
                if (firstSub.results.Count != 1) return true;
                if (firstSub.results[0].count != 6) return true;
                var secondResult = firstAgg.results[1];
                if (firstResult.value != "sunny two") return true;
                if (firstResult.count != 5) return true;
                if (firstResult.subs.Count != 1) return true;
                var secondSub = secondResult.subs[0];
                if (secondSub.field != "group_id") return true;
                if (secondSub.results.Count != 1) return true;
                if (secondSub.results[0].count != 5) return true;

                return false;
            }).WaitAndRetry(
                5,
                retryAttempt => TimeSpan.FromSeconds(1));

            List<AggregationsResult> aggregations = new List<AggregationsResult>();
            List<UnifiedSearchResult> searchResults = new List<UnifiedSearchResult>();
            searchPolicy.Execute(() =>
            {
                searchResults = indexManager.UnifiedSearch(definitions, ref totalCount, out aggregations);
                return aggregations;
            });

            Assert.AreEqual(11, totalCount);
            Assert.AreEqual(1, aggregations.Count);
            var firstAgg = aggregations[0];
            Assert.AreEqual(groupBy.Key, firstAgg.field);
            Assert.AreEqual(2, firstAgg.totalItems);
            Assert.AreEqual(2, firstAgg.results.Count);
            var firstResult = firstAgg.results[0];
            Assert.AreEqual("sunny one", firstResult.value);
            Assert.AreEqual(6, firstResult.count);
            Assert.AreEqual(1, firstResult.subs.Count);
            var firstSub = firstResult.subs[0];
            Assert.AreEqual("group_id", firstSub.field);
            Assert.AreEqual(0, firstSub.totalItems);
            Assert.AreEqual(1, firstSub.results.Count);
            Assert.AreEqual(6, firstSub.results[0].count);
            var secondResult = firstAgg.results[1];
            Assert.AreEqual("sunny two", secondResult.value);
            Assert.AreEqual(5, secondResult.count);
            Assert.AreEqual(1, secondResult.subs.Count);
            var secondSub = secondResult.subs[0];
            Assert.AreEqual("group_id", secondSub.field);
            Assert.AreEqual(0, secondSub.totalItems);
            Assert.AreEqual(1, secondSub.results.Count);
            Assert.AreEqual(5, secondSub.results[0].count);
        }

        //[Test]
        public void TestNestSearchMediaBuilder()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            Media randomMedia, randomMedia2;
            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            var media = SetUpMockMedia(partnerId, language, 2);
            indexManager.PublishMediaIndex(indexDate, true, true);
            randomMedia = media[0];
            randomMedia2 = media[1];
            indexManager.UpsertMedia(randomMedia.m_nMediaID);
            indexManager.UpsertMedia(randomMedia2.m_nMediaID);

            var nestMediaBuilder = new UnifiedSearchNestMediaBuilder();
            nestMediaBuilder.Definitions = new MediaSearchObj()
            { m_nGroupId = partnerId, m_nMediaID = randomMedia.m_nMediaID };

            nestMediaBuilder.QueryType = eQueryType.EXACT;
            var queryContainer = nestMediaBuilder.GetQuery();
            var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var searchResponse = elasticClient.Search<NestBaseAsset>(s =>
                {
                    s.Index(Indices.Index(nestMediaBuilder.GetIndices()))
                        .Query(q => nestMediaBuilder.GetQuery());
                    s = nestMediaBuilder.SetSizeAndFrom(s);
                    return s;
                }
            );
        }

        public void Test3Languages()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageRus = new ApiObjects.LanguageObj()
            {
                ID = language.ID + 1,
                Code = "rus",
                Name = "Russian",
                IsDefault = false,
            };
            var languageJap = new ApiObjects.LanguageObj()
            {
                ID = language.ID + 2,
                Code = "jap",
                Name = "Japanese",
                IsDefault = false,
            };
            var languageObjs = new List<LanguageObj>() { language, languageRus, languageJap }.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language, languageRus, languageJap },
                ref _mockCatalogManager);

            var indexManager = GetIndexV7Manager(partnerId);
            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(DateTime.UtcNow);
            Assert.True(result);

            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            randomMedia.m_sName = "multilingualName eng";

            var randomMediaRus = randomMedia.Clone();
            randomMediaRus.m_sName = "multilingualName rus";

            var randomMediaJap = randomMedia.Clone();
            randomMediaJap.m_sName = "multilingualName jap";

            var innerDictionary = new Dictionary<int, ApiObjects.SearchObjects.Media>() { };
            innerDictionary[language.ID] = randomMedia;
            innerDictionary[languageRus.ID] = randomMediaRus;
            innerDictionary[languageJap.ID] = randomMediaJap;

            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID))
                .Returns(innerDictionary);

            var dictionary = new Dictionary<int, Dictionary<int, Media>>();
            dictionary[randomMedia.m_nMediaID] = innerDictionary;
            indexManager.InsertMedias(dictionary, indexDate);

            indexManager.PublishMediaIndex(indexDate, true, true);
        }

        //[Test]
        public void TestSortByStartDate()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            var media2 = SetUpMockMedia(partnerId, language, 10);

            foreach (var m in media2)
            {
                m.m_dTagValues.Add("test_tag", new HashSet<string>() { "value" });
                m.m_nMediaTypeID = 50;
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media1 = SetUpMockMedia(partnerId, language, 10);

            for (int i = 0; i < media1.Count; i++)
            {
                var m = media1[i];
                m.m_dTagValues.Add("test_tag", new HashSet<string>() { media2[i].m_sName });
                m.m_nMediaTypeID = 26;
                m.m_sStartDate = DateTime.Today.AddMonths(-1).ToString(Utils.ES_DATE_FORMAT);
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media3 = SetUpMockMedia(partnerId, language, 5);

            foreach (var m in media3)
            {
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

            Assert.IsNotEmpty(indexName);

            indexManager.PublishEpgIndex(indexName, false, true, true);

            var definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                langauge = language,
                order = new OrderObj()
                {
                    m_eOrderBy = OrderBy.START_DATE,
                    m_eOrderDir = OrderDir.DESC
                },
                parentMediaTypes = new Dictionary<int, int>() { { 26, 50 } },
                preference = "123456",
                associationTags = new Dictionary<int, string>() { { 26, "test_tag" } },
            };

            var searchPolicy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            int totalCount = 0;
            var searchResults = searchPolicy.Execute(() => indexManager.UnifiedSearch(definitions, ref totalCount));

            Assert.IsNotNull(searchResults);
        }

        //[Test]
        public void TestSortByStats()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            var media1 = SetUpMockMedia(partnerId, language, 4);

            foreach (var m in media1)
            {
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            // media 0 = 6 views
            // media 1 = 3 views (+3 from two years ago)
            // media 2 = 4 views
            // media 3 = 0 views
            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-1),
                GroupID = partnerId,
                MediaID = media1[0].m_nMediaID,
                MediaType = media1[0].m_nMediaTypeID.ToString(),
                Count = 4
            });
            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = media1[0].m_nMediaID,
                MediaType = media1[0].m_nMediaTypeID.ToString(),
                Count = 2
            });

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = media1[1].m_nMediaID,
                MediaType = media1[1].m_nMediaTypeID.ToString(),
                Count = 3
            });

            // to be filtered by start date
            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddYears(-2),
                GroupID = partnerId,
                MediaID = media1[1].m_nMediaID,
                MediaType = media1[1].m_nMediaTypeID.ToString(),
                Count = 3
            });

            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-2),
                GroupID = partnerId,
                MediaID = media1[2].m_nMediaID,
                MediaType = media1[2].m_nMediaTypeID.ToString(),
                Count = 2
            });
            indexManager.InsertSocialStatisticsData(new ApiObjects.Statistics.SocialActionStatistics()
            {
                Action = "firstplay",
                Date = DateTime.UtcNow.AddHours(-3),
                GroupID = partnerId,
                MediaID = media1[2].m_nMediaID,
                MediaType = media1[2].m_nMediaTypeID.ToString(),
                Count = 2
            });

            var definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                langauge = language,
                orderByFields = new List<IEsOrderByField>
                {
                    new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, DateTime.UtcNow.AddDays(-1))
                },
                preference = "123456",
            };

            var searchPolicy = Policy.HandleResult<List<UnifiedSearchResult>>(
                x =>
                {
                    if (x == null)
                    {
                        return true;
                    }
                    if (x.Count < 4)
                    {
                        return true;
                    }
                    if (media1[0].m_nMediaID.ToString() != x[0].AssetId)
                    {
                        return true;
                    }
                    if (media1[1].m_nMediaID.ToString() != x[2].AssetId)
                    {
                        return true;
                    }
                    if (media1[2].m_nMediaID.ToString() != x[1].AssetId)
                    {
                        return true;
                    }
                    if (media1[3].m_nMediaID.ToString() != x[3].AssetId)
                    {
                        return true;
                    }
                    
                    return false;
                }).WaitAndRetry(
                10,
                retryAttempt => TimeSpan.FromSeconds(1));

            int totalCount = 0;
            var searchResults = searchPolicy.Execute(() => indexManager.UnifiedSearch(definitions, ref totalCount));

            Assert.IsNotNull(searchResults);
            // first is first
            Assert.AreEqual(media1[0].m_nMediaID.ToString(), searchResults[0].AssetId);
            // second is third
            Assert.AreEqual(media1[1].m_nMediaID.ToString(), searchResults[2].AssetId);
            // third is second
            // confusing? read the numbers in the comments above :)
            Assert.AreEqual(media1[2].m_nMediaID.ToString(), searchResults[1].AssetId);
            Assert.AreEqual(media1[3].m_nMediaID.ToString(), searchResults[3].AssetId);
        }


        //[Test]
        public void TestGroupByReorderbuckets()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            var result = indexManager.SetupMediaIndex(indexDate);

            Assert.True(result);

            indexManager.PublishMediaIndex(indexDate, true, true);

            var media1 = SetUpMockMedia(partnerId, language, 2);

            foreach (var m in media1)
            {
                m.m_dMeatsValues.Add("test_string_meta", m.m_nMediaID.ToString());
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            string lastMediaId = "0";
            string secondMediaId = "0";
            string firstMediaId = "0";

            var media2 = SetUpMockMedia(partnerId, language, 3);
            string lastResultName = "sunny last";
            
            foreach (var m in media2)
            {
                lastMediaId = lastMediaId.CompareTo(m.m_nMediaID.ToString()) > 0 ? lastMediaId : m.m_nMediaID.ToString();
                m.m_sName = lastResultName;
                m.m_dMeatsValues.Add("test_string_meta", $"A{m.m_nMediaID}");
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media3 = SetUpMockMedia(partnerId, language, 2);
            string secondResultName = "sunny two";
            
            foreach (var m in media3)
            {
                secondMediaId = secondMediaId.CompareTo(m.m_nMediaID.ToString()) > 0 ? secondMediaId : m.m_nMediaID.ToString();
                m.m_sName = secondResultName;
                m.m_dMeatsValues.Add("test_string_meta", $"B{m.m_nMediaID}");
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var media4 = SetUpMockMedia(partnerId, language, 4);
            string firstResultName = "testing STARTS with sun";

            foreach (var m in media4)
            {
                firstMediaId = firstMediaId.CompareTo(m.m_nMediaID.ToString()) > 0 ? firstMediaId : m.m_nMediaID.ToString();
                m.m_sName = firstResultName;
                m.m_dMeatsValues.Add("test_string_meta", $"C{m.m_nMediaID}");
                indexManager.UpsertMedia(m.m_nMediaID);
            }

            var indexName = indexManager.SetupEpgIndex(DateTime.UtcNow, false);

            Assert.IsNotEmpty(indexName);

            indexManager.PublishEpgIndex(indexName, false, true, true);
            var groupBy = new GroupByDefinition()
            {
                Key = "name",
                Type = eFieldType.LanguageSpecificField
            };

            var definitions = new UnifiedSearchDefinitions()
            {
                countryId = 18,
                deviceRuleId = new int[] { },
                epgDaysOffest = 7,
                groupId = partnerId,
                pageSize = 30,
                shouldSearchMedia = true,
                filterPhrase = new BooleanPhrase(new List<BooleanPhraseNode>()
                {
                    new BooleanLeaf("is_active", true, typeof(bool)),
                    new BooleanLeaf("name", "sun", typeof(string), ComparisonOperator.WordStartsWith, true, true)
                },
                eCutType.And),
                langauge = language,
                order = new OrderObj()
                {
                    m_eOrderBy = OrderBy.META,
                    m_sOrderValue = "test_string_meta",
                    m_eOrderDir = OrderDir.DESC
                },
                parentMediaTypes = new Dictionary<int, int>() { { 1, 2 } },
                preference = "123456",
                shouldAddIsActiveTerm = true,
                shouldGetDomainsRecordings = true,
                shouldSearchEpg = true,
                shouldUseCatalogStartDateForMedia = true,
                shouldUseEndDateForEpg = true,
                shouldUseSearchEndDate = true,
                shouldUseStartDateForEpg = true,
                shouldUseStartDateForMedia = true,
                groupBy = new List<GroupByDefinition>() { groupBy },
                distinctGroup = groupBy,
                topHitsCount = 1
            };

            int totalCount = 0;

            var searchPolicy = Policy.HandleResult<List<AggregationsResult>>(x =>
            {
                if (x == null) return true;
                if (x.Count != 1) return true;
                var firstAgg = x[0];
                if (firstAgg.field != "name") return true;
                if (firstAgg.totalItems != 3) return true;
                var firstResult = firstAgg.results[0];
                if (firstResult.value != firstResultName) return true;
                if (firstResult.topHits == null || firstResult.topHits.Count == 0) return true;
                if (firstResult.topHits[0].AssetId != firstMediaId.ToString()) return true;
                var secondResult = firstAgg.results[1];
                if (secondResultName != secondResult.value) return true;
                if (secondResult.topHits == null || secondResult.topHits.Count == 0) return true;
                if (secondMediaId.ToString() != secondResult.topHits[0].AssetId) return true;
                var lastResult = firstAgg.results[2];
                if (lastResultName != lastResult.value) return true;
                if (lastResult.topHits == null || lastResult.topHits.Count == 0) return true;
                if (lastMediaId.ToString() != lastResult.topHits[0].AssetId) return true;

                return false;
            })
            .WaitAndRetry(
                5,
                retryAttempt => TimeSpan.FromSeconds(1));

            List<AggregationsResult> aggregations = new List<AggregationsResult>();
            List<UnifiedSearchResult> searchResults = new List<UnifiedSearchResult>();
            searchPolicy.Execute(() =>
            {
                searchResults = indexManager.UnifiedSearch(definitions, ref totalCount, out aggregations);
                return aggregations;
            });

            Assert.IsNotNull(aggregations);
            Assert.AreEqual(1, aggregations.Count);
            var firstAgg = aggregations[0];
            Assert.AreEqual("name", firstAgg.field);
            Assert.AreEqual(3, firstAgg.totalItems);
            
            var firstResult = firstAgg.results[0];
            Assert.AreEqual(firstResultName, firstResult.value);
            Assert.IsNotNull(firstResult.topHits);
            Assert.IsNotEmpty(firstResult.topHits);
            Assert.AreEqual(firstMediaId.ToString(), firstResult.topHits[0].AssetId);

            var secondResult = firstAgg.results[1];
            Assert.AreEqual(secondResultName, secondResult.value);
            Assert.IsNotNull(secondResult.topHits);
            Assert.IsNotEmpty(secondResult.topHits);
            Assert.AreEqual(secondMediaId.ToString(), secondResult.topHits[0].AssetId);

            var lastResult = firstAgg.results[2];
            Assert.AreEqual(lastResultName, lastResult.value);
            Assert.IsNotNull(lastResult.topHits);
            Assert.IsNotEmpty(lastResult.topHits);
            Assert.AreEqual(lastMediaId.ToString(), lastResult.topHits[0].AssetId);
        }

        //[Test]
        public void TestEntitledAssets()
        {
            // arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var dictionary = new Dictionary<int, Media>() { };
            var indexManager = GetIndexV7Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);

            var medias = new Dictionary<int, Dictionary<int, Media>>();
            var mediaOne = new Dictionary<int, Media>();
            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            randomMedia.epgIdentifier = $"1{randomMedia.m_nMediaTypeID}";
            mediaOne.Add(language.ID, randomMedia);
            medias.Add(1, mediaOne);

            //act
            indexManager.InsertMedias(medias, indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);

            var definitions = new UnifiedSearchDefinitions();

            // Copy definitions from original object
            definitions.entitlementSearchDefinitions = new EntitlementSearchDefinitions()
            {
                entitledPaidForAssets = new Dictionary<eAssetTypes, List<string>>()
                {
                    { eAssetTypes.MEDIA, new List<string>(){randomMedia.m_nMediaID.ToString() } }
                }
            };
            definitions.entitlementSearchDefinitions.shouldSearchNotEntitled = false;
            definitions.deviceRuleId = new int[] { 0 };
            definitions.groupId = partnerId;
            definitions.indexGroupId = partnerId;
            definitions.geoBlockRules = new List<int>() { 0 };
            definitions.permittedWatchRules = "0";
            definitions.shouldSearchMedia = true;
            definitions.shouldSearchEpg = false;
            definitions.shouldSearchRecordings = false;
            definitions.userTypeID = 0;

            // Most important part - tell the definitions to search only entitled assets and only of linear channels
            definitions.filterPhrase = new BooleanLeaf("entitled_assets", "true", typeof(string), ComparisonOperator.Contains);
            definitions.mediaTypes = new List<int>() { randomMedia.m_nMediaTypeID };

            // Also indicate that we are interested in this field
            definitions.extraReturnFields.Add("epg_identifier");

            var policy = Policy.HandleResult<List<int>>(x => x == null || x.Count == 0)
                .WaitAndRetry(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(1));

            var entitledEpgLinearChannels = policy.Execute(() =>
            {
                return indexManager.GetEntitledEpgLinearChannels(definitions);
            });

            Assert.NotNull(entitledEpgLinearChannels);
            Assert.IsNotEmpty(entitledEpgLinearChannels);
            Assert.AreEqual(randomMedia.epgIdentifier, entitledEpgLinearChannels[0].ToString());
        }

        //[Test]
        public void TestGetCurrentProgramInfosByDate()
        {

        }
    }
}
