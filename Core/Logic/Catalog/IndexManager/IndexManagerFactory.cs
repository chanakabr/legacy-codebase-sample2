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
            var indexManagerV2 = new IndexManagerV2(partnerId,
                new ElasticSearchApi(ApplicationConfiguration.Current),
                new GroupsCacheManager.GroupManager(),
                new ESSerializerV2(),
                CatalogManager.Instance,
                ElasticSearchIndexDefinitions.Instance,
                LayeredCache.Instance,
                ChannelManager.Instance, 
                CatalogCache.Instance());
            
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
