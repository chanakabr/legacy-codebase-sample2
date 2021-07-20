// using ApiObjects;
// using ApiObjects.SearchObjects;
// using ConfigurationManager;
// using Core.Catalog;
// using Core.Catalog.CatalogManagement;
// using Core.Catalog.Response;
// using ElasticSearch.Common;
// using ElasticSearch.Common.DeleteResults;
// using GroupsCacheManager;
// using Moq;
// using NUnit.Framework;
// using Polly;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Threading;
// using ApiLogic.Tests.IndexManager.helpers;
// using ApiObjects.BulkUpload;
// using CachingProvider.LayeredCache;
// using Core.Catalog.Cache;
// using IngestHandler.Common;
// using Microsoft.VisualBasic;
// using Polly.Retry;
//
// namespace ApiLogic.Tests.IndexManager
// {
//     [TestFixture]
//     public class IndexManagerTests
//     {
//         private MockRepository _mockRepository;
//         private Mock<IGroupManager> _mockGroupManager;
//         private Mock<ICatalogManager> _mockCatalogManager;
//         private Mock<IChannelManager> _mockChannelManager;
//         private ESSerializerV2 _mockEsSerializerV2;
//         private ElasticSearchApi _esApi;
//         private ElasticSearchIndexDefinitions _elasticSearchIndexDefinitions;
//         private Mock<ILayeredCache> _mockLayeredCache;
//         private Mock<ICatalogCache> _mockCatalogCache;
//         
//         private static int _nextPartnerId = 10000;
//         private Random _random;
//
//         #region Helpers
//         
//         private IndexManagerV2 GetIndexV2Manager(int partnerId)
//         {
//             return new IndexManagerV2(partnerId,
//                 _esApi,
//                 _mockGroupManager.Object,
//                 _mockEsSerializerV2,
//                 _mockCatalogManager.Object,
//                 _elasticSearchIndexDefinitions,
//                 _mockLayeredCache.Object, 
//                 _mockChannelManager.Object,
//                 _mockCatalogCache.Object
//                 );
//         }
//
//         private void SetupOpcPartnerMocks(int partnerId,IEnumerable<LanguageObj> languages )
//         {
//             _mockCatalogManager.Setup(x => x.DoesGroupUsesTemplates(partnerId)).Returns(true);
//
//             var catalogGroupCache = new CatalogGroupCache()
//             {
//                 LanguageMapByCode = languages.ToDictionary(x => x.Code),
//                 LanguageMapById = languages.ToDictionary(x => x.ID)
//             };
//
//             _mockCatalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache)).Returns(true);
//         }
//
//         private LanguageObj GetRandomLanguage()
//         {
//             var language = new ApiObjects.LanguageObj()
//             {
//                 ID = _random.Next(1000)+1,
//                 Code = "en",
//                 Name = "english",
//                 IsDefault = true
//             };
//             return language;
//         }
//
//         private int GetRandomPartnerId()
//         {
//             var partnerId = _nextPartnerId+++_random.Next(1000);
//             return partnerId;
//         }
//
//         private  Media GetRandomMedia(int partnerId)
//         {
//             var rand = _random.Next(1000);
//             return new Media()
//             {
//                 m_nIsActive = 1,
//                 m_nMediaID = 12345+rand,
//                 m_sName = $"test media {rand}",
//                 m_sDescription = "test description",
//                 m_nGroupID = partnerId,
//                 m_sStartDate = DateTime.Today.AddDays(-1).ToString(IndexManagerV2.DATE_FORMAT)
//             };
//         }
//         
//         private TagValue GetRandomTag(int languageId)
//         {
//             return new TagValue()
//             {
//                 tagId = _random.Next(1000)+1,
//                 value = "test",
//                 topicId = 2,
//                 languageId = languageId,
//                 createDate = 1000,
//                 updateDate = 1000
//             };
//         }
//         #endregion
//
//         [SetUp]
//         public void SetUp()
//         {
//              _random = new Random();
//              //this is done in order to load tcm config from local path
//              var currentDomainBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets");
//             System.Environment.SetEnvironmentVariable("TCM_FROM_LOCAL", "true");
//             System.Environment.SetEnvironmentVariable("LOCAL_PATH", currentDomainBaseDirectory);
//             ApplicationConfiguration.Init();
//             
//             _mockEsSerializerV2 = new ESSerializerV2();
//             _mockRepository = new MockRepository(MockBehavior.Loose);
//             _esApi = new ElasticSearchApi(ApplicationConfiguration.Current);
//             _mockGroupManager = _mockRepository.Create<IGroupManager>();
//             _mockCatalogManager = _mockRepository.Create<ICatalogManager>();
//             _mockChannelManager = _mockRepository.Create<IChannelManager>();
//             _mockCatalogCache = _mockRepository.Create<ICatalogCache>();
//             _mockLayeredCache = _mockRepository.Create<ILayeredCache>();
//             _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(ElasticSearch.Common.Utils.Instance);
//             
//         }
//         
//         [Test]
//         public void TestTagsCrud()
//         {
//             var partnerId = GetRandomPartnerId();
//             var language = GetRandomLanguage();
//             SetupOpcPartnerMocks(partnerId,new []{language});
//             var indexManager = GetIndexV2Manager(partnerId);
//             var tagsIndexName = indexManager.SetupTagsIndex();
//             var randomTag = GetRandomTag(language.ID);
//             indexManager.AddTagsToIndex(tagsIndexName, new List<TagValue>()
//             {
//                 randomTag
//             });
//
//             var publishResult = indexManager.PublishTagsIndex(tagsIndexName, true, true);
//
//             Assert.True(publishResult);
//
//             
//             var policy = Policy.HandleResult<List<TagValue>>(x => x == null || x.Count==0).WaitAndRetry(
//                 3,
//                 retryAttempt => TimeSpan.FromSeconds(1));
//
//             var searchResult=policy.Execute(() =>
//             {
//                return indexManager.SearchTags(new TagSearchDefinitions()
//                 {
//                     GroupId = partnerId,
//                     ExactSearchValue = randomTag.value,
//                     PageSize = 1
//                     //, Language = language
//                 }, out int number);
//             });
//
//             var searchTags = indexManager.SearchTags(new TagSearchDefinitions()
//             {
//                 GroupId = partnerId,
//                 ExactSearchValue = randomTag.value,
//                 PageSize = 1
//                 //, Language = language
//             }, out int number);
//             Assert.IsNotNull(searchResult);
//             Assert.AreEqual(searchResult.Count, 1);
//             Assert.AreEqual(searchResult[0].tagId, randomTag.tagId);
//             
//             
//             
//             //todo gil : check with sunny its not deleting 
//             // indexManager.DeleteTag(randomTag.tagId);
//             // var deletePolicy = Policy.HandleResult<List<TagValue>>(x => x != null).WaitAndRetry(3,
//             //     retryAttempt => TimeSpan.FromSeconds(1));
//             //
//             // searchResult = deletePolicy.Execute(() =>
//             // {
//             //     return indexManager.SearchTags(new TagSearchDefinitions()
//             //     {
//             //         GroupId = partnerId,
//             //         ExactSearchValue = randomTag.value,
//             //         PageSize = 1,
//             //         Language = language
//             //     }, out int number);
//             // });
//             //
//             // Assert.IsNull(searchResult);
//         }
//         
//         [Test]
//         public void TestMediaIndexCrud()
//         {
//             //arrange
//             int partnerId = GetRandomPartnerId();
//             var language = GetRandomLanguage();
//             SetupOpcPartnerMocks(partnerId,new []{language});
//             var dictionary = new Dictionary<int, Media>(){};
//             var indexManager = GetIndexV2Manager(partnerId);
//             
//             var mediaIndexName = indexManager.SetupMediaIndex(new List<ApiObjects.LanguageObj>() {language}, language);
//             var medias = new Dictionary<int, Dictionary<int, Media>>();
//             var mediaOne = new Dictionary<int, Media>();
//             var randomMedia = GetRandomMedia(partnerId);
//             mediaOne.Add(language.ID, randomMedia);
//             medias.Add(1, mediaOne);
//             
//             //act
//             indexManager.InsertMedias(medias, mediaIndexName);
//             indexManager.PublishMediaIndex(mediaIndexName, true, true);
//
//             var totalItems = 0;
//             var notInUse = 0;
//             var searchResult = new List<UnifiedSearchResult>();
//
//             var policy = Policy.HandleResult<List<UnifiedSearchResult>>(x => x == null || x.Count == 0).WaitAndRetry(
//                 3,
//                 retryAttempt => TimeSpan.FromSeconds(1));
//
//             var specificAssets = new Dictionary<ApiObjects.eAssetTypes, List<string>>()
//             {
//                 {
//                     ApiObjects.eAssetTypes.MEDIA, new List<string>(){randomMedia.m_nMediaID.ToString()}
//                 }
//             };
//             var unifiedSearchDefinitions = 
//                 new UnifiedSearchDefinitions()
//                 .WithPageIndex(0)
//                 .WithGroupId(partnerId)
//                 .WithLanguage(language)
//                 .WithPageSize(10)
//                 .ShouldSearchMedia()
//                 .WithSpecificAssets(specificAssets);
//             
//             searchResult = policy.Execute(() =>
//             {
//                 return indexManager.UnifiedSearch(unifiedSearchDefinitions,
//                     ref totalItems,
//                     ref notInUse);
//             });            
//             
//             //assert
//             Assert.IsNotNull(searchResult);
//             Assert.AreEqual(1,searchResult.Count);
//             Assert.AreEqual(randomMedia.m_nMediaID.ToString(), searchResult[0].AssetId);
//             
//             dictionary[language.ID] = new Media() {m_sName = "test_name"};
//             _mockCatalogManager
//                 .Setup(x => x.GetGroupMedia(It.IsAny<int>(), randomMedia.m_nMediaID, It.IsAny<CatalogGroupCache>()))
//                 .Returns(dictionary);
//                 
//             //todo gil,check with sunny about query to see that media updated
//             var upsertMedia = indexManager.UpsertMedia(randomMedia.m_nMediaID);
//             Assert.True(upsertMedia);
//
//             var searchResultsObj = indexManager.SearchMedias(new MediaSearchObj() {m_sName = "test_name"}, language.ID, false);
//             Assert.IsNotNull(searchResultsObj);
//
//
//             //check update date
//             var assetsUpdateDate = indexManager.GetAssetsUpdateDate(eObjectType.Media, new List<int>() {randomMedia.m_nMediaID});
//             Assert.AreEqual(searchResultsObj.m_resultIDs[0].UpdateDate.ToString(), assetsUpdateDate[0].UpdateDate.ToString());
//             
//             //check deletion
//             var deleteMedia = indexManager.DeleteMedia(randomMedia.m_nMediaID);
//             var policyDeletion = Policy.HandleResult<List<UnifiedSearchResult>>(x => x != null || x.Count > 0).WaitAndRetry(
//                 3,
//                 retryAttempt => TimeSpan.FromSeconds(1));
//             
//             searchResult = policyDeletion.Execute(() =>
//             {
//                 return indexManager.UnifiedSearch(unifiedSearchDefinitions,
//                     ref totalItems,
//                     ref notInUse); 
//             });
//             
//             Assert.IsEmpty(searchResult);
//         }
//         
//         [Test]
//         public void TestIp2Country()
//         {
//             var randomPartnerId = GetRandomPartnerId();
//             var indexManager = GetIndexV2Manager(randomPartnerId);
//             string indexName = indexManager.SetupIPToCountryIndex();
//             string israel = "Israel";
//             string usa = "USA";
//             int usaId = 321;
//
//             List<IPV4> ipv4 = new List<IPV4>()
//             {
//                 new IPV4("1", 123, "il", israel, 26, 50),
//                 new IPV4("2", 123, "il", israel, 260, 500),
//                 new IPV4("3", usaId, "us", usa, 620, 625),
//             };
//
//             bool insertResult = indexManager.InsertDataToIPToCountryIndex(indexName, ipv4, null);
//             Assert.IsTrue(insertResult);
//
//             bool publishResult = indexManager.PublishIPToCountryIndex(indexName);
//             Assert.IsTrue(publishResult);
//
//             bool searchSuccess = false;
//             var policy = Policy.HandleResult<Country>(x => x == null).WaitAndRetry(
//                 3,
//                 retryAttempt => TimeSpan.FromSeconds(1));
//
//             // israel
//             var country = policy.Execute(() => indexManager.GetCountryByIp("0.0.0.40", out searchSuccess));
//             Assert.IsTrue(searchSuccess);
//             Assert.IsNotNull(country);
//             Assert.AreEqual(israel, country.Name);
//
//             // still israel
//             country = indexManager.GetCountryByIp("0.0.1.40", out searchSuccess);
//
//             Assert.IsTrue(searchSuccess);
//             Assert.IsNotNull(country);
//             Assert.AreEqual(israel, country.Name);
//
//             country = indexManager.GetCountryByCountryName(usa);
//             Assert.IsNotNull(country);
//             Assert.AreEqual(country.Id, usaId);
//
//             // no country
//             country = indexManager.GetCountryByIp("1.2.3.4", out searchSuccess);
//
//             Assert.IsTrue(searchSuccess);
//             Assert.IsNull(country);
//         }
//         
//         [Test]
//         public void TestEpgCrud()
//         {
//             var randomPartnerId = GetRandomPartnerId();
//             var indexManager = GetIndexV2Manager(randomPartnerId);
//             var language = GetRandomLanguage();
//             SetupOpcPartnerMocks(randomPartnerId,new []{language});
//             var languageObjs = new List<ApiObjects.LanguageObj>(){language}.ToDictionary(x=>x.Code);
//             var policy = Policy.Handle<Exception>().WaitAndRetry(3,retryAttempt => TimeSpan.FromSeconds(1));
//             var epgId = 1+_random.Next(1000);
//             
//             //act
//             var setupEpgV2Index = indexManager.SetupEpgV2Index(DateTime.Now, languageObjs, language, policy);
//             
//             //assert
//             Assert.IsNotEmpty(setupEpgV2Index);
//
//             var dateOfProgramsToIngest = DateTime.Now.AddDays(-1);
//             var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
//             var epgCbObjects = new List<EpgCB>();
//             epgCbObjects.Add(new EpgCB(){Language = language.Code,StartDate = dateOfProgramsToIngest});
//             
//             crudOperations.ItemsToAdd.Add(new EpgProgramBulkUploadObject()
//             {
//                 GroupId = randomPartnerId, StartDate = dateOfProgramsToIngest, EpgId = (ulong)epgId, EpgCbObjects = epgCbObjects
//             });
//             
//             
//             indexManager.DeleteProgramsFromIndex(crudOperations.ItemsToDelete, setupEpgV2Index, languageObjs);
//
//             var programsToIndex = crudOperations.ItemsToAdd
//                 .Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.AffectedItems)
//                 .ToList();
//             
//             indexManager.UpsertProgramsToDraftIndex(programsToIndex, setupEpgV2Index, 
//                 dateOfProgramsToIngest, language, languageObjs);
//             
//             
//             var epgCbDocumentIdsByEpgId = indexManager.GetEpgCBDocumentIdsByEpgId(new long[] {epgId}, languageObjs.Values);
//             Assert.AreEqual(epgCbDocumentIdsByEpgId.First(),epgId.ToString(),"Expected document id and epg id to be the same");
//         }
//     }
// }