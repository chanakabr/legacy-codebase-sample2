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
using Utils = ElasticSearch.Common.Utils;

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

        private static int _nextPartnerId = 10000;
        private Random _random;

        #region Helpers
        private IndexManagerV7 GetIndexV7Manager(int partnerId)
        {
            return new IndexManagerV7(partnerId,
                NESTFactory.GetInstance(ApplicationConfiguration.Current),
                ApplicationConfiguration.Current
                )
                ;
        }

        private void SetupOpcPartnerMocks(int partnerId, IEnumerable<LanguageObj> languages)
        {
            _mockCatalogManager.Setup(x => x.DoesGroupUsesTemplates(partnerId)).Returns(true);

            var catalogGroupCache = new CatalogGroupCache()
            {
                LanguageMapByCode = languages.ToDictionary(x => x.Code),
                LanguageMapById = languages.ToDictionary(x => x.ID)
            };

            _mockCatalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache)).Returns(true);
        }

        private LanguageObj GetRandomLanguage()
        {
            var language = new ApiObjects.LanguageObj()
            {
                ID = _random.Next(1000) + 1,
                Code = "en",
                Name = "english",
                IsDefault = true,
            };
            return language;
        }

        private int GetRandomPartnerId()
        {
            var partnerId = _nextPartnerId++ + _random.Next(1000);
            return partnerId;
        }

        private Media GetRandomMedia(int partnerId)
        {
            var rand = _random.Next(1000);
            return new Media()
            {
                m_nIsActive = 1,
                m_nMediaID = 12345 + rand,
                m_sName = $"test media {rand}",
                m_sDescription = "test description",
                m_nGroupID = partnerId,
                m_sStartDate = DateTime.Today.AddDays(-1).ToString(Utils.ES_DATE_FORMAT),
                m_nMediaTypeID = rand + 1
            };
        }

        private Channel GetRandomChannel(int randomPartnerId)
        {
            var randomNum = _random.Next(1000) + 1;
            return new Channel()
            {
                m_nParentGroupID = randomPartnerId,
                m_nChannelID = randomNum,
                m_sName = $"some name for tests {randomNum}",
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now.AddDays(-1),
                SystemName = $"test {randomNum}",
                AssetUserRuleId = randomNum,
                m_nIsActive = 1
            };
        }

        private SocialActionStatistics GetRandomSocialActionStat(int randomPartnerId)
        {
            return new SocialActionStatistics()
            {
                GroupID = randomPartnerId,
                Date = DateTime.Now.AddDays(-1),
                MediaID = _random.Next(1000),
                Action = "like",
                Count = 10
            };
        }


        private TagValue GetRandomTag(int languageId)
        {
            return new TagValue()
            {
                tagId = _random.Next(1000) + 1,
                value = "test",
                topicId = 2,
                languageId = languageId,
                createDate = 1000,
                updateDate = 1000
            };
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
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(ElasticSearch.Common.Utils.Instance, ApplicationConfiguration.Current);

        }

        [Test]
        public void TestBasics()
        {
            var partnerId = GetRandomPartnerId();
            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();

            Assert.IsTrue(result);
        }
    }
}