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
using ApiLogic.Tests.IndexManager.helpers;
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
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(ElasticSearch.Common.Utils.Instance, ApplicationConfiguration.Current);
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
            var stat1 = IndexManagerMockDataCreator.GetRandomSocialActionStat(partnerId);
            
            var indexManager = GetIndexV7Manager(partnerId);
            var result = indexManager.SetupSocialStatisticsDataIndex();
            
            var res=indexManager.InsertSocialStatisticsData(stat1);
            Assert.True(res);
        }
    }
}