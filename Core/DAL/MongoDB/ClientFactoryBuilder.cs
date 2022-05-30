using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ApiObjects.Pricing;
using Microsoft.Extensions.DependencyInjection;
using OTT.Lib.MongoDB;

namespace DAL.MongoDB
{
    public class ClientFactoryBuilder : IClientFactoryBuilder
    {
        private static readonly Lazy<IClientFactoryBuilder> LazyInstance = new Lazy<IClientFactoryBuilder>(
            () => new ClientFactoryBuilder(),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly ConcurrentDictionary<string, IMongoDbClientFactory> MongoDbClientFactories
            = new ConcurrentDictionary<string, IMongoDbClientFactory>();

        public static IClientFactoryBuilder Instance => LazyInstance.Value;


        public IMongoDbClientFactory GetClientFactory(string databaseName, IConnectionStringHelper helper)
            => GetClientFactory(databaseName, null, helper);
        
        public IMongoDbClientFactory GetClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties,
            IConnectionStringHelper helper)
            => MongoDbClientFactories.GetOrAdd(databaseName,
                dbName =>
                {
                    var configuration = new MongoDbConfiguration { ConnectionString = helper.GetConnectionString() };
                    if (collectionProperties != null)
                    {
                        configuration.CollectionProps = collectionProperties;
                    }

                    return new ServiceCollection()
                        .AddMongoDbClientFactory(configuration, dbName)
                        .BuildServiceProvider()
                        .GetService<IMongoDbClientFactory>();
                });
    }
}