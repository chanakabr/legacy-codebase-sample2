using System;
using System.Collections.Concurrent;
using Phx.Lib.Log;
using System.Reflection;
using System.Threading;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.IndexManager;
using CachingProvider.LayeredCache;
using CanaryDeploymentManager;
using Phx.Lib.Appconfig;
using Core.Catalog.Cache;
using EventBus.Kafka;
using ElasticSearch.Common;
using Core.Catalog.CatalogManagement;
using ApiLogic.Catalog;
using ApiLogic.IndexManager;
using ApiLogic.IndexManager.Helpers;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ElasticSearch.NEST;
using ElasticSearch.Utilities;
using ApiLogic.IndexManager.QueryBuilders;
using TvinciCache.Adapters;
using ApiLogic.IndexManager.Mappings;
using Core.GroupManagers;
using ApiLogic.IndexManager.Sorting;
using ElasticSearch.Utils;
using TVinciShared;

namespace Core.Catalog
{
    public interface IIndexManagerFactory
    {
        IIndexManager GetIndexManager(int partnerId);
        bool IsV2Version(int partnerId);
    }
    
    public class IndexManagerFactory : IIndexManagerFactory
    {
        
        private static readonly Lazy<IndexManagerFactory> Lazy = new Lazy<IndexManagerFactory>(() => new IndexManagerFactory(), LazyThreadSafetyMode.PublicationOnly);

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static IndexManagerFactory Instance { get { return Lazy.Value; } }

        private ConcurrentDictionary<string, IIndexManager> _indexManagerInstance;

        private static ElasticsearchVersion? _esTestingVersion;
        
        private const string ES_TESTING_VERSION = "ES_TESTING_VERSION";

        public IndexManagerFactory()
        {
            _indexManagerInstance = new ConcurrentDictionary<string, IIndexManager>();
            _esTestingVersion = TryGetEsTestingVersion();
        }

        public IIndexManager GetIndexManager(int partnerId)
        {
            var isMigrationEventsEnabled = !_esTestingVersion.HasValue && CanaryDeploymentFactory.Instance
                .GetElasticsearchCanaryDeploymentManager()
                .IsMigrationEventsEnabled(partnerId);
            var elasticsearchActiveVersion = GetElasticsearchVersion(partnerId);
            var keyName = $"{elasticsearchActiveVersion}{partnerId}{isMigrationEventsEnabled}";

            return _indexManagerInstance.GetOrAdd(keyName,
                s => CreateIndexManager(partnerId, isMigrationEventsEnabled, elasticsearchActiveVersion));
        }

        public bool IsV2Version(int partnerId) => GetElasticsearchVersion(partnerId) == ElasticsearchVersion.ES_2_3;

        private static ElasticsearchVersion GetElasticsearchVersion(int partnerId) => _esTestingVersion
            ?? CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().GetActiveElasticsearchActiveVersion(partnerId);

        private static ElasticsearchVersion? TryGetEsTestingVersion()
        {
            var esTestingVersion = Environment.GetEnvironmentVariable(ES_TESTING_VERSION);
            if (esTestingVersion.IsNullOrEmptyOrWhiteSpace())
            {
                return null;
            }

            if (!Enum.TryParse(esTestingVersion, out ElasticsearchVersion esVersion))
            {
                var message = $"error {ES_TESTING_VERSION} value {esTestingVersion} could not be parsed to ElasticsearchVersion type";
                logger.Error(message);
                return null;
            }

            return esVersion;
        }

        private IIndexManager CreateIndexManager(int partnerId, bool isMigrationEventsEnabled,
            ElasticsearchVersion version)
        {
            switch (version)
            {
                case ElasticsearchVersion.ES_7:
                    var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
                    var indexManagerV7 = new IndexManagerV7(partnerId,
                        elasticClient,
                        ApplicationConfiguration.Current,
                        new GroupsCacheManager.GroupManager(),
                        CatalogManager.Instance,
                        ElasticSearchIndexDefinitionsNest.DefinitionInstance,
                        ChannelManager.Instance,
                        CatalogCache.Instance(), new TtlService(),
                        WatchRuleManager.Instance,
                        ChannelQueryBuilder.Instance(ElasticsearchVersion.ES_7),
                        GroupsFeatureAdapter.Instance,
                        LayeredCache.Instance,
                        NamingHelper.Instance,
                        GroupSettingsManager.Instance,
                        SortingService.Instance(ElasticsearchVersion.ES_7),
                        StartDateAssociationTagsSortStrategyV7.Instance,
                        StatisticsSortStrategyV7.Instance,
                        SortingAdapter.Instance,
                        EsSortingService.Instance(ElasticsearchVersion.ES_7),
                        UnifiedQueryBuilderInitializer.Instance(ElasticsearchVersion.ES_7),
                        RegionManager.Instance);

                    if (isMigrationEventsEnabled)
                    {
                        return new IndexManagerEventsDecorator(indexManagerV7,
                            contextProvider => KafkaPublisher.GetFromTcmConfiguration(contextProvider),
                            IndexManagerVersion.EsV7,
                            partnerId);
                    }

                    return indexManagerV7;
                    break;
                
                default:
                    var indexManagerV2 = new IndexManagerV2(partnerId,
                        ElasticSearchApi.Instance,
                        new GroupsCacheManager.GroupManager(),
                        new ESSerializerV2(),
                        CatalogManager.Instance,
                        ElasticSearchIndexDefinitions.Instance,
                        LayeredCache.Instance,
                        ChannelManager.Instance,
                        CatalogCache.Instance(),
                        WatchRuleManager.Instance,
                        ChannelQueryBuilder.Instance(ElasticsearchVersion.ES_2_3),
                        MappingTypeResolver.Instance,
                        NamingHelper.Instance,
                        GroupSettingsManager.Instance,
                        SortingService.Instance(ElasticsearchVersion.ES_2_3),
                        StartDateAssociationTagsSortStrategy.Instance,
                        StatisticsSortStrategy.Instance,
                        SortingAdapter.Instance,
                        EsSortingService.Instance(ElasticsearchVersion.ES_2_3),
                        UnifiedQueryBuilderInitializer.Instance(ElasticsearchVersion.ES_2_3),
                        RegionManager.Instance);

                    if (isMigrationEventsEnabled)
                    {
                        return new IndexManagerEventsDecorator(indexManagerV2,
                            contextProvider => KafkaPublisher.GetFromTcmConfiguration(contextProvider),
                            IndexManagerVersion.EsV2,
                            partnerId);
                    }

                    return indexManagerV2;
                    break;
            }
        }
    }
}
