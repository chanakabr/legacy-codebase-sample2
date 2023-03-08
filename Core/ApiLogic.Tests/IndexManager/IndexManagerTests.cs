using ApiObjects;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using GroupsCacheManager;
using Moq;
using NUnit.Framework;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiLogic.Tests.IndexManager.helpers;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using ApiObjects.Statistics;
using CachingProvider.LayeredCache;
using Catalog.Response;
using Core.Catalog.Cache;
using EpgGroupSettings = GroupsCacheManager.EpgGroupSettings;
using KeyValuePair = System.Collections.Generic.KeyValuePair;
using Utils = ElasticSearch.Common.Utils;
using ApiLogic.Catalog;
using ApiLogic.EPG;
using ApiLogic.IndexManager;
using ApiLogic.Tests.ConfigurationMocks;
using ElasticSearch.NEST;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.QueryBuilders;
using ApiLogic.IndexManager.Mappings;
using ApiObjects.Epg;
using Core.GroupManagers;
using ApiLogic.IndexManager.Sorting;
using ElasticSearch.Utils;
using ApiObjects.Base;

namespace ApiLogic.Tests.IndexManager
{
    //[TestFixture]
    public class IndexManagerTests
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
        private Mock<IMappingTypeResolver> _mockMappingTypeResolver;
        private INamingHelper _mockNamingHelper;
        private IGroupSettingsManager _mockGroupSettingsManager;
        private Mock<IStartDateAssociationTagsSortStrategy> _mockStartDateAssociationTagsSortStrategy;
        private Mock<IStatisticsSortStrategy> _mockStatisticsSortStrategy;
        private Mock<ISortingService> _mockSortingService;
        private Mock<ISortingAdapter> _mockSortingAdapter;
        private Mock<IEsSortingService> _mockEsSortingService;
        private Mock<IUnifiedQueryBuilderInitializer> _mockQueryInitializer;
        private Mock<IRegionManager> _mockRegionManager;

        private IndexManagerV2 GetIndexV2Manager(int partnerId)
        {
            return new IndexManagerV2(partnerId,
                _esApi,
                _mockGroupManager.Object,
                _mockEsSerializerV2,
                _mockCatalogManager.Object,
                _elasticSearchIndexDefinitions,
                _mockLayeredCache.Object,
                _mockChannelManager.Object,
                _mockCatalogCache.Object,
                _mockWatchRuleManager.Object,
                _mockChannelQueryBuilder.Object,
                _mockMappingTypeResolver.Object,
                _mockNamingHelper,
                _mockGroupSettingsManager,
                _mockSortingService.Object,
                _mockStartDateAssociationTagsSortStrategy.Object,
                _mockStatisticsSortStrategy.Object,
                _mockSortingAdapter.Object,
                _mockEsSortingService.Object,
                _mockQueryInitializer.Object,
                _mockRegionManager.Object
            );
        }

        //[SetUp]
        public void SetUp()
        {
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
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(ElasticSearch.Common.Utils.Instance, ApplicationConfiguration.Current);
            _mockMappingTypeResolver = _mockRepository.Create<IMappingTypeResolver>();

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
            _mockSortingService = _mockRepository.Create<ISortingService>();
            _mockStartDateAssociationTagsSortStrategy = _mockRepository.Create<IStartDateAssociationTagsSortStrategy>();
            _mockStatisticsSortStrategy = _mockRepository.Create<IStatisticsSortStrategy>();
            _mockQueryInitializer = _mockRepository.Create<IUnifiedQueryBuilderInitializer>();
        }

        //[Test]
        public void TestTagsCrud()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV2Manager(partnerId);
            var tagsIndexName = indexManager.SetupTagsIndex(DateTime.UtcNow);
            var randomTag = IndexManagerMockDataCreator.GetRandomTag(language.ID);
            var allTagValues = new List<TagValue>()
            {
                randomTag
            };
            indexManager.InsertTagsToIndex(tagsIndexName, allTagValues);

            var publishResult = indexManager.PublishTagsIndex(tagsIndexName, true, true);

            Assert.True(publishResult);


            var searchPolicy = Policy.HandleResult<List<TagValue>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var searchDefinitions = new TagSearchDefinitions()
            {
                GroupId = partnerId,
                ExactSearchValue = randomTag.value,
                PageSize = 1,
                Language = language
            };

            var searchResult = searchPolicy.Execute(() =>
              {
                  return indexManager.SearchTags(searchDefinitions, out int number);
              });

            Assert.IsNotNull(searchResult);
            Assert.AreEqual(searchResult.Count, 1);
            Assert.AreEqual(searchResult[0].tagId, randomTag.tagId);

            //test update tag
            randomTag.value = "testing2";
            indexManager.UpdateTag(randomTag);
            searchDefinitions.ExactSearchValue = randomTag.value;

            var searchTags = searchPolicy.Execute(() =>
              {
                  return indexManager.SearchTags(searchDefinitions, out int number);
              });

            // searchTags = indexManager.SearchTags(searchDefinitions, out number);
            Assert.IsNotNull(searchTags);
            Assert.AreEqual(searchTags.Count, 1);
            Assert.AreEqual(searchTags[0].tagId, randomTag.tagId);
            Assert.AreEqual(searchTags[0].value, randomTag.value);

            var deleteTag = indexManager.DeleteTag(randomTag.tagId);
            var deletePolicy = Policy.HandleResult<List<TagValue>>(x => x != null && x.Count > 0).WaitAndRetry(3,
                retryAttempt => TimeSpan.FromSeconds(1));
            searchDefinitions.ExactSearchValue = randomTag.value;

            searchTags = deletePolicy.Execute(() =>
              {
                  return indexManager.SearchTags(searchDefinitions, out int number);
              });

            Assert.IsEmpty(searchTags);

            //test delete tag by topic
            //add tags and remove by topic right after
            indexManager.InsertTagsToIndex(tagsIndexName, allTagValues);
            searchDefinitions.ExactSearchValue = randomTag.value;
            searchTags = searchPolicy.Execute(() =>
              {
                  return indexManager.SearchTags(searchDefinitions, out int number);
              });
            Assert.IsNotEmpty(searchTags);

            var deleteTagsByTopic = indexManager.DeleteTagsByTopic(randomTag.topicId);
            searchTags = deletePolicy.Execute(() =>
            {
                return indexManager.SearchTags(searchDefinitions, out int number);
            });
            Assert.IsEmpty(searchTags);
        }

        //[Test]
        public void TestMediaIndexCrud()
        {
            //arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var dictionary = new Dictionary<int, Media>() { };
            var indexManager = GetIndexV2Manager(partnerId);

            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            var medias = new Dictionary<int, Dictionary<int, Media>>();
            var mediaOne = new Dictionary<int, Media>();
            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            mediaOne.Add(language.ID, randomMedia);
            medias.Add(randomMedia.m_nMediaID, mediaOne);

            var randomMedia2 = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var mediaOne2 = new Dictionary<int, Media>();
            mediaOne2.Add(language.ID, randomMedia2);
            medias.Add(randomMedia2.m_nMediaID, mediaOne2);

            //act
            indexManager.InsertMedias(medias, indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);

            var totalItems = 0;
            var notInUse = 0;
            var searchResult = new List<UnifiedSearchResult>();

            var policy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var specificAssets = new Dictionary<eAssetTypes, List<string>>()
            {
                {
                    eAssetTypes.MEDIA, new List<string>(){randomMedia.m_nMediaID.ToString()}
                }
            };
            var unifiedSearchDefinitions =
                new UnifiedSearchDefinitions()
                    .WithPageIndex(0)
                    .WithGroupId(partnerId)
                    .WithLanguage(language)
                    .WithPageSize(10)
                    .ShouldSearchMedia()
                    .WithSpecificAssets(specificAssets);

            searchResult = policy.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions,
                    ref totalItems
                    );
            });


            //assert
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(randomMedia.m_nMediaID.ToString(), searchResult[0].AssetId);

            var newMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            newMedia.m_sName = "upsert_test";
            dictionary[language.ID] = newMedia;
            _mockCatalogManager
                .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID))
                .Returns(dictionary);

            var upsertMedia = indexManager.UpsertMedia(randomMedia.m_nMediaID);
            Assert.True(upsertMedia);

            var mediaSearchObj = new MediaSearchObj()
            {
                m_sName = "upsert_test",
                m_nPageSize = 1,
                m_nPageIndex = 0,
                m_dAnd = new List<SearchValue>() { new SearchValue("name", "upsert_test")
                    {
                        m_lValue = new List<string>() { "upsert_test" }
                    }
                },
                m_nGroupId = partnerId,
                m_bExact = true,
                m_sPermittedWatchRules = "0",
                m_nIndexGroupId = partnerId
            };

            var searchMediaPolicy = Policy.HandleResult<SearchResultsObj>(x => x == null || x.m_resultIDs == null).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var searchResultsObj = searchMediaPolicy.Execute(() =>
            {
                return indexManager.SearchMedias(mediaSearchObj, language.ID, false);
            });

            Assert.IsNotNull(searchResultsObj);
            Assert.IsNotNull(searchResultsObj.m_resultIDs);
            Assert.AreEqual(1, searchResultsObj.m_resultIDs.Count);
            Assert.AreEqual(newMedia.m_nMediaID, searchResultsObj.m_resultIDs[0].assetID);

            // negative test
            var negativeSearchResult = indexManager.SearchMedias(new MediaSearchObj()
            {
                m_sName = "invalid_name",
                m_nPageSize = 1,
                m_nPageIndex = 0,
                m_dAnd = new List<SearchValue>() { new SearchValue("name", "invalid_name")
                    {
                        m_lValue = new List<string>() { "invalid_name" }
                    }
                },
                m_nGroupId = partnerId,
                m_bExact = true,
                m_sPermittedWatchRules = "0",
                m_nIndexGroupId = partnerId
            },
            language.ID, false);

            Assert.IsNotNull(negativeSearchResult);
            Assert.AreEqual(0, negativeSearchResult.n_TotalItems);

            //check update date
            var assetsUpdateDate = indexManager.GetAssetsUpdateDate(eObjectType.Media, new List<int>() { randomMedia.m_nMediaID });
            Assert.AreEqual(randomMedia.m_sUpdateDate,
                assetsUpdateDate[0].UpdateDate.ToString(Utils.ES_DATE_FORMAT));

            var assetsUpdateDates = indexManager.GetAssetsUpdateDates(new List<UnifiedSearchResult>()
            {
                new UnifiedSearchResult()
                {
                    AssetId = randomMedia.m_nMediaID.ToString(),
                    AssetType = eAssetTypes.MEDIA
                }
            },
            ref totalItems,
            1, 0, true);

            Assert.AreEqual(randomMedia.m_sUpdateDate,
                assetsUpdateDates[0].m_dUpdateDate.ToString(Utils.ES_DATE_FORMAT));

            //check deletion
            var deleteMedia = indexManager.DeleteMedia(randomMedia.m_nMediaID);
            var policyDeletion = Policy.HandleResult<List<UnifiedSearchResult>>(x => x != null || x.Count > 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            searchResult = policyDeletion.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions,
                    ref totalItems
                    );
            });

            Assert.IsEmpty(searchResult);
        }

        //[Test]
        public void TestIp2Country()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var indexManager = GetIndexV2Manager(randomPartnerId);
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

            bool insertResult = indexManager.InsertDataToIPToCountryIndex(indexName, ipv4, null);
            Assert.IsTrue(insertResult);

            bool publishResult = indexManager.PublishIPToCountryIndex(indexName);
            Assert.IsTrue(publishResult);

            bool searchSuccess = false;
            var policy = Policy.HandleResult<Country>(x => x == null).WaitAndRetry(
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

            country = indexManager.GetCountryByCountryName(usa);
            Assert.IsNotNull(country);
            Assert.AreEqual(country.Id, usaId);

            // no country
            country = indexManager.GetCountryByIp("1.2.3.4", out searchSuccess);

            Assert.IsTrue(searchSuccess);
            Assert.IsNull(country);
        }

        //[Test]
        public void TestEpgV2Crud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageObjs = new List<ApiObjects.LanguageObj>() { language }.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language }, ref _mockCatalogManager);

            _mockMappingTypeResolver.Setup(x => x.GetMappingType(false, language)).Returns("epg_en");
            var indexManager = GetIndexV2Manager(randomPartnerId);
            var policy = Policy.Handle<Exception>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));
            var epgId = 1 + new Random().Next(1000);

            //act
            var indexName = _mockNamingHelper.GetDailyEpgIndexName(randomPartnerId, DateTime.Now);
            var setupEpgV2Index = indexManager.SetupEpgV2Index(indexName);

            //assert
            Assert.IsNotEmpty(setupEpgV2Index);

            var dateOfProgramsToIngest = DateTime.Now.AddDays(-1);
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            var epgCbObjects = new List<EpgCB>();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            epgCbObjects.Add(new EpgCB() { Language = language.Code, StartDate = dateOfProgramsToIngest, ChannelID = randomChannel.m_nChannelID });

            var epgItem = new EpgProgramBulkUploadObject()
            {
                GroupId = randomPartnerId,
                StartDate = dateOfProgramsToIngest,
                EpgId = (ulong)epgId,
                EpgCbObjects = epgCbObjects,
                EpgExternalId = $"1{epgId}"
            };
            crudOperations.ItemsToAdd.Add(epgItem);


            indexManager.DeletePrograms(crudOperations.ItemsToDelete, setupEpgV2Index, languageObjs);

            var programsToIndex = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
                .ToList();

            indexManager.UpsertPrograms(programsToIndex, setupEpgV2Index, language, languageObjs);

            List<string> epgCbDocumentIdsByEpgId = indexManager.GetEpgCBDocumentIdsByEpgId(new long[] { epgId }, languageObjs.Values);
            Assert.AreEqual(epgCbDocumentIdsByEpgId.First(), epgId.ToString(), "Expected document id and epg id to be the same");

            //check deletion
            crudOperations.ItemsToDelete.Add(epgItem);
            indexManager.DeletePrograms(crudOperations.ItemsToDelete, setupEpgV2Index, languageObjs);

            var deletePolicy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x != null && x.Count > 0).WaitAndRetry(3,
                retryAttempt => TimeSpan.FromSeconds(1));
            var totalItems = 0;
            var notInUse = 0;
            var searchResult = new List<UnifiedSearchResult>();
            var specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>()
            {
                {
                    ApiObjects.eAssetTypes.EPG, new List<string>(){ epgId.ToString() }
                }
            };
            var unifiedSearchDefinitions =
                new UnifiedSearchDefinitions()
                .WithPageIndex(0)
                .WithGroupId(randomPartnerId)
                .WithLanguage(language)
                .WithPageSize(10)
                .ShouldSearchEpg()
                .WithSpecificAssets(specificAssets);
            unifiedSearchDefinitions.epgDaysOffest = 7;

            var notResultEpgList = deletePolicy.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions, ref totalItems);
            });

            Assert.AreEqual(0, totalItems);
        }

        //[Test]
        public void TestEpgV1Crud()
        {
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            var languageObjs = new List<ApiObjects.LanguageObj>() { language }.ToDictionary(x => x.Code);
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            _mockMappingTypeResolver.Setup(x => x.GetMappingType(false, language)).Returns("epg_en");

            var indexManager = GetIndexV2Manager(partnerId);
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

            };

            _mockCatalogCache.Setup(x => x.GetLinearChannelSettings(It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(new Dictionary<string, ApiObjects.Catalog.LinearChannelSettings>());
            indexManager.AddEPGsToIndex(indexName, false, epgs, new Dictionary<long, List<int>>(), null);
            indexManager.PublishEpgIndex(indexName, false, true, true);

            var totalItems = 0;
            var searchResult = new List<UnifiedSearchResult>();

            var searchPolicy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>()
            {
                {
                    ApiObjects.eAssetTypes.EPG, new List<string>(){ epgId.ToString() }
                }
            };

            var unifiedSearchDefinitions =
                new UnifiedSearchDefinitions()
                .WithPageIndex(0)
                .WithGroupId(partnerId)
                .WithLanguage(language)
                .WithPageSize(10)
                .ShouldSearchEpg()
                .WithSpecificAssets(specificAssets);
            unifiedSearchDefinitions.epgDaysOffest = 7;

            searchResult = searchPolicy.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions,
                    ref totalItems
                    );
            });

            //assert
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(epgId.ToString(), searchResult[0].AssetId);

            var epgProgramBulkUploadObjects = indexManager.GetCurrentProgramsByDate(randomChannel.m_nChannelID, DateTime.Now.AddDays(-2),
                DateTime.Now.AddDays(1));
            Assert.IsNotEmpty(epgProgramBulkUploadObjects);
            Assert.AreEqual(epgId, epgProgramBulkUploadObjects[0].EpgId);
        }

        //[Test]
        public void TestChannelsCrud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language }, ref _mockCatalogManager);
            _mockMappingTypeResolver.Setup(x => x.GetMappingType(false, language)).Returns("epg_en");

            var indexManager = GetIndexV2Manager(randomPartnerId);

            var channelIndexName = indexManager.SetupChannelMetadataIndex(DateTime.UtcNow);
            indexManager.AddChannelsMetadataToIndex(channelIndexName, new List<Channel>() { randomChannel });
            indexManager.PublishChannelsMetadataIndex(channelIndexName, true, true);

            var response = new GenericResponse<Channel>();
            response.Object = randomChannel;
            response.SetStatus(eResponseStatus.OK);

            _mockChannelManager
                .Setup(x => x.GetChannelById(It.IsAny<ContextData>(), It.IsAny<int>(), It.IsAny<bool>()))
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
        public void TestSocialCrud()
        {
            var randomPartnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var randomChannel = IndexManagerMockDataCreator.GetRandomChannel(randomPartnerId);
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(randomPartnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV2Manager(randomPartnerId);
            var stat1 = IndexManagerMockDataCreator.GetRandomSocialActionStat(randomPartnerId);

            //act
            //test insert
            var insertSocialStatisticsData = indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(insertSocialStatisticsData);

            insertSocialStatisticsData = indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(insertSocialStatisticsData);


            var assetIDsToStatsMapping = new Dictionary<int, AssetStatsResult>();
            assetIDsToStatsMapping[stat1.MediaID] = new AssetStatsResult();
            var endDate = DateTime.Now.AddDays(1);
            var startDate = DateTime.Now.AddDays(-2);
            var assetIDs = new List<int>() { stat1.MediaID };
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

            //test delete
            var socialSearch = new StatisticsActionSearchObj()
            {
                Action = stat1.Action,
                GroupID = randomPartnerId,
                MediaID = stat1.MediaID,
                MediaType = stat1.MediaType,
                Date = stat1.Date
            };

            var deleteSocialAction = indexManager.DeleteSocialAction(socialSearch);
            Assert.True(deleteSocialAction);


            assetIDsToStatsMapping = new Dictionary<int, AssetStatsResult>();
            assetIDsToStatsMapping[stat1.MediaID] = new AssetStatsResult();
            endDate = DateTime.Now.AddDays(1);
            startDate = DateTime.Now.AddDays(-2);
            assetIDs = new List<int>() { stat1.MediaID };
            statsType = StatsType.MEDIA;

            var deletePolicy = Policy.HandleResult<Dictionary<int, AssetStatsResult>>(x =>
                x.Count > 0 && x.ContainsKey(stat1.MediaID) && x[stat1.MediaID].m_nLikes > 0
            ).WaitAndRetry(
                10,
                retryAttempt => TimeSpan.FromSeconds(1));

            res = deletePolicy.Execute(() =>
              {
                  assetIDsToStatsMapping = new Dictionary<int, AssetStatsResult>();
                  indexManager.GetAssetStats(assetIDs, startDate, endDate, statsType, ref assetIDsToStatsMapping);
                  return assetIDsToStatsMapping;
              });

            indexManager.GetAssetStats(assetIDs, startDate, endDate, statsType, ref assetIDsToStatsMapping);

            Assert.AreEqual(0, res.Count, "Expected to have 0 like but wasn't");
        }

        //[Test]
        public void TestSubscriptionMedias()
        {
            //arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();

            var group = new Group(partnerId,
                new List<int>() { partnerId },
                new EpgGroupSettings()
                {
                    GroupId = partnerId,
                });
            group.AddLanguage(language);
            _mockGroupManager.Setup(x => x.GetGroup(It.IsAny<int>())).Returns(
                group);



            var indexManager = GetIndexV2Manager(partnerId);
            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            var medias = new Dictionary<int, Dictionary<int, Media>>();
            var mediaOne = new Dictionary<int, Media>();
            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var dictionary = new Dictionary<int, Media>() { };
            mediaOne.Add(language.ID, randomMedia);
            medias.Add(1, mediaOne);
            var newMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            dictionary[language.ID] = newMedia;

            //act
            indexManager.InsertMedias(medias, indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);



            var totalItems = 0;
            var notInUse = 0;


            var policy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>()
            {
                {
                    ApiObjects.eAssetTypes.MEDIA, new List<string>(){randomMedia.m_nMediaID.ToString()}
                }
            };
            var unifiedSearchDefinitions =
                new UnifiedSearchDefinitions()
                    .WithPageIndex(0)
                    .WithGroupId(partnerId)
                    .WithLanguage(language)
                    .WithPageSize(10)
                    .ShouldSearchMedia()
                    .WithSpecificAssets(specificAssets);

            var searchResult = policy.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions,
                    ref totalItems
                    );
            });

            //assert
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(randomMedia.m_nMediaID.ToString(), searchResult[0].AssetId);

            //check SearchSubscriptionMedias
            var mediaSearchObj = new MediaSearchObj()
            {
                m_sName = randomMedia.m_sName,
                m_nPageSize = 1,
                m_nPageIndex = 0,
                m_dAnd = new List<SearchValue>() { new SearchValue("name", randomMedia.m_sName)
                    {
                        m_lValue = new List<string>() {randomMedia.m_sName }
                    }
                },
                m_nGroupId = partnerId,
                m_bExact = true,
                m_sPermittedWatchRules = "0",
                m_nIndexGroupId = partnerId
            };

            var baseSearchObjects = new List<MediaSearchObj>() { mediaSearchObj };


            var oOrderObj = new OrderObj()
            {
                m_eOrderBy = OrderBy.ID,
                m_eOrderDir = OrderDir.ASC
            };
            var searchSubscriptionMedias =
                indexManager.SearchSubscriptionMedias(baseSearchObjects,
                    language.ID,
                    true,
                    randomMedia.m_nMediaTypeID.ToString(),
                    oOrderObj,
                    0,
                    1000);

            Assert.IsNotNull(searchSubscriptionMedias);

            Assert.IsNotEmpty(searchSubscriptionMedias.m_resultIDs);
            Assert.AreEqual(randomMedia.m_nMediaID, searchSubscriptionMedias.m_resultIDs.First().assetID);
        }

        //[Test]
        public void TestSubscriptionAssets()
        {
            //arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();

            var group = new Group(partnerId,
                new List<int>() { partnerId },
                new EpgGroupSettings()
                {
                    GroupId = partnerId,
                });
            group.AddLanguage(language);
            _mockGroupManager.Setup(x => x.GetGroup(It.IsAny<int>())).Returns(
                group);


            var indexManager = GetIndexV2Manager(partnerId);
            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            var medias = new Dictionary<int, Dictionary<int, Media>>();
            var mediaOne = new Dictionary<int, Media>();
            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var dictionary = new Dictionary<int, Media>() { };
            mediaOne.Add(language.ID, randomMedia);
            medias.Add(1, mediaOne);
            var newMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            dictionary[language.ID] = newMedia;

            //act
            indexManager.InsertMedias(medias, indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);



            var totalItems = 0;
            var notInUse = 0;


            var policy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
                3,
                retryAttempt => TimeSpan.FromSeconds(1));

            var specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>()
            {
                {
                    eAssetTypes.MEDIA, new List<string>(){randomMedia.m_nMediaID.ToString()}
                }
            };
            var unifiedSearchDefinitions =
                new UnifiedSearchDefinitions()
                    .WithPageIndex(0)
                    .WithGroupId(partnerId)
                    .WithLanguage(language)
                    .WithPageSize(10)
                    .ShouldSearchMedia()
                    .WithSpecificAssets(specificAssets);

            var searchResult = policy.Execute(() =>
            {
                return indexManager.UnifiedSearch(unifiedSearchDefinitions,
                    ref totalItems);
            });

            //assert
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(randomMedia.m_nMediaID.ToString(), searchResult[0].AssetId);

            //check SearchSubscriptionMedias
            var oOrderObj = new OrderObj()
            {
                m_eOrderBy = OrderBy.ID,
                m_eOrderDir = OrderDir.ASC
            };

            var searchObjects = new List<BaseSearchObject>() { new UnifiedSearchDefinitions() { langauge = language } };
            var assets =
                indexManager.SearchSubscriptionAssets(searchObjects,
                language.ID,
                true,
                randomMedia.m_nMediaTypeID.ToString(),
                oOrderObj,
                0,
                1000, ref totalItems);

            Assert.IsNotNull(assets);
            Assert.AreEqual(randomMedia.m_nMediaID.ToString(), assets[0].AssetId);

        }

        //[Test]
        public void TestEntitledAssets()
        {
            // arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var dictionary = new Dictionary<int, Media>() { };
            var indexManager = GetIndexV2Manager(partnerId);

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
        public void TestUnifiedSearchForGroupBy()
        {
            //arrange
            int partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            var language = IndexManagerMockDataCreator.GetEnglishLanguageWithRandomId();
            IndexManagerMockDataCreator.SetupOpcPartnerMocks(partnerId, new[] { language }, ref _mockCatalogManager);
            var indexManager = GetIndexV2Manager(partnerId);
            var policyGroupBy = Policy.HandleResult<AggregationsResult>(x => x == null || x.results.Count == 0)
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1));

            var indexDate = DateTime.UtcNow;
            indexManager.SetupMediaIndex(indexDate);
            var medias = new Dictionary<int, Dictionary<int, Media>>();
            var media1 = new Dictionary<int, Media>();
            var randomMedia = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            media1.Add(language.ID, randomMedia);
            medias.Add(randomMedia.m_nMediaID, media1);

            var randomMedia2 = IndexManagerMockDataCreator.GetRandomMedia(partnerId);
            var media2 = new Dictionary<int, Media>();
            media2.Add(language.ID, randomMedia2);
            medias.Add(randomMedia2.m_nMediaID, media2);

            //act
            indexManager.InsertMedias(medias, indexDate);
            indexManager.PublishMediaIndex(indexDate, true, true);

            var orderObj = new OrderObj()
            {
                m_eOrderBy = OrderBy.ID,
                m_eOrderDir = OrderDir.ASC
            };

            var specificAssets = new Dictionary<eAssetTypes, List<string>>()
            {
                {
                    eAssetTypes.MEDIA,
                    new List<string>() {randomMedia.m_nMediaID.ToString(), randomMedia2.m_nMediaID.ToString()}
                }
            };
            var groupBy = new GroupByDefinition() { Key = "name", Type = eFieldType.LanguageSpecificField };

            var definitions =
                new UnifiedSearchDefinitions()
                    .WithPageIndex(0)
                    .WithGroupId(partnerId)
                    .WithLanguage(language)
                    .WithPageSize(10)
                    .ShouldSearchMedia()
                    .WithSpecificAssets(specificAssets)
                    .WithGroupByOrder(AggregationOrder.Count_Asc)
                    .WithOrder(orderObj)
                    .WithDistinctGroup(groupBy);

            definitions.groupBy = new List<GroupByDefinition>() { groupBy };

            var result = policyGroupBy.Execute(() => { return indexManager.UnifiedSearchForGroupBy(definitions); });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.totalItems);
            // verify that it contains all the medias in the result
            Assert.AreEqual(1, result.results.Where(x => x.value == randomMedia.m_sName).Count());
            Assert.AreEqual(1, result.results.Where(x => x.value == randomMedia2.m_sName).Count());
            Assert.AreEqual(0, result.results.Where(x => x.value == "NAME NOT EXISTS").Count());
        }
        
    }
}
