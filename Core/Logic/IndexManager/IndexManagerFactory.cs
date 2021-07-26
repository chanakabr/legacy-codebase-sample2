using System;
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;
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

namespace Core.Catalog
{
    public interface IIndexManagerFactory
    {
        IIndexManager GetInstance(int partnerId);
    }
    
    public class IndexManagerFactory : IIndexManagerFactory
    {
        private IndexManagerFactory(){}
        public static IIndexManager GetInstance(int partnerId)
        {
            var partnerConfiguration = CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().GetPartnerConfiguration(partnerId);
            if( partnerConfiguration.Object.ElasticsearchActiveVersion== ElasticsearchVersion.ES_7_13)
            {
                var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
                return new IndexManagerV7(partnerId, 
                    elasticClient, 
                    ApplicationConfiguration.Current,
                    new GroupsCacheManager.GroupManager(),
                    CatalogManager.Instance,
                    ElasticSearchIndexDefinitions.Instance,
                    ChannelManager.Instance,
                    CatalogCache.Instance());
            }
                
            var indexManagerV2 = new IndexManagerV2(partnerId,
                new ElasticSearchApi(ApplicationConfiguration.Current),
                new GroupsCacheManager.GroupManager(),
                new ESSerializerV2(),
                CatalogManager.Instance,
                ElasticSearchIndexDefinitions.Instance,
                LayeredCache.Instance,
                ChannelManager.Instance, 
                CatalogCache.Instance(),
                WatchRuleManager.Instance);
            
            if (CanaryDeploymentFactory.Instance.GetElasticsearchCanaryDeploymentManager().IsMigrationEventsEnabled(partnerId))
            {
                var useRandomPartitioner = false;
                var eventBusPublisher = KafkaPublisher.GetFromTcmConfiguration( useRandomPartitioner);
                return new IndexManagerEventsDecorator(indexManagerV2,
                    eventBusPublisher,
                    IndexManagerVersion.EsV2,
                    partnerId);
            }
            
            return indexManagerV2;
        }        

        public static IIndexManagerFactory GetFactory()
        {
            return new IndexManagerFactory();
        }
        
        // implemented for dependency injection (like in Topicmanager)
        IIndexManager IIndexManagerFactory.GetInstance(int partnerId)
        {
            return GetInstance(partnerId);
        }
    }
}
