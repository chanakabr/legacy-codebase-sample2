using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;
using System.Threading;
using ApiLogic.Catalog.IndexManager;
using CachingProvider.LayeredCache;
using CanaryDeploymentManager;
using ConfigurationManager;
using Core.Catalog.Cache;
using EventBus.Kafka;
using ElasticSearch.Common;
using Core.Catalog.CatalogManagement;
using ApiLogic.Catalog;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ElasticSearch.NEST;
using ElasticSearch.Utilities;
using ApiLogic.IndexManager.QueryBuilders;
using TvinciCache.Adapters;
using ApiLogic.IndexManager.Mappings;

namespace Core.Catalog
{
    public interface IIndexManagerFactory
    {
        IIndexManager GetIndexManager(int partnerId);
    }
    
    public class IndexManagerFactory : IIndexManagerFactory
    {
        
        private static readonly Lazy<IndexManagerFactory> Lazy = new Lazy<IndexManagerFactory>(() => new IndexManagerFactory(), LazyThreadSafetyMode.PublicationOnly);

        public static IndexManagerFactory Instance { get { return Lazy.Value; } }

        private ConcurrentDictionary<string, IIndexManager> _indexManagerInstance;
        
        private IndexManagerFactory()
        {
            _indexManagerInstance = new ConcurrentDictionary<string, IIndexManager>();
        }
        
        public IIndexManager GetIndexManager(int partnerId)
        {
            var isMigrationEventsEnabled =
                CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager()
                    .IsMigrationEventsEnabled(partnerId);
            var activeElasticsearchActiveVersion = CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().
                GetActiveElasticsearchActiveVersion(partnerId);
            var keyName = $"{activeElasticsearchActiveVersion}{partnerId}{isMigrationEventsEnabled}";

            return _indexManagerInstance.GetOrAdd(keyName, s =>
            {
                return CreateIndexManager(partnerId, isMigrationEventsEnabled, activeElasticsearchActiveVersion);
            });
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
                        ChannelQueryBuilder.Instance,
                        GroupsFeatureAdapter.Instance,
                        LayeredCache.Instance);

                    if (isMigrationEventsEnabled)
                    {
                        var useRandomPartitioner = false;
                        var eventBusPublisher = KafkaPublisher.GetFromTcmConfiguration(useRandomPartitioner);
                        return new IndexManagerEventsDecorator(indexManagerV7,
                            eventBusPublisher,
                            IndexManagerVersion.EsV7,
                            partnerId);
                    }

                    return indexManagerV7;
                    break;
                default:
                    var indexManagerV2 = new IndexManagerV2(partnerId,
                        new ElasticSearchApi(ApplicationConfiguration.Current),
                        new GroupsCacheManager.GroupManager(),
                        new ESSerializerV2(),
                        CatalogManager.Instance,
                        ElasticSearchIndexDefinitions.Instance,
                        LayeredCache.Instance,
                        ChannelManager.Instance,
                        CatalogCache.Instance(),
                        WatchRuleManager.Instance,
                        ChannelQueryBuilder.Instance,
                        MappingTypeResolver.Instance);

                    if (isMigrationEventsEnabled)
                    {
                        var useRandomPartitioner = false;
                        var eventBusPublisher = KafkaPublisher.GetFromTcmConfiguration(useRandomPartitioner);
                        return new IndexManagerEventsDecorator(indexManagerV2,
                            eventBusPublisher,
                            IndexManagerVersion.EsV2,
                            partnerId);
                    }

                    return indexManagerV2;
                    break;
            }
        }
    }
}
